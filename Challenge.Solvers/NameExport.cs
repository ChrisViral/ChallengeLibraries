/*
 * This file is meant as a way to export some declared names to the generator environment without having to include their respective references.
 * Basically the same idea as C++ forward declarations.
 */

// ReSharper disable PartialTypeWithSinglePart
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Challenge.Solvers
{
    public partial class Solver
    {
        public virtual partial void Run(uint part);
    }

    namespace Attributes
    {
        public partial class SolverAttribute;
        public partial class PartAttribute;
        public partial class SolverTableAttribute;
    }
}
