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
public readonly record struct SolverData(uint Year, uint Day, uint? Part, string Module)
{
    /// <summary>
    /// Creates a <see cref="SolverData"/> object from a given <see cref="SolverAttribute"/>
    /// </summary>
    /// <param name="attribute"></param>
    public SolverData(SolverAttribute attribute) :this(attribute.Year, attribute.Day, attribute.Part, attribute.Module) { }
}
