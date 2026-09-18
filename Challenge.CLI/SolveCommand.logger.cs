using Microsoft.Extensions.Logging;

namespace Challenge.CLI
{
    public partial class SolveCommand
    {
        [LoggerMessage(LogLevel.Error, "Could not fetch input for Challenge {Year} {Day}{Module}: {Error}")]
        static partial void LogInputFetchFailed(ILogger logger, int year, int day, string module, string error);

        [LoggerMessage(LogLevel.Information, "Fetching input for for Challenge {Year} {Day}{Module}")]
        static partial void LogFetchingInput(ILogger logger, int year, int day, string module);
    }
}
