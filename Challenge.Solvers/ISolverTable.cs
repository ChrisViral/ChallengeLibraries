using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Challenge.Solvers;

/// <summary>
/// Solver Table interface
/// </summary>
[PublicAPI]
public interface ISolverTable
{
    /// <summary>
    /// Gets the solver for the given year/day combination
    /// </summary>
    /// <param name="year">Solver year</param>
    /// <param name="day">Solver day</param>
    /// <param name="logger">Logger instance</param>
    /// <returns>The found and instantiated solver</returns>
    /// <exception cref="System.InvalidOperationException">If the solver for the given <paramref name="year"/> and <paramref name="day"/> was not found</exception>
    Solver Get(uint year, uint day, ILogger logger);
}
