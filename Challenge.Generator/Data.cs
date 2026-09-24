using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Challenge.Generator;

/// <summary>
/// Solver part method data
/// </summary>
/// <param name="Name">Method name</param>
/// <param name="Part">Method part</param>
internal readonly record struct PartMethod(string Name, uint Part);

/// <summary>
/// Method data
/// </summary>
/// <param name="Node">Method syntax node</param>
/// <param name="Symbol">Method symbol</param>
internal sealed record MethodData(MethodDeclarationSyntax Node, IMethodSymbol Symbol);

/// <summary>
/// Part method info
/// </summary>
/// <param name="MethodNode">Method node</param>
/// <param name="MethodSymbol">Method symbol</param>
/// <param name="Part">Method part</param>
/// <param name="IsInvalidPartDeclaration">Method symbol</param>
internal sealed record PartMethodInfo(MethodDeclarationSyntax MethodNode,
                                      IMethodSymbol MethodSymbol,
                                      uint Part,
                                      bool IsInvalidPartDeclaration = false);

/// <summary>
/// Solver info
/// </summary>
/// <param name="ClassNode">Solver class node</param>
/// <param name="ClassSymbol">Solver class symbol</param>
/// <param name="PartMethods">Solver part methods</param>
/// <param name="Year">Solver year</param>
/// <param name="Day">Solver part</param>
/// <param name="IsNestedType">If the class is a nested type</param>
/// <param name="IsNotMarkedPartial">If the class isn't marked as partial</param>
/// <param name="IsMarkedAbstract">If the class is marked as abstract</param>
/// <param name="IsMissingConstructor">If the class is missing it's required constructor</param>
/// <param name="IsMissingBaseClass">If the Solver base class is missing</param>
/// <param name="PartRunMethod">The Part Run method override, if found</param>
internal sealed record SolverInfo(ClassDeclarationSyntax ClassNode,
                                  INamedTypeSymbol ClassSymbol,
                                  IReadOnlyList<PartMethodInfo> PartMethods,
                                  uint Year, uint Day,
                                  bool IsNestedType = false,
                                  bool IsNotMarkedPartial = false,
                                  bool IsMarkedAbstract = false,
                                  bool IsMissingConstructor = false,
                                  bool IsMissingBaseClass = false,
                                  MethodData? PartRunMethod = null)
{
    /// <summary>
    /// Handles all class-level diagnostics
    /// </summary>
    /// <param name="context">Source generation context</param>
    /// <returns><see langword="true"/> if a diagnostic has been emitted or generation is not needed, otherwise <see langword="false"/></returns>
    public bool HandleClassDiagnostics(SourceProductionContext context)
    {
        // Diagnostic of class is a nested type
        if (this.IsNestedType)
        {
            PostDiagnostic(context, Diagnostics.IsNestedType);
            return true;
        }

        // Diagnostic if base class is missing
        if (this.IsMissingBaseClass)
        {
            PostDiagnostic(context, Diagnostics.MissingBaseClassDescriptor);
            return true;
        }

        // Diagnostic if class is marked abstract
        if (this.IsMarkedAbstract)
        {
            PostDiagnostic(context, Diagnostics.SolverClassIsAbstract);
            return true;
        }

        // Ignore if no methods or constructor to generate
        if (this.PartMethods.Count is 0 && !this.IsMissingConstructor) return true;

        // Diagnostic if not marked as partial
        if (this.IsNotMarkedPartial)
        {
            PostDiagnostic(context, Diagnostics.ClassNotPartial);
            return true;
        }

        if (this.PartMethods.Count is not 0 && this.PartRunMethod is not null)
        {
            Diagnostic diagnostic = Diagnostic.Create(Diagnostics.MethodCannotBeDefined,
                                                      this.PartRunMethod.Node.Identifier.GetLocation(),
                                                      this.PartRunMethod.Symbol.Name);
            context.ReportDiagnostic(diagnostic);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Posts the given diagnostic
    /// </summary>
    /// <param name="context">Source generation context</param>
    /// <param name="descriptor">Diagnostic descriptor</param>
    private void PostDiagnostic(SourceProductionContext context, DiagnosticDescriptor descriptor)
    {
        Diagnostic diagnostic = Diagnostic.Create(descriptor,
                                                  this.ClassNode.Identifier.GetLocation(),
                                                  this.ClassSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }
}

/// <summary>
/// Solver Table info
/// </summary>
/// <param name="ClassNode">Solver Table class node</param>
/// <param name="ClassSymbol">Solver Table class symbol</param>
/// <param name="ExistingGetSolverMethod">The existing GetSolver method, if any</param>
/// <param name="IsNestedType">If the class is a nested type</param>
/// <param name="IsNotMarkedPartial">If the class isn't marked as partial</param>
internal sealed record SolverTableInfo(ClassDeclarationSyntax ClassNode,
                                       INamedTypeSymbol ClassSymbol,
                                       IMethodSymbol? ExistingGetSolverMethod,
                                       bool IsNestedType = false,
                                       bool IsNotMarkedPartial = false)
{
    /// <summary>
    /// Handles all class-level diagnostics
    /// </summary>
    /// <param name="context">Source generation context</param>
    /// <returns><see langword="true"/> if a diagnostic has been emitted or generation is not needed, otherwise <see langword="false"/></returns>
    public bool HandleClassDiagnostics(SourceProductionContext context)
    {
        // Diagnostic of class is a nested type
        if (this.IsNestedType)
        {
            PostDiagnostic(context, Diagnostics.IsNestedType);
            return true;
        }

        // Diagnostic if class is marked abstract
        if (this.IsNotMarkedPartial)
        {
            PostDiagnostic(context, Diagnostics.ClassNotPartial);
            return true;
        }

        if (this.ExistingGetSolverMethod is null) return false;

        // Diagnostic if existing solver method is defined in the target class
        if (SymbolEqualityComparer.Default.Equals(this.ExistingGetSolverMethod.ContainingType, this.ClassSymbol))
        {
            MethodDeclarationSyntax methodSyntaxNode = (MethodDeclarationSyntax)this.ExistingGetSolverMethod.DeclaringSyntaxReferences.First().GetSyntax();
            Diagnostic diagnostic = Diagnostic.Create(Diagnostics.MethodCannotBeOverriden,
                                                      methodSyntaxNode.Identifier.GetLocation(),
                                                      this.ExistingGetSolverMethod.Name);
            context.ReportDiagnostic(diagnostic);
            return true;
        }

        // Diagnostic if the method is sealed in the parent
        if (this.ExistingGetSolverMethod is { IsOverride: true, IsSealed: true } or { IsOverride: false, IsVirtual: false, IsAbstract: false })
        {
            PostDiagnostic(context, Diagnostics.MethodCannotBeOverriden);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Posts the given diagnostic
    /// </summary>
    /// <param name="context">Source generation context</param>
    /// <param name="descriptor">Diagnostic descriptor</param>
    private void PostDiagnostic(SourceProductionContext context, DiagnosticDescriptor descriptor)
    {
        Diagnostic diagnostic = Diagnostic.Create(descriptor,
                                                  this.ClassNode.Identifier.GetLocation(),
                                                  this.ClassSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }
}
