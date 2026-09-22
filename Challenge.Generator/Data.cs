using System.Collections.Generic;
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
/// <param name="IsMarkedAbstract">If the class is marked as abstract</param>
/// <param name="IsMissingConstructor">If the class is missing it's required constructor</param>
/// <param name="IsMissingBaseClass">If the Solver base class is missing</param>
internal sealed record SolverInfo(ClassDeclarationSyntax ClassNode,
                                  INamedTypeSymbol ClassSymbol,
                                  IReadOnlyList<PartMethodInfo> PartMethods,
                                  bool IsNotMarkedPartial   = false,
                                  bool IsMarkedAbstract     = false,
                                  bool IsMissingConstructor = false,
                                  bool IsMissingBaseClass   = false);
