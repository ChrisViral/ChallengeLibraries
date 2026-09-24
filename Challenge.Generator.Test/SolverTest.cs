using System.Diagnostics.CodeAnalysis;
using Challenge.Solvers;
using Challenge.Solvers.Attributes;

namespace Challenge.Generator.Test;

[Solver(2000, 1)]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
internal sealed partial class SolverTest : Solver
{
    /// <inheritdoc />
    public SolverTest() : base(null!) { }

    [Part(1)]
    private void RunPart1() { }

    [Part(2)]
    private void RunPart2() { }

    [Part(3)]
    private void RunPart3() { }
}

[SolverTable]
internal sealed partial class SolverTableTest;
