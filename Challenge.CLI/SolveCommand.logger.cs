using Microsoft.Extensions.Logging;

namespace Challenge.CLI;

public partial class SolveCommand
{
    [LoggerMessage(LogLevel.Information, "Fetching input for for {Challenge} {Year} {Day}{Part}{Module}")]
    static partial void LogFetchingInput(ILogger logger, string challenge, uint year, uint day, string part, string module);

    [LoggerMessage(LogLevel.Error, "Could not fetch input for {Challenge} {Year} {Day}{Part}{Module}")]
    static partial void LogInputFetchFailed(ILogger logger, string challenge, uint year, uint day, string part, string module, Exception exception);

    [LoggerMessage(LogLevel.Error, "Failed to create the solver for {Challenge} {Year} {Day}{Part}{Module}")]
    static partial void LogFailedCreateSolver(ILogger logger, string challenge, uint year, uint day, string part, string module);

    [LoggerMessage(LogLevel.Error, "Running solver for {Challenge} {Year} {Day}{Part}{Module}")]
    static partial void LogRunSolver(ILogger logger, string challenge, uint year, uint day, string part, string module);

    [LoggerMessage(LogLevel.Error, "Encountered exception while creating solver for {Challenge} {Year} {Day}{Part}{Module}")]
    static partial void LogExceptionWhileCreatingSolver(ILogger logger, string challenge, uint year, uint day, string part, string module, Exception exception);

    [LoggerMessage(LogLevel.Error, "Encountered exception while running solver for {Challenge} {Year} {Day}{Part}{Module}")]
    static partial void LogExceptionWhileRunningSolver(ILogger logger, string challenge, uint year, uint day, string part, string module, Exception exception);

    [LoggerMessage(LogLevel.Information, "Problem input parsed in: {Elapsed}\n")]
    static partial void LogInputParsed(ILogger logger, string elapsed);

    [LoggerMessage(LogLevel.Information, "Correct answer!")]
    static partial void LogCorrectAnswer(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Submitted answer is incorrect:\n{ErrorMessage}")]
    static partial void LogIncorrectAnswer(ILogger logger, string errorMessage);

    [LoggerMessage(LogLevel.Information, "Instantiating solver {Type}")]
    static partial void LogInstantiatingSolver(ILogger logger, string type);
}
