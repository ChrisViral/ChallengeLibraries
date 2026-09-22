using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Challenge.Generator.Attributes;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace Challenge.Generator;

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
                                      bool IsInvalidPartDeclaration);

/// <summary>
/// Solver info
/// </summary>
/// <param name="ClassNode">Solver class node</param>
/// <param name="ClassSymbol">Solver class symbol</param>
/// <param name="PartMethods">Solver part methods</param>
/// <param name="IsNotMarkedPartial">If the class isn't marked as partial</param>
/// <param name="IsMissingBaseClass">If the Solver base class is missing</param>
internal sealed record SolverInfo(ClassDeclarationSyntax ClassNode,
                                  INamedTypeSymbol ClassSymbol,
                                  IReadOnlyList<PartMethodInfo> PartMethods,
                                  bool IsNotMarkedPartial = false,
                                  bool IsMissingBaseClass = false);

/// <summary>
/// Solver part method data
/// </summary>
/// <param name="Name">Method name</param>
/// <param name="Part">Method part</param>
[UsedImplicitly]
internal readonly record struct PartMethod(string Name, uint Part);

/// <summary>
/// A sample source generator that creates a custom report based on class properties. The target class should be annotated with the 'Generators.ReportAttribute' attribute.
/// When using the source code as a baseline, an incremental source generator is preferable because it reduces the performance overhead.
/// </summary>
[Generator]
public sealed class SolverRunPartsGenerator : IIncrementalGenerator
{
    private const string SOLVER_BASE_TYPE_NAME = "Challenge.Solvers.Solver";

    private static readonly DiagnosticDescriptor MissingBaseClassDescriptor = new("CG001",
                                                                                  "Missing Solver base class",
                                                                                  $"Class {{0}} is marked with {typeof(SolverAttribute).FullName}, but does not inherit from {SOLVER_BASE_TYPE_NAME}",
                                                                                  "SourceGenerator",
                                                                                  DiagnosticSeverity.Error,
                                                                                  true);

    private static readonly DiagnosticDescriptor InvalidPartMethodSignatureDescriptor = new("CG002",
                                                                                            "Invalid Solver part method signature",
                                                                                            $"Methods tagged with {typeof(PartAttribute).FullName} should have a single uint parameter as signature",
                                                                                            "SourceGenerator",
                                                                                            DiagnosticSeverity.Error,
                                                                                            true);

    private static readonly DiagnosticDescriptor DuplicatedPartValueDescriptor = new("CG003",
                                                                                     "Duplicated Solver part value",
                                                                                     "A solver part with the same value has already been defined in this class",
                                                                                     "SourceGenerator",
                                                                                     DiagnosticSeverity.Error,
                                                                                     true);

