using Challenge.Solvers;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Challenge.Generator.Test;

[Solver(2000, 1), UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed partial class GeneratorTest : Solver
{
    /// <inheritdoc />
    public GeneratorTest(string input, ILogger logger) : base(input, logger) { }

    [Part(1)]
    private static void RunPart1() { }

    [Part(2)]
    private static void RunPart2() { }

    [Part(3)]
    private static void RunPart3() { }
}
