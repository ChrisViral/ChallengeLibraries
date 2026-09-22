using Microsoft.Extensions.Logging;

namespace Challenge.Solvers;

public partial class Solver
{
    [LoggerMessage(LogLevel.Information, "Part {Part}: {Answer}")]
    static partial void LogPartAnswer(ILogger logger, uint part, string answer);

    [LoggerMessage(LogLevel.Information, "Time: {Time}\n")]
    static partial void LogPartTime(ILogger logger, string time);

    [LoggerMessage(LogLevel.Information, "{Message}")]
    static partial void LogMessage(ILogger logger, object message);
}