    private static readonly DiagnosticDescriptor SolverClassNotPartial = new("CG004",
                                                                             "Solver class is not partial",
                                                                             "The solver class must be partial to allow for source generation",
                                                                             "SourceGenerator",
                                                                             DiagnosticSeverity.Error,
                                                                             true);

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<SolverInfo?> solvers = context.SyntaxProvider
                                                                .CreateSyntaxProvider(FindSolvers, GetSolverInfo)
                                                                .Where(s => s is not null);
        context.RegisterSourceOutput(solvers, RegisterSolverSource!);
    }

    private static bool FindSolvers(SyntaxNode node, CancellationToken token)
    {
        return node is ClassDeclarationSyntax { BaseList.Types.Count: > 0, AttributeLists.Count: > 0 };
    }

    private static SolverInfo? GetSolverInfo(GeneratorSyntaxContext context, CancellationToken token)
    {
        ClassDeclarationSyntax solverNode = (ClassDeclarationSyntax)context.Node;
        INamedTypeSymbol? solverSymbol = context.SemanticModel.GetDeclaredSymbol(solverNode, token);
        if (solverSymbol is null) return null;

        if (!solverNode.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))) return new SolverInfo(solverNode, solverSymbol, [], IsNotMarkedPartial: true);

        bool hasSolverAttribute = solverSymbol.GetAttributes()
                                        .Any(a => a.AttributeClass?.ToDisplayString() == typeof(SolverAttribute).FullName);
        if (!hasSolverAttribute) return null;

        INamedTypeSymbol solverBaseSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(SOLVER_BASE_TYPE_NAME)!;
        if (!InheritsType(solverSymbol, solverBaseSymbol)) return new SolverInfo(solverNode, solverSymbol, [], IsMissingBaseClass: true);

        INamedTypeSymbol partAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(PartAttribute).FullName!)!;
        PartMethodInfo[] methods =
        [
            ..solverNode.Members
                        .OfType<MethodDeclarationSyntax>()
                        .Select(m => (node: m, symbol: context.SemanticModel.GetDeclaredSymbol(m, token)!))
                        .Where(m => m.symbol is not null)
                        .Select(m => (m.node, m.symbol, attribute: GetAttributeOfType(m.symbol, partAttributeSymbol)))
                        .Where(m => m.attribute is not null)
                        .Select(m => new PartMethodInfo(m.node, m.symbol, (uint)m.attribute.ConstructorArguments[0].Value!, HasValidPartMethodSignature(m.symbol)))!
        ];


        return methods.Length is not 0 ? new SolverInfo(solverNode, solverSymbol, methods) : null;
    }

    private static void RegisterSolverSource(SourceProductionContext context, SolverInfo solver)
    {
        if (solver.IsNotMarkedPartial)
        {
            Diagnostic diagnostic = Diagnostic.Create(SolverClassNotPartial,
                                                      solver.ClassNode.Identifier.GetLocation(),
                                                      solver.ClassSymbol.Name);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        if (solver.IsMissingBaseClass)
        {
            Diagnostic diagnostic = Diagnostic.Create(MissingBaseClassDescriptor,
                                                      solver.ClassNode.Identifier.GetLocation(),
                                                      solver.ClassSymbol.Name);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        HashSet<uint> parts = [];
        List<PartMethod> methodsToGenerate = new(solver.PartMethods.Count);
        foreach (PartMethodInfo partMethodInfo in solver.PartMethods)
        {
            if (partMethodInfo.IsInvalidPartDeclaration)
            {
                Diagnostic diagnostic = Diagnostic.Create(InvalidPartMethodSignatureDescriptor,
                                                          partMethodInfo.MethodNode.Identifier.GetLocation(),
                                                          partMethodInfo.MethodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
                continue;
            }

            if (!parts.Add(partMethodInfo.Part))
            {
                Diagnostic diagnostic = Diagnostic.Create(DuplicatedPartValueDescriptor,
                                                          partMethodInfo.MethodNode.Identifier.GetLocation(),
                                                          partMethodInfo.MethodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
                continue;
            }

            methodsToGenerate.Add(new PartMethod(partMethodInfo.MethodSymbol.Name, partMethodInfo.Part));
        }

        if (methodsToGenerate.Count is 0) return;

        methodsToGenerate.Sort((a, b) => a.Part.CompareTo(b.Part));
        string fileNamespace = solver.ClassSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        string classAccess = solver.ClassSymbol.DeclaredAccessibility switch
        {
            Accessibility.ProtectedAndInternal => "protected internal",
            Accessibility.Protected            => "protected",
            Accessibility.Internal             => "internal",
            Accessibility.Public               => "public",
            _                                  => string.Empty
        };
        string className = solver.ClassSymbol.ToDisplayString();
        context.AddSource($"{solver.ClassSymbol.ToDisplayString()}.g.cs", SourceText.From(GenerateSource(fileNamespace, classAccess, className, methodsToGenerate)));
    }

    private static bool InheritsType(INamedTypeSymbol? type, INamedTypeSymbol parentType)
    {
        while (type is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, parentType)) return true;
            type = type.BaseType;
        }

        return false;
    }

    private static AttributeData GetAttributeOfType(IMethodSymbol methodSymbol, INamedTypeSymbol attributeSymbol)
    {
        return methodSymbol.GetAttributes()
                           .First(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass?.OriginalDefinition, attributeSymbol));
    }

    private static bool HasValidPartMethodSignature(IMethodSymbol method)
    {
        return method.Parameters.Length is 1
            && method.Parameters[0] is { Type.SpecialType: SpecialType.System_UInt32 };
    }

    private static string GenerateSource(string fileNamespace, string classAccess, string className, IReadOnlyCollection<PartMethod> methods)
    {
        using Stream? resource = typeof(SolverRunPartsGenerator).Assembly.GetManifestResourceStream("Challenge.Generator.Templates.Solver.sbn");
        if (resource is null) return string.Empty;

        using StreamReader reader = new(resource);
        Template template = Template.Parse(reader.ReadToEnd());
        return template.Render(new
        {
            Namespace = fileNamespace,
            Access = classAccess,
            ClassName = className,
            ToolName = typeof(SolverRunPartsGenerator).FullName,
            Version = typeof(SolverRunPartsGenerator).Assembly.GetName().Version.ToString(),
            Methods = methods
        });
    }
}
