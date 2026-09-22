using Challenge.Solvers;
using Microsoft.CodeAnalysis;

namespace Challenge.Generator;

/// <summary>
/// Diagnostics definitions
/// </summary>
internal static class Diagnostics
{
    /// <summary>
    /// Missing Solver base class
    /// </summary>
    public static DiagnosticDescriptor MissingBaseClassDescriptor { get; } =
        new("CG001",
            "Missing Solver base class",
            $"Class {{0}} is marked with {typeof(SolverAttribute).FullName}, but does not inherit from {typeof(Solver).FullName}",
            "SourceGenerator",
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    /// Invalid Solver part method signature
    /// </summary>
    public static DiagnosticDescriptor InvalidPartMethodSignatureDescriptor { get; } =
        new("CG002",
            "Invalid Solver part method signature",
            $"Method {{0}} tagged with {typeof(PartAttribute).FullName} should be parameterless",
            "SourceGenerator",
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    /// Duplicated Solver part value
    /// </summary>
    public static DiagnosticDescriptor DuplicatedPartValueDescriptor { get; } =
        new("CG003",
            "Duplicated Solver part value",
            "A solver part with the same part value has already been defined in this class",
            "SourceGenerator",
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    /// Solver class is not partial
    /// </summary>
    public static DiagnosticDescriptor SolverClassNotPartial { get; } =
        new("CG004",
            "Solver class is not partial",
            "The solver class {0} must be partial to allow for source generation when using parts",
            "SourceGenerator",
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    /// Solver class is abstract
    /// </summary>
    public static DiagnosticDescriptor SolverClassIsAbstract { get; } =
        new("CG005",
            "Solver class is abstract",
            "The solver class {0} must not be abstract to allow instantiation by system",
            "SourceGenerator",
            DiagnosticSeverity.Error,
            true);
}
