using JetBrains.Annotations;

namespace Challenge.CLI;

/// <summary>
/// Solver data
/// </summary>
/// <param name="year">Solver year</param>
/// <param name="day">Solver day</param>
[PublicAPI, AttributeUsage(AttributeTargets.Class)]
public sealed class SolverAttribute(uint year, uint day) : Attribute
{
    /// <summary>
    /// Solver year
    /// </summary>
    public uint Year { get; } = year;

    /// <summary>
    /// Solver day
    /// </summary>
    public uint Day { get; } = day;

    /// <summary>
    /// Solver module
    /// </summary>
    public string Module { get; init; } = string.Empty;
}
