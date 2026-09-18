using DotMake.CommandLine;

namespace Challenge.CLI;

/// <summary>
/// Challenge Solver Interface
/// </summary>
[CliCommand(Description = "Challenge Solver Interface")]
public sealed class ChallengeCommand : ICliRunWithContext
{
    /// <inheritdoc />
    public void Run(CliContext cliContext)
    {
        cliContext.ShowHelp();
#if DEBUG
        cliContext.ShowHierarchy();
#endif
    }
}
