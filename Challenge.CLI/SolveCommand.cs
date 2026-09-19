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
    private static readonly Type BaseSolverType = typeof(Solver);
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
    /// Challenge module
    /// </summary>
    [CliOption(Description = "Challenge module")]
    public string Module { get; set; } = string.Empty;

    private string ModuleString => string.IsNullOrEmpty(this.Module) ? string.Empty : $" ({this.Module})";

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
        LogFetchingInput(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.ModuleString);
        Result<string> result = await this.Resolver.FetchInput(this.Year, this.Day, this.Module, cliContext.CancellationToken).ConfigureAwait(false);

        // Get input data
        if (!result.TryGetValue(out string? input))
        {
            LogInputFetchFailed(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.ModuleString, result.Error);
            return 1;
        }

        // Create solver instance
        if (!TryCreateSolver(input, out Solver? solver, out TimeSpan parseTime))
        {
            LogFailedCreateSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.ModuleString);
            return 1;
        }

        // Log input parse time
        LogInputParsed(this.Logger, parseTime.GetElapsedString());

#if !DEBUG
        // In debug mode we want to break at the exception location
        using (solver)
        {
            solver.RunAndStartStopwatch();
            solver.LogElapsed();
        }
#else
        try
        {
            //Run solver
            solver.RunAndStartStopwatch();
        }
        catch (Exception e)
        {
            //Log any exceptions that occur
            LogExceptionWhileRunningSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.ModuleString, e);
            return 1;
        }
        finally
        {
            solver.LogElapsed();
            solver.Dispose();
        }
#endif
        return 0;
    }

    private bool TryCreateSolver(string input, [NotNullWhen(true)] out Solver? solver, out TimeSpan parseTime)
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
                                        .SingleOrDefault(d => d.attribute?.Year == this.Year
                                                           && d.attribute.Day == this.Day)
                                        .type;
            // Check type
            if (solverType is null)
            {
                solver = null;
                parseTime = TimeSpan.Zero;
                return false;
            }

            // Insantiate solver
            Stopwatch parseWatch = Stopwatch.StartNew();
            solver = Activator.CreateInstance(solverType, input, this.loggerFactory.CreateLogger(solverType)) as Solver;
            parseWatch.Stop();
            parseTime = parseWatch.Elapsed;
            return solver is not null;
        }
        catch (Exception e)
        {
            // Log exceptions
            LogExceptionWhileCreatingSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.ModuleString, e);
            solver = null;
            parseTime = TimeSpan.Zero;
            return false;
        }
    }
}
