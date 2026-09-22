using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Challenge.Solvers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using Scriban.Runtime;

namespace Challenge.Generator;

/// <summary>
/// A sample source generator that creates a custom report based on class properties. The target class should be annotated with the 'Generators.ReportAttribute' attribute.
/// When using the source code as a baseline, an incremental source generator is preferable because it reduces the performance overhead.
/// </summary>
[Generator]
public sealed class SolverRunPartsGenerator : IIncrementalGenerator
{
    /// <summary>
    /// ILogger type name
    /// </summary>
    private const string LOGGER_TYPE_FULL_NAME = "Microsoft.Extensions.Logging.ILogger";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find valid solvers
        IncrementalValuesProvider<SolverInfo?> solvers = context.SyntaxProvider
                                                                .CreateSyntaxProvider(FindSolvers, GetSolverInfo)
                                                                .Where(s => s is not null);

        // Register them and generate
        context.RegisterSourceOutput(solvers, RegisterSolverSource!);
    }

    /// <summary>
    /// Findd potential solver objects
    /// </summary>
    /// <param name="node">Current syntax node</param>
    /// <param name="token">Cancellation token</param>
    /// <returns><see landword="true"/> if the node could be a solver, otherwise <see landword="false"/></returns>
    private static bool FindSolvers(SyntaxNode node, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    /// <summary>
    /// Gets the solver info for a given context node
    /// </summary>
    /// <param name="context">Generation context</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    private static SolverInfo? GetSolverInfo(GeneratorSyntaxContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        // Get the class symbol
        ClassDeclarationSyntax solverNode = (ClassDeclarationSyntax)context.Node;
        INamedTypeSymbol? solverSymbol = context.SemanticModel.GetDeclaredSymbol(solverNode, token);
        if (solverSymbol is null) return null;

        // Check if type has the solver attribute
        INamedTypeSymbol? solverAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(SolverAttribute).FullName!);
        if (solverAttributeSymbol is null || GetAttributeOfType(solverSymbol, solverAttributeSymbol) is null) return null;

        // Check if the type inherits Solver
        INamedTypeSymbol solverBaseSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(Solver).FullName!)!;
        if (!InheritsType(solverSymbol, solverBaseSymbol)) return new SolverInfo(solverNode, solverSymbol, [], IsMissingBaseClass: true);

        // Check if the type is marked as abstract
        bool isMarkedAbstract = solverNode.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        if (isMarkedAbstract) return new SolverInfo(solverNode, solverSymbol, [], IsMarkedAbstract: true);

        // Check if the type has the required constructor
        INamedTypeSymbol loggerSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(LOGGER_TYPE_FULL_NAME)!;
        bool isMissingConstructor = !solverSymbol.Constructors.Any(m => IsValidConstructorSignature(m, loggerSymbol));

        // Check if the type is marked as partial
        bool isNotMarkedPartial = !solverNode.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

        // Get all methods of the class
        MethodData[] methods =
        [
            ..solverNode.Members
                        .OfType<MethodDeclarationSyntax>()
                        .Select(m => new MethodData(m, context.SemanticModel.GetDeclaredSymbol(m, token)!))
                         // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                        .Where(m => m.Symbol is not null)
        ];

        // Get potential extra Run method
        MethodData? partRunMethod = methods.FirstOrDefault(IsPartRunMethod);

        // Check if the type contains any methods tagged with the Part attribute
        INamedTypeSymbol partAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(PartAttribute).FullName!)!;
        PartMethodInfo[] partMethods =
        [
            ..methods.Select(m => (m.Node, m.Symbol, attribute: GetAttributeOfType(m.Symbol, partAttributeSymbol)!))
                     .Where(m => m.attribute is not null)
                     .Select(m => new PartMethodInfo(m.Node, m.Symbol, (uint)m.attribute.ConstructorArguments[0].Value!, !HasValidPartMethodSignature(m.Symbol)))!
        ];

        // Return a solver info if we have methods to generate or a constructor to generate
        return partMethods.Length is not 0 || isMissingConstructor
                   ? new SolverInfo(solverNode, solverSymbol, partMethods,
                                    IsNotMarkedPartial: isNotMarkedPartial,
                                    IsMissingConstructor: isMissingConstructor,
                                    PartRunMethod: partRunMethod)
                   : null;
    }

    /// <summary>
    /// Register solvers for source generation
    /// </summary>
    /// <param name="context">Source generation context</param>
    /// <param name="solver">Solver instance</param>
    private static void RegisterSolverSource(SourceProductionContext context, SolverInfo solver)
    {
        // Handle class diagnostics
        if (solver.HandleClassDiagnostics(context)) return;

        // Generate source code
        IReadOnlyList<PartMethod> methodsToGenerate = GetMethodsToGenerate(context, solver);
        string className = solver.ClassSymbol.Name;
        context.AddSource($"{className}.generated.cs", SourceText.From(GenerateSource(className, solver, methodsToGenerate), Encoding.UTF8));
    }

    /// <summary>
    /// Gets a list of methods to generate
    /// </summary>
    /// <param name="context">Source generation context</param>
    /// <param name="solver">Solver info</param>
    /// <returns>A list of all method data to generate</returns>
    private static IReadOnlyList<PartMethod> GetMethodsToGenerate(SourceProductionContext context, SolverInfo solver)
    {
        // Exit early if no methods found
        if (solver.PartMethods.Count is 0) return [];

        // List methods to generate
        List<PartMethod> methodsToGenerate = new(solver.PartMethods.Count);
        HashSet<uint> parts = [];
        foreach (PartMethodInfo partMethodInfo in solver.PartMethods)
        {
            // Diagnostic if the method has an invalid declaration
            if (partMethodInfo.IsInvalidPartDeclaration)
            {
                Diagnostic diagnostic = Diagnostic.Create(Diagnostics.InvalidPartMethodSignatureDescriptor,
                                                          partMethodInfo.MethodNode.Identifier.GetLocation(),
                                                          partMethodInfo.MethodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
                continue;
            }

            // Diagnostic if the part number has been seen before
            if (!parts.Add(partMethodInfo.Part))
            {
                Diagnostic diagnostic = Diagnostic.Create(Diagnostics.DuplicatedPartValueDescriptor,
                                                          partMethodInfo.MethodNode.Identifier.GetLocation(),
                                                          partMethodInfo.MethodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
                continue;
            }

            // Add the part method
            methodsToGenerate.Add(new PartMethod(partMethodInfo.MethodSymbol.Name, partMethodInfo.Part));
        }

        // Sort by part number
        methodsToGenerate.Sort((a, b) => a.Part.CompareTo(b.Part));
        return methodsToGenerate;
    }

    /// <summary>
    /// Checks if a type symbol inherits another one
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <param name="parentType">Parent type to find</param>
    /// <returns><see langword="true"/> if <paramref name="type"/> inherits <paramref name="parentType"/>, otherwise <see langword="false"/></returns>
    private static bool InheritsType(INamedTypeSymbol? type, INamedTypeSymbol parentType)
    {
        while (type is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, parentType)) return true;
            type = type.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Gets the attribute of a given type on the specified symbol
    /// </summary>
    /// <param name="symbol">Symbol to check</param>
    /// <param name="attributeSymbol">Attribute symbol to find</param>
    /// <returns>The first attribute of type <paramref name="attributeSymbol"/> on <paramref name="symbol"/>, otherwise <see langword="null"/></returns>
    private static AttributeData? GetAttributeOfType(ISymbol symbol, INamedTypeSymbol attributeSymbol)
    {
        return symbol.GetAttributes()
                           .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass?.OriginalDefinition, attributeSymbol));
    }

    /// <summary>
    /// Checks if the given method symbol is a valid Part method
    /// </summary>
    /// <param name="method">Method to check</param>
    /// <returns><see langword="true"/> if <paramref name="method"/> is a valid Part method, otherwise <see langword="false"/></returns>
    private static bool HasValidPartMethodSignature(IMethodSymbol method)
    {
        return method.Parameters.Length is 0;
    }

    /// <summary>
    /// Checks if the given constructor is a valid Solver constructor
    /// </summary>
    /// <param name="constructor">Constructor to check</param>
    /// <param name="loggerSymbol">Logger type symbol</param>
    /// <returns><see langword="true"/> if <paramref name="constructor"/> is a valid Solver constructor, otherwise <see langword="false"/></returns>
    private static bool IsValidConstructorSignature(IMethodSymbol constructor, INamedTypeSymbol loggerSymbol)
    {
        return constructor.Parameters.Length is 2
            && constructor.Parameters[0].Type.SpecialType is SpecialType.System_String
            && SymbolEqualityComparer.Default.Equals(constructor.Parameters[1].Type.OriginalDefinition, loggerSymbol);
    }

    /// <summary>
    /// Checks if the given method is an override of the base Part Run method
    /// </summary>
    /// <param name="data">Method data</param>
    /// <returns><see langword="true"/> if <paramref name="data"/> is a Part Run method override, otherwise <see langword="false"/></returns>
    private static bool IsPartRunMethod(MethodData data)
    {
        return data.Symbol is { Name: nameof(Solver.Run), Parameters.Length: 1 }
            && data.Symbol.Parameters[0].Type.SpecialType is SpecialType.System_UInt32;
    }

    /// <summary>
    /// Generates the source code for a given solver
    /// </summary>
    /// <param name="className">Solver class name</param>
    /// <param name="solver">Solver data</param>
    /// <param name="methods">Part methods</param>
    /// <returns>The generated source code for this <paramref name="solver"/></returns>
    private static string GenerateSource(string className, SolverInfo solver, IReadOnlyList<PartMethod> methods)
    {
        string fileNamespace = solver.ClassSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        string classAccess = solver.ClassSymbol.DeclaredAccessibility switch
        {
            Accessibility.ProtectedAndInternal => "protected internal",
            Accessibility.Protected            => "protected",
            Accessibility.Internal             => "internal",
            Accessibility.Public               => "public",
            _                                  => string.Empty
        };

        using Stream? resource = typeof(SolverRunPartsGenerator).Assembly.GetManifestResourceStream("Challenge.Generator.Templates.Solver.sbn");
        if (resource is null) return string.Empty;

        using StreamReader reader = new(resource);
        Template template = Template.Parse(reader.ReadToEnd());
        ScriptObject[] methodsContainer = new ScriptObject[methods.Count];
        for (int i = 0; i < methods.Count; i++)
        {
            PartMethod method = methods[i];
            methodsContainer[i] = new ScriptObject
            {
                [nameof(PartMethod.Name)] = method.Name,
                [nameof(PartMethod.Part)] = method.Part
            };
        }
        return template.Render(new
        {
            fileNamespace,
            classAccess,
            className,
            solver.IsMissingConstructor,
            ToolName = typeof(SolverRunPartsGenerator).FullName,
            Version = typeof(SolverRunPartsGenerator).Assembly.GetName().Version.ToString(),
            HasParts = methodsContainer.Length is not 0,
            Methods = methodsContainer
        });
    }
}
