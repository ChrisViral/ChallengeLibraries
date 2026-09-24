using System.Diagnostics.CodeAnalysis;
using Challenge.Solvers;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.TimeSpans;
using CSharpFunctionalExtensions;
using DotMake.CommandLine;
using Microsoft.Extensions.Logging;

namespace Challenge.CLI;

/// <summary>
/// Solve a specific challenge instance
/// </summary>
[CliCommand(Description = "Solve a specific challenge instance", Name = "solve", Parent = typeof(ChallengeCommand))]
public sealed partial class SolveCommand(ILoggerFactory loggerFactory, ISolverResolver resolver) : ICliRunAsyncWithContextAndReturn
{
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
        // Load solver
        if (!TryLoadSolver(out Solver? solver))
        {
            LogFailedCreateSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.ModuleString);
            return 1;
        }

        using (solver)
        {
            // Fetch input
            LogFetchingInput(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.PartsString, this.ModuleString);

            List<(SolverData, string)> inputs;
            if (this.Parts.IsEmpty)
            {
                inputs = new List<(SolverData, string)>(1);
                (SolverData, string input) input = await GetInput(null, cliContext.CancellationToken).ConfigureAwait(false);
                if (string.IsNullOrEmpty(input.input)) return 1;

                inputs.Add(input);
            }
            else
            {
                inputs = new List<(SolverData, string)>(this.Parts.Length);
                foreach (uint part in this.Parts)
                {
                    (SolverData, string input) input = await GetInput(part, cliContext.CancellationToken).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(input.input)) break;

                    inputs.Add(input);
                }

                if (inputs.IsEmpty) return 1;
            }

            LogRunningSolvers(this.Logger);
            return await RunAllSolvers(solver, inputs, cliContext.CancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Get the input for a given part
    /// </summary>
    /// <param name="part">Solver part</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>A tuple containing the solver data and loaded solver</returns>
    private async Task<(SolverData, string)> GetInput(uint? part, CancellationToken token)
    {
        // Get input data
        SolverData data = new(this.Year, this.Day, part, this.Module);
        Result<string> fetchResult = await this.Resolver.FetchInput(data, token).ConfigureAwait(false);

        // Get input data
        if (!fetchResult.TryGetValue(out string? input))
        {
            LogInputFetchFailed(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, GetPartString(data.Part), this.ModuleString, fetchResult.Error);
            return (default, string.Empty);
        }
        return (data, input);
    }

    /// <summary>
    /// Tries to load the solver for the current year/day
    /// </summary>
    /// <param name="solver">Loaded solver</param>
    /// <returns><see langword="true"/> if the solver was loaded, otherwise <see langword="false"/></returns>
    private bool TryLoadSolver([NotNullWhen(true)] out Solver? solver)
    {
        try
        {
            // Instantiate solver
            LogInstantiatingSolver(this.Logger, this.Year, this.Day);
            solver = this.Resolver.GetSolver(this.Year, this.Day);
        }
        catch (Exception e)
        {
            // Log exceptions
            LogExceptionWhileCreatingSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, this.ModuleString, e);
            solver = null;
            return false;
        }

        if (solver is null) return false;

        solver.Logger = this.loggerFactory.CreateLogger(solver.GetType());
        LogSolverLoaded(this.Logger, solver.GetType().FullName!);
        return true;

    }

    /// <summary>
    /// Runs all solvers
    /// </summary>
    /// <param name="solver">Solver to run</param>
    /// <param name="inputs">Solver inputs</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Program return code</returns>
    private async Task<int> RunAllSolvers(Solver solver, List<(SolverData data, string input)> inputs, CancellationToken token)
    {
#if DEBUG
        if (inputs.Count is 1)
        {
            // In debug mode we want to break at the exception location
            LogRunSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, string.Empty, this.Module);
            solver.ParseInput(inputs[0].input);
            LogInputParsed(this.Logger, solver.ParseTime.GetElapsedString());
            solver.RunAndStartStopwatch();
        }
        else
        {
            foreach ((SolverData data, string input) in inputs)
            {
                // In debug mode we want to break at the exception location
                LogRunSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, GetPartString(data.Part), this.Module);
                solver.ParseInput(input);
                LogInputParsed(this.Logger, solver.ParseTime.GetElapsedString());
                solver.RunAndStartStopwatch(data.Part!.Value);
            }
        }
#else
        uint? currentPart = null;
        try
        {
            if (inputs.Count is 1)
            {
                // In debug mode we want to break at the exception location
                LogRunSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, string.Empty, this.Module);
                solver.ParseInput(inputs[0].input);
                LogInputParsed(this.Logger, solver.ParseTime.GetElapsedString());
                solver.RunAndStartStopwatch();
            }
            else
            {
                foreach ((SolverData data, string input) in inputs)
                {
                    // In debug mode we want to break at the exception location
                    LogRunSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, GetPartString(data.Part), this.Module);
                    solver.ParseInput(input);
                    LogInputParsed(this.Logger, solver.ParseTime.GetElapsedString());
                    solver.RunAndStartStopwatch(data.Part!.Value);
                }
            }
        }
        catch (Exception e)
        {
            //Log any exceptions that occur
            LogExceptionWhileRunningSolver(this.Logger, this.Resolver.ChallengeName, this.Year, this.Day, GetPartString(currentPart), this.ModuleString, e);
            return 1;
        }
#endif

        LogElapsed(this.Logger, solver.SolveTime.GetElapsedString());

        if (this.SubmitAnswer)
        {
            (SolverData data, _) = inputs[^1];
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

        return 0;
    }

    /// <summary>
    /// Challenge part string representation
    /// </summary>
    /// <param name="part">Challenge part</param>
    /// <returns>The string representing this part</returns>
    private static string GetPartString(uint? part) => part.HasValue ? $" Part {part.Value}" : string.Empty;
}
