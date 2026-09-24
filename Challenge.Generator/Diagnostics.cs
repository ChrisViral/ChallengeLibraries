using Challenge.Solvers;
using Challenge.Solvers.Attributes;
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
    public static readonly DiagnosticDescriptor MissingBaseClassDescriptor =
        new("CG001",
            "Missing Solver base class",
            $"Class {{0}} is marked with {typeof(SolverAttribute).FullName}, but does not inherit from {typeof(Solver).FullName}",
            "SourceGenerator",
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    /// Invalid Solver part method signature
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidPartMethodSignatureDescriptor =
        new("CG002",
            "Invalid Solver part method signature",
            $"Method {{0}} tagged with {typeof(PartAttribute).FullName} should be parameterless",
            "SourceGenerator",
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    /// Duplicated Solver part value
    /// </summary>
    public static readonly DiagnosticDescriptor DuplicatedPartValueDescriptor =
        new("CG003",
            "Duplicated Solver part value",
            "A solver part with the same part value has already been defined in this class",
            "SourceGenerator",
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    /// Class is not partial
    /// </summary>
    public static readonly DiagnosticDescriptor ClassNotPartial =
        new("CG004",
            "Class is not partial",
            "The class {0} must be partial to allow for source generation",
            "SourceGenerator",
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    /// Solver class is abstract
    /// </summary>
    public static readonly DiagnosticDescriptor SolverClassIsAbstract =
        new("CG005",
            "Solver class is abstract",
            "The solver class {0} must not be abstract to allow instantiation by system",
            "SourceGenerator",
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    /// Method cannot be user-defined
    /// </summary>
    public static readonly DiagnosticDescriptor MethodCannotBeDefined =
        new("CG006",
            "Method cannot be user-defined",
            "The method {0} cannot be user-defined as it will be generated",
            "SourceGenerator",
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    /// Method cannot be overriden
    /// </summary>
    public static readonly DiagnosticDescriptor MethodCannotBeOverriden =
        new("CG007",
            "Method cannot be overriden",
            "The method {0} needs to be overrideable from the parent type as it will be generated",
            "SourceGenerator",
            DiagnosticSeverity.Error,
            true);

    /// <summary>
    /// Class cannot be a nested type
    /// </summary>
    public static readonly DiagnosticDescriptor IsNestedType =
        new("CG008",
            "Class cannot be a nested type",
            "The class {0} cannot be a nested type for proper code generation",
            "SourceGenerator",
            DiagnosticSeverity.Error,
            true);
}
