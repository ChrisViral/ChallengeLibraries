using JetBrains.Annotations;

namespace Challenge.Solvers.Attributes;

/// <summary>
/// Solver Table generation markup attribute
/// </summary>
[PublicAPI, AttributeUsage(AttributeTargets.Class), MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
public sealed partial class SolverTableAttribute : Attribute;
