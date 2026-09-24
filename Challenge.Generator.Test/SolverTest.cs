using System.Diagnostics.CodeAnalysis;
using Challenge.Solvers;
using Challenge.Solvers.Attributes;
using CSharpFunctionalExtensions;

namespace Challenge.Generator.Test;

[Solver(2000, 1)]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
internal sealed partial class SolverTest : Solver
{
    [Part(1)]
    private void RunPart1() { }

    [Part(2)]
    private void RunPart2() { }

    [Part(3)]
    private void RunPart3() { }
}

[Solver(2001, 2)]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
internal sealed partial class SolverTest2 : Solver
{
    [Part(1)]
    private void RunPart1() { }

    [Part(2)]
    private void RunPart2() { }

    [Part(3)]
    private void RunPart3() { }
}

[SolverTable]
internal partial class SolverTableTest : ISolverResolver
{
    /// <inheritdoc />
    public string ChallengeName { get; } = "";

    /// <inheritdoc />
    public async Task<Result<string>> FetchInput(SolverData data, CancellationToken token = default)
    {
        return default;
    }

    /// <inheritdoc />
    public async Task<Result> SubmitAnswer(string answer, SolverData data, CancellationToken token = default)
    {
        return default;
    }
}
