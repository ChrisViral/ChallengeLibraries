using CSharpFunctionalExtensions;
using DotMake.CommandLine;
using Microsoft.Extensions.Logging;

namespace Challenge.CLI;

/// <summary>
/// Solve a specific challenge instance
/// </summary>
[CliCommand(Description = "Solve a specific challenge instance", Name = "solve", Parent = typeof(ChallengeCommand))]
public sealed partial class SolveCommand(ILogger<SolveCommand> logger, IInputFetcher inputFetcher) : ICliRunAsyncWithContextAndReturn
{
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

    /// <summary>
    /// Logger instance
    /// </summary>
    private ILogger Logger { get; } = logger;

    /// <summary>
    /// Input fetcher instance
    /// </summary>
    private IInputFetcher InputFetcher { get; } = inputFetcher;

    /// <inheritdoc />
    public async Task<int> RunAsync(CliContext cliContext)
    {
        LogFetchingInput(this.Logger, this.Year, this.Day, string.IsNullOrEmpty(this.Module) ? string.Empty : $" ({this.Module})");
        Result<string> result = await this.InputFetcher.Fetch(this.Year, this.Day, this.Module, cliContext.CancellationToken).ConfigureAwait(false);
        if (!result.TryGetValue(out string? data))
        {
            LogInputFetchFailed(this.Logger, this.Year, this.Day, string.IsNullOrEmpty(this.Module) ? string.Empty : $" ({this.Module})", result.Error);
            return 1;
        }

        return 0;
    }
}
