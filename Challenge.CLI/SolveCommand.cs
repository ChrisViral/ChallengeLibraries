using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Challenge.Solvers;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Enumerables;
using Challenge.Utils.Extensions.TimeSpans;
using CSharpFunctionalExtensions;
using DotMake.CommandLine;
using Microsoft.Extensions.Logging;
using ZLinq;

namespace Challenge.CLI;

/// <summary>
/// Solve a specific challenge instance
/// </summary>
[CliCommand(Description = "Solve a specific challenge instance", Name = "solve", Parent = typeof(ChallengeCommand))]
public sealed partial class SolveCommand(ILoggerFactory loggerFactory, ISolverResolver resolver) : ICliRunAsyncWithContextAndReturn
{
    /// <summary> Solver type </summary>
    private static readonly Type BaseSolverType = typeof(Solver);
    /// <summary> Solver constructor parameter types </summary>
    private static readonly Type[] ConstructorParamTypes = [typeof(string), typeof(ILogger)];

    private readonly ILoggerFactory loggerFactory = loggerFactory;

    /// <summary>
    /// Challenge year
    /// </summary>
    [CliArgument(Description = "Challenge year")]
    public uint Year { get; set; }

    /// <summary>
    /// Challenge day
    /// </summary>
    [CliArgument(Description = "Challenge day")]
    public uint Day { get; set; }

    /// <summary>
    /// Challenge parts
    /// </summary>
    [CliOption(Description = "Challenge part", AllowMultipleArgumentsPerToken = true)]
    public uint[] Parts { get; set; } = [];

    /// <summary>
    /// Challenge module
    /// </summary>
    [CliOption(Description = "Challenge module")]
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// If the answer should be submitted or not
    /// </summary>
    [CliOption(Description = "If the answer should be submitted or not")]
    public bool SubmitAnswer { get; set; }

    /// <summary>
    /// Challenge part string representation
    /// </summary>
    private string PartsString => !this.Parts.IsEmpty ? $" Parts {string.Join(", ", this.Parts)}" : string.Empty;

    /// <summary>
    /// Module name string representation
    /// </summary>
    private string ModuleString => !string.IsNullOrEmpty(this.Module) ? $" ({this.Module})" : string.Empty;

    /// <summary>
    /// Logger instance
    /// </summary>
    private ILogger Logger { get; } = loggerFactory.CreateLogger<SolveCommand>();

    /// <summary>
    /// Input fetcher instance
    /// </summary>
    private ISolverResolver Resolver { get; } = resolver;

