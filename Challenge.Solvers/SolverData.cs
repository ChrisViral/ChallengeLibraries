using JetBrains.Annotations;

namespace Challenge.Solvers;

/// <summary>
/// Solver data object
/// </summary>
/// <param name="Year">Solver year</param>
/// <param name="Day">Solver day</param>
/// <param name="Part">Solver part</param>
/// <param name="Module">Solver module</param>
[PublicAPI]
public readonly record struct SolverData(uint Year, uint Day, uint? Part, string Module);
