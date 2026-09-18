using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Challenge.Solvers;
using Challenge.Utils;
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
    public int Year { get; set; }

    /// <summary>
    /// Challenge day
    /// </summary>
    [CliArgument(Description = "Challenge day")]
    public int Day { get; set; }

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
        string solverFullName = this.Resolver.GetSolverFullName(this.Year, this.Day, this.Module);
        if (!TryCreateSolver(input, solverFullName, out Solver? solver, out Stopwatch? parseWatch))
        {
            LogFailedCreateSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.ModuleString);
            return 1;
        }

        // Log input parse time
        LogInputParsed(this.Logger, ChallengeUtils.GetElapsedString(parseWatch.Elapsed));

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
            LogExceptionWhileRunningSolver(this.Logger, solverFullName, this.Resolver.ChallengeName, this.Year, this.Day, this.ModuleString, e);
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

    private bool TryCreateSolver(string input, string solverFullName, [NotNullWhen(true)] out Solver? solver, [NotNullWhen(true)] out Stopwatch? parseWatch)
    {
        try
        {
            // Get solver type
            Type? solverType = AppDomain.CurrentDomain
                                        .GetAssemblies()
                                        .SelectMany(a => a.GetTypes())
                                        .Where(t => t is { IsAbstract: false, IsGenericType: false }
                                                 && t.IsAssignableTo(BaseSolverType)
                                                 && t.GetConstructor(ConstructorParamTypes) is not null)
                                       .SingleOrDefault(t => t.FullName == solverFullName);
            // Check type
            if (solverType is null)
            {
                solver = null;
                parseWatch = null;
                return false;
            }

            // Insantiate solver
            parseWatch = Stopwatch.StartNew();
            solver = Activator.CreateInstance(solverType, input, this.loggerFactory.CreateLogger(solverType)) as Solver;
            parseWatch.Stop();
            return solver is not null;
        }
        catch (Exception e)
        {
            // Log exceptions
            LogExceptionWhileCreatingSolver(this.Logger, solverFullName, this.Resolver.ChallengeName, this.Year, this.Day, this.ModuleString, e);
            solver = null;
            parseWatch = null;
            return false;
        }
    }
}