    /// <inheritdoc />
    /// ReSharper disable once CognitiveComplexity
    public async Task<int> RunAsync(CliContext cliContext)
    {
        // Fetch input
        LogFetchingInput(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.PartsString, this.ModuleString);

        List<(SolverData data, Solver solver)> solvers;
        if (this.Parts.IsEmpty)
        {
            solvers = new List<(SolverData, Solver)>(1);
            (SolverData data, Solver? solver) fetched = await GetSolver(null, cliContext.CancellationToken).ConfigureAwait(false);
            if (fetched.solver is null) return 1;

            solvers.Add(fetched!);
        }
        else
        {
            solvers = new List<(SolverData, Solver)>(this.Parts.Length);
            foreach (uint part in this.Parts)
            {
                (SolverData data, Solver? solver) fetched = await GetSolver(part, cliContext.CancellationToken).ConfigureAwait(false);
                if (fetched.solver is null) break;

                solvers.Add(fetched!);
            }

            if (solvers.IsEmpty) return 1;
        }

        return await RunAllSolvers(solvers, cliContext.CancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get the solver for the given part
    /// </summary>
    /// <param name="part">Solver part</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>A tuple containing the solver data and loaded solver</returns>
    private async Task<(SolverData, Solver?)> GetSolver(uint? part, CancellationToken token)
    {
        // Get input data
        SolverData data = new(this.Year, this.Day, part, this.Module);
        Result<string, Exception> fetchResult = await this.Resolver.FetchInput(data, token).ConfigureAwait(false);

        // Get input data
        if (!fetchResult.TryGetValue(out string? input))
        {
            LogInputFetchFailed(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, GetPartString(data.Part), this.ModuleString, fetchResult.Error);
            return (default, null);
        }

        // Create solver instance
        if (!TryCreateSolver(input, data, out Solver? solver, out TimeSpan parseTime))
        {
            LogFailedCreateSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, GetPartString(data.Part), this.ModuleString);
            return (default, null);
        }

        // Log input parse time
        LogInputParsed(this.Logger, parseTime.GetElapsedString());
        return (data, solver);
    }

    /// <summary>
    /// Tries to create
    /// </summary>
    /// <param name="input"></param>
    /// <param name="data"></param>
    /// <param name="solver"></param>
    /// <param name="parseTime"></param>
    /// <returns></returns>
    private bool TryCreateSolver(string input, SolverData data, [NotNullWhen(true)] out Solver? solver, out TimeSpan parseTime)
    {
        try
        {
            // Get solver type
            Type? solverType = AppDomain.CurrentDomain
                                        .GetAssemblies()
                                        .SelectMany(a => a.GetTypes())
                                        .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericType: false }
                                                 && t.IsAssignableTo(BaseSolverType)
                                                 && t.GetConstructor(ConstructorParamTypes) is not null)
                                        .Select(t => (type: t, attribute: t.GetCustomAttribute<SolverAttribute>()))
                                        .SingleOrDefault(t => t.attribute is not null
                                                           && data == new SolverData(t.attribute))
                                        .type;
            // Check type
            if (solverType is null)
            {
                solver = null;
                parseTime = TimeSpan.Zero;
                return false;
            }

            // Instantiate solver
            LogInstantiatingSolver(this.Logger, solverType.FullName ?? string.Empty);
            Stopwatch parseWatch = Stopwatch.StartNew();
            solver = Activator.CreateInstance(solverType, input, this.loggerFactory.CreateLogger(solverType)) as Solver;
            parseWatch.Stop();
            parseTime = parseWatch.Elapsed;
            return solver is not null;
        }
        catch (Exception e)
        {
            // Log exceptions
            LogExceptionWhileCreatingSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, GetPartString(data.Part), this.ModuleString, e);
            solver = null;
            parseTime = TimeSpan.Zero;
            return false;
        }
    }

    /// <summary>
    /// Runs all solvers
    /// </summary>
    /// <param name="solvers">Solvers to run</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Program return code</returns>
    private async Task<int> RunAllSolvers(IReadOnlyList<(SolverData data, Solver solver)> solvers, CancellationToken token)
    {
#if DEBUG
        foreach ((SolverData _, Solver solver) in solvers)
        {
            // In debug mode we want to break at the exception location
            solver.RunAndStartStopwatch();
            solver.LogElapsed();
        }
#else
        foreach ((SolverData data, Solver solver) in solvers)
        {
            try
            {
                solver.RunAndStartStopwatch();
                solver.LogElapsed();
            }
            catch (Exception e)
            {
                //Log any exceptions that occur
                LogExceptionWhileRunningSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, GetPartString(data.Part), this.ModuleString, e);
                return 1;
            }
        }
#endif

        if (this.SubmitAnswer)
        {
            (SolverData data, Solver solver) = solvers[^1];
            Result submitResult = await this.Resolver.SubmitAnswer(solver.LastAnswer, data, token).ConfigureAwait(false);
            if (!submitResult.TryGetError(out string? error))
            {
                LogCorrectAnswer(this.Logger);
            }
            else
            {
                LogIncorrectAnswer(this.Logger, error);
            }
        }

        solvers.ForEach(d => d.solver.Dispose());
        return 0;
    }

    /// <summary>
    /// Challenge part string representation
    /// </summary>
    /// <param name="part">Challenge part</param>
    /// <returns>The string representing this part</returns>
    private static string GetPartString(uint? part) => part.HasValue ? $" Part {part.Value}" : string.Empty;
}
