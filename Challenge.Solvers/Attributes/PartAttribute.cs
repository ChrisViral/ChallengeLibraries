using JetBrains.Annotations;

namespace Challenge.Solvers.Attributes;

/// <summary>
/// Solver part attribute
/// </summary>
/// <param name="part">Part for this method</param>
[PublicAPI, AttributeUsage(AttributeTargets.Method, Inherited = false), MeansImplicitUse(ImplicitUseTargetFlags.Itself)]
public sealed partial class PartAttribute(uint part) : Attribute
{
    /// <summary>
    /// Part for this method
    /// </summary>
    public uint Part { get; init; } = part;
}
