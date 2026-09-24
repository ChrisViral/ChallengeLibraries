using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Challenge.Solvers;
using Challenge.Solvers.Attributes;
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
public sealed class SolverGenerator : IIncrementalGenerator
{
    /// <summary>
    /// ILogger fully qualified name
    /// </summary>
    private const string LOGGER_TYPE_FULL_NAME = "Microsoft.Extensions.Logging.ILogger";

    /// <summary>
    /// GetSolver method name
    /// </summary>
    private const string GET_SOLVER_METHOD_NAME = "GetSolver";

    /// <summary>
    /// This assembly's version string
    /// </summary>
    private static string Version
    {
        get
        {
            Version version = typeof(SolverGenerator).Assembly.GetName().Version;
            return $"{version.Major}.{version.Minor:D2}.{version.Build:D4}.{version.Revision:D4}";
        }
    }

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find valid solvers and generate source for them
        IncrementalValuesProvider<SolverInfo> allSolvers = context.SyntaxProvider
                                                                  .CreateSyntaxProvider(FindClassesWithAttributes, GetSolverInfo)
                                                                  .Where(s => s is not null)!;
        context.RegisterSourceOutput(allSolvers, GenerateSolverSource);

        // Generate solver match source
        IncrementalValuesProvider<(SolverTableInfo, ImmutableArray<SolverInfo>)> allSolverTables = context.SyntaxProvider
                                                                                                          .CreateSyntaxProvider(FindClassesWithAttributes, GetSolverTableInfo)
                                                                                                          .Where(s => s is not null)
                                                                                                          .Combine(allSolvers.Collect())!;
        context.RegisterSourceOutput(allSolverTables, GenerateSolverTableSource);
    }

    /// <summary>
    /// Find potential solver objects
    /// </summary>
    /// <param name="node">Current syntax node</param>
    /// <param name="token">Cancellation token</param>
    /// <returns><see landword="true"/> if the node could be a solver, otherwise <see landword="false"/></returns>
    private static bool FindClassesWithAttributes(SyntaxNode node, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    /// <summary>
    /// Gets the solver info for a given context node
    /// </summary>
    /// <param name="context">Generation context</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>The parsed solver info</returns>
    private static SolverInfo? GetSolverInfo(GeneratorSyntaxContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        // Get the class symbol
        ClassDeclarationSyntax solverNode = (ClassDeclarationSyntax)context.Node;
        INamedTypeSymbol? solverSymbol = context.SemanticModel.GetDeclaredSymbol(solverNode, token);
        if (solverSymbol is null) return null;

        // Check if type has the solver attribute
        INamedTypeSymbol? solverAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(SolverAttribute).FullName!);
        if (solverAttributeSymbol is null) return null;

        AttributeData? solverAttribute = GetAttributeOfType(solverSymbol, solverAttributeSymbol);
        if (solverAttribute is null) return null;

        // Get the attribute args
        uint year = (uint)solverAttribute.ConstructorArguments[0].Value!;
        uint day  = (uint)solverAttribute.ConstructorArguments[1].Value!;

        // Check if the type inherits Solver
        INamedTypeSymbol solverBaseSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(Solver).FullName!)!;
        if (!InheritsType(solverSymbol, solverBaseSymbol)) return new SolverInfo(solverNode, solverSymbol, [], year, day, IsMissingBaseClass: true);

        // Check if the type is marked as abstract
        bool isMarkedAbstract = solverNode.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        if (isMarkedAbstract) return new SolverInfo(solverNode, solverSymbol, [], year, day, IsMarkedAbstract: true);

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
                   ? new SolverInfo(solverNode, solverSymbol, partMethods, year, day,
                                    IsNotMarkedPartial: isNotMarkedPartial,
                                    IsMissingConstructor: isMissingConstructor,
                                    PartRunMethod: partRunMethod)
                   : null;
    }

    /// <summary>
    /// Gets the solver table info for a given context node
    /// </summary>
    /// <param name="context">Generation context</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>The parsed solver table info</returns>
    private static SolverTableInfo? GetSolverTableInfo(GeneratorSyntaxContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        // Get the class symbol
        ClassDeclarationSyntax solverTableNode = (ClassDeclarationSyntax)context.Node;
        INamedTypeSymbol? solverTableSymbol = context.SemanticModel.GetDeclaredSymbol(solverTableNode, token);
        if (solverTableSymbol is null) return null;

        // Check if type has the solver table attribute
        INamedTypeSymbol? solverAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(SolverTableAttribute).FullName!);
        if (solverAttributeSymbol is null ||GetAttributeOfType(solverTableSymbol, solverAttributeSymbol) is null) return null;

        // Check if the type is marked as partial
        bool isNotMarkedPartial = !solverTableNode.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        if (isNotMarkedPartial) return new SolverTableInfo(solverTableNode, solverTableSymbol, null, IsNotMarkedPartial: true);

        INamedTypeSymbol loggerSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(LOGGER_TYPE_FULL_NAME)!;
        IMethodSymbol? getSolverMethodSymbol = GetGetSolverMethodDefinition(solverTableSymbol, loggerSymbol);
        return new SolverTableInfo(solverTableNode, solverTableSymbol, getSolverMethodSymbol, IsNotMarkedPartial: isNotMarkedPartial);
    }

    /// <summary>
    /// Register solvers for source generation
    /// </summary>
    /// <param name="context">Source generation context</param>
    /// <param name="solver">Solver instance</param>
    private static void GenerateSolverSource(SourceProductionContext context, SolverInfo solver)
    {
        // Handle class diagnostics
        if (solver.HandleClassDiagnostics(context)) return;

        // Generate source code
        IReadOnlyList<PartMethod> methodsToGenerate = GetMethodsToGenerate(context, solver);
        context.AddSource($"{solver.ClassSymbol.ToDisplayString()}.generated.cs", SourceText.From(GenerateSolverSource(solver, methodsToGenerate), Encoding.UTF8));
    }

    /// <summary>
    /// Registers solver tables for source generation
    /// </summary>
    /// <param name="context">Source generation context</param>
    /// <param name="solverTable">Solver table instance</param>
    private static void GenerateSolverTableSource(SourceProductionContext context, (SolverTableInfo info, ImmutableArray<SolverInfo> solvers) solverTable)
    {
        // Handle class diagnostics
        if (solverTable.info.HandleClassDiagnostics(context)) return;
        context.AddSource($"{solverTable.info.ClassSymbol.ToDisplayString()}.generated.cs", SourceText.From(GenerateSolverTableSource(solverTable.info, solverTable.solvers), Encoding.UTF8));
    }

    /// <summary>
    /// Generates the source code for a given solver
    /// </summary>
    /// <param name="solver">Solver data</param>
    /// <param name="partMethods">Part methods</param>
    /// <returns>The generated source code for this <paramref name="solver"/></returns>
    private static string GenerateSolverSource(SolverInfo solver, IReadOnlyList<PartMethod> partMethods)
    {
        if (!TryGetTemplate("Solver", out Template? template)) return string.Empty;

        string fileNamespace = solver.ClassSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        string classAccess = GetAccessString(solver.ClassSymbol.DeclaredAccessibility);
        string className = solver.ClassSymbol.Name;
        string toolName = typeof(SolverGenerator).FullName!;

        ScriptObject[] methods = new ScriptObject[partMethods.Count];
        for (int i = 0; i < partMethods.Count; i++)
        {
            PartMethod method = partMethods[i];
            methods[i] = new ScriptObject
            {
                ["name"] = method.Name,
                ["part"] = method.Part
            };
        }

        return template.Render(new
        {
            fileNamespace,
            classAccess,
            className,
            solver.IsMissingConstructor,
            toolName,
            Version,
            methods
        });
    }

    /// <summary>
    /// Generates source code for the solver table
    /// </summary>
    /// <param name="solverTableInfo">Solver info data</param>
    /// <param name="solverInfos">Solvers to generate for</param>
    private static string GenerateSolverTableSource(SolverTableInfo solverTableInfo, ImmutableArray<SolverInfo> solverInfos)
    {
        // Get template
        if (!TryGetTemplate("SolverTable", out Template? template)) return string.Empty;

        // Create solvers array
        ScriptObject[] solvers = new ScriptObject[solverInfos.Length];
        for (int i = 0; i < solvers.Length; i++)
        {
            SolverInfo solver = solverInfos[i];
            solvers[i] = new ScriptObject
            {
                ["year"] = solver.Year,
                ["day"]  = solver.Day,
                ["name"] = solver.ClassSymbol.ToDisplayString()
            };
        }

        string className = solverTableInfo.ClassSymbol.Name;
        string fileNamespace = solverTableInfo.ClassSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        string classAccess = GetAccessString(solverTableInfo.ClassSymbol.DeclaredAccessibility);
        string toolName = typeof(SolverGenerator).FullName!;
        bool needsOverride = solverTableInfo.ExistingGetSolverMethod is { IsOverride: true }
                                                                     or { IsVirtual: true }
                                                                     or { IsAbstract: true };

        // Render template and write source
        return template.Render(new
        {
            fileNamespace,
            classAccess,
            className,
            toolName,
            needsOverride,
            Version,
            solvers
        });
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
        return constructor.Parameters.Length is 1
            && SymbolEqualityComparer.Default.Equals(constructor.Parameters[0].Type.OriginalDefinition, loggerSymbol);
    }

    /// <summary>
    /// Checks if the given method is a Part Run method
    /// </summary>
    /// <param name="data">Method data</param>
    /// <returns><see langword="true"/> if <paramref name="data"/> is a Part Run method, otherwise <see langword="false"/></returns>
    private static bool IsPartRunMethod(MethodData data)
    {
        return data.Symbol is { Name: nameof(Solver.Run), Parameters.Length: 1 }
            && data.Symbol.Parameters[0].Type.SpecialType is SpecialType.System_UInt32;
    }

    /// <summary>
    /// Gets the GetSolver method for this type, if implemented
    /// </summary>
    /// <param name="type">Type to find the GetSolver method in</param>
    /// <param name="loggerSymbol">ILogger type symbol</param>
    /// <returns>The found GetSolver method symbol, or <see langword="null"/></returns>
    private static IMethodSymbol? GetGetSolverMethodDefinition(INamedTypeSymbol? type, INamedTypeSymbol loggerSymbol)
    {
        while (type is not null)
        {
            IMethodSymbol? getSolverMethod = type.GetMembers()
                                                    .OfType<IMethodSymbol>()
                                                    .FirstOrDefault(m => IsGetSolverMethod(m, loggerSymbol));
            if (getSolverMethod is not null) return getSolverMethod;

            type = type.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Checks if the given method is a GetSolver method
    /// </summary>
    /// <param name="method">Method symbol</param>
    /// <param name="loggerSymbol">ILogger type symbol</param>
    /// <returns><see langword="true"/> if <paramref name="method"/> is a GetSolver method, otherwise <see langword="false"/></returns>
    private static bool IsGetSolverMethod(IMethodSymbol method, INamedTypeSymbol loggerSymbol)
    {
        return method is { Name: GET_SOLVER_METHOD_NAME, Parameters.Length: 3 }
            && method.Parameters[0].Type.SpecialType is SpecialType.System_UInt32
            && method.Parameters[1].Type.SpecialType is SpecialType.System_UInt32
            && SymbolEqualityComparer.Default.Equals(method.Parameters[2].Type.OriginalDefinition, loggerSymbol);
    }

    /// <summary>
    /// Gets the accessibility string for a given accessibility value
    /// </summary>
    /// <param name="accessibility">Member accessibility</param>
    /// <returns>The equivalent accessibility string</returns>
    private static string GetAccessString(Accessibility accessibility) => accessibility switch
    {
        Accessibility.ProtectedAndInternal => "protected internal",
        Accessibility.Protected            => "protected",
        Accessibility.Internal             => "internal",
        Accessibility.Public               => "public",
        _                                  => string.Empty
    };

    /// <summary>
    /// Tries to get the given template from the assembly resources
    /// </summary>
    /// <param name="name">Template file name, without the extension</param>
    /// <param name="template">Found template, if any</param>
    /// <returns><see langword="true"/> if the template was found and created, otherwise <see langword="false"/></returns>
    private static bool TryGetTemplate(string name, [NotNullWhen(true)] out Template? template)
    {
        using Stream? resourceStream = typeof(SolverGenerator).Assembly.GetManifestResourceStream($"{typeof(SolverGenerator).Namespace}.Templates.{name}.sbn");
        if (resourceStream is not null)
        {
            using StreamReader reader = new(resourceStream);
            template = Template.Parse(reader.ReadToEnd());
            return true;
        }

        template = null;
        return false;
    }
}
