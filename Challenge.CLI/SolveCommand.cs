using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Challenge.Solvers;
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
    /// Challenge part
    /// </summary>
    [CliOption(Description = "Challenge part")]
    public uint? Part { get; set; }

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
    private string PartString => this.Part.HasValue ? $" Part {this.Part.Value}" : string.Empty;

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
    public async Task<int> RunAsync(CliContext cliContext)
    {
        // Fetch input
        LogFetchingInput(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.PartString, this.ModuleString);
        SolverData data = new(this.Year, this.Day, this.Part, this.Module);
        Result<string, Exception> fetchResult = await this.Resolver.FetchInput(data, cliContext.CancellationToken).ConfigureAwait(false);

        // Get input data
        if (!fetchResult.TryGetValue(out string? input))
        {
            LogInputFetchFailed(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.PartString, this.ModuleString, fetchResult.Error);
            return 1;
        }

        // Create solver instance
        if (!TryCreateSolver(input, data, out Solver? solver, out TimeSpan parseTime))
        {
            LogFailedCreateSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.PartString, this.ModuleString);
            return 1;
        }

        // Log input parse time
        LogInputParsed(this.Logger, parseTime.GetElapsedString());

#if DEBUG
        // In debug mode we want to break at the exception location
        using (solver)
        {
            await RunSolver(solver, data, cliContext.CancellationToken).ConfigureAwait(false);
        }
#else
        try
        {
            await RunSolver(solver, data, cliContext.CancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            //Log any exceptions that occur
            LogExceptionWhileRunningSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.PartString, this.ModuleString, e);
            return 1;
        }
        finally
        {
            solver.Dispose();
        }
#endif
        return 0;
    }

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
            LogExceptionWhileCreatingSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.PartString, this.ModuleString, e);
            solver = null;
            parseTime = TimeSpan.Zero;
            return false;
        }
    }

    /// <summary>
    /// Runs the solver
    /// </summary>
    /// <param name="solver">Solver to run</param>
    /// <param name="data">Solver data</param>
    /// <param name="token">Cancellation token</param>
    private async Task RunSolver(Solver solver, SolverData data, CancellationToken token)
    {
        solver.RunAndStartStopwatch();
        solver.LogElapsed();
        if (!this.SubmitAnswer) return;

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
}
