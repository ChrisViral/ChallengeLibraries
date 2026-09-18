using Microsoft.Extensions.Logging;

namespace Challenge.CLI;

public partial class SolveCommand
{
    [LoggerMessage(LogLevel.Information, "Fetching input for for {Challenge} {Year} {Day}{Module}")]
    static partial void LogFetchingInput(ILogger logger, string challenge, int year, int day, string module);

    [LoggerMessage(LogLevel.Error, "Could not fetch input for {Challenge} {Year} {Day}{Module}: {Error}")]
    static partial void LogInputFetchFailed(ILogger logger, string challenge, int year, int day, string module, string error);

    [LoggerMessage(LogLevel.Error, "Failed to create the solver for {Challenge} {Year} {Day}{Module}")]
    static partial void LogFailedCreateSolver(ILogger logger, string challenge, int year, int day, string module);

    [LoggerMessage(LogLevel.Error, "Running solver for {Challenge} {Year} {Day}{Module}")]
    static partial void LogRunSolver(ILogger logger, string challenge, int year, int day, string module);

    [LoggerMessage(LogLevel.Error, "Encountered exception while creating solver {Name} for {Challenge} {Year} {Day}{Module}")]
    static partial void LogExceptionWhileCreatingSolver(ILogger logger, string name, string challenge, int year, int day, string module, Exception exception);

    [LoggerMessage(LogLevel.Error, "Encountered exception while running solver {Name} for {Challenge} {Year} {Day}{Module}")]
    static partial void LogExceptionWhileRunningSolver(ILogger logger, string name, string challenge, int year, int day, string module, Exception exception);

    [LoggerMessage(LogLevel.Information, "Problem input parsed in: {Elapsed}\n")]
    static partial void LogInputParsed(ILogger logger, string elapsed);
}
