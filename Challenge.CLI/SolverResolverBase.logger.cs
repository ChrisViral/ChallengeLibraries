using Microsoft.Extensions.Logging;

namespace Challenge.CLI;

public partial class SolverResolverBase<T>
{
    [LoggerMessage(LogLevel.Error, "Only {Seconds:F0} seconds elapsed since last request, please wait at least {RateLimit:F0} seconds")]
    static partial void LogRateLimited(ILogger logger, double seconds, double rateLimit);

    [LoggerMessage(LogLevel.Information, "Cached input fetched from {FileName}")]
    static partial void LogCachedInputLoaded(ILogger logger, string fileName);
}
