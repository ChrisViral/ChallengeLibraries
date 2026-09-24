# Challenge Solver Source Generator

The **Challenge Solver Source Generator** automatically generates the plumbing needed to turn a simple solver class into a fully‑functional Advent‑of‑Code style solver.

## How it works

1. Annotate your solver class with `SolverAttribute` and the correct year/day.
2. If the solver requires specific parts with different inputs, annotate the methods for each part with `PartAttribute` and the correct part number.
3. Create a solver lookup table with `SolverTableAttribute`.
4. Build – the generator emits the code needed to run everything.

## Usage

### Solver without parts

```csharp
using Challenge.Solvers;

// Add the attribute and specify the year/day
[Solver(2026, 1)]
public sealed class Day01 : Solver<int>   // Must inherit a Solver related class, specifying input type as needed
{
    // The constructor is optional, but only parameterless constructors may be defined
    public Day01() { }
    
    // Solution goes here
    public override void Run() { }
    
    // Input parsing goes here
    public override int Convert(string[] rawInput) => int.Parse(rawInput[0]);
}
```

- `SolverAttribute` takes two required arguments: `year` and `day`.
- The class must derive from `Challenge.Solvers.Solver`, and **cannot** be abstract.
- For no-parts implementations, the `Run()` method must be overriden.
- The input conversion method must be overriden.

### Solver with parts

```csharp
using Challenge.Solvers;

// Add the attribute and specify the year/day, class must be partial
[Solver(2026, 2)]
public sealed partial class Day25Solver : Solver<int> // Must inherit a Solver related class, specifying input type as needed
{
    // The constructor is optional, but only parameterless constructors may be defined
    public Day01() { }
    
    // Part 1 solution goes here
    [Part(1)]
    public void RunPart1() { }

    // Part 2 solution goes here, etc.
    [Part(2)]
    public void RunPart2() {  }
    
    // Input parsing goes here
    public override int Convert(string[] rawInput) => int.Parse(rawInput[0]);
}
```

- `SolverAttribute` takes two required arguments: `year` and `day`.
- The class must derive from `Challenge.Solvers.Solver`, and **cannot** be abstract.
- `PartAttribute` marks a method that implements logic for a specific part number.
- The generator emits an overridden `Run(uint part)` that dispatches to these methods.
- The input conversion method must be overriden.

### Solver Table

```csharp
using Challenge.Solvers;

// Add the attribute to mark class for code generation
[SolverTable]
public sealed partial class SolverTable : ISolverResolver  // Must implement ISolverResolver
{
    // Other implementations of ISolverResolver goes here
}
```

- `SolverTableAttribute` marks a class that implements `ISolverResolver`. The generator injects `GetSolver`.

## Using the generator

The implementation of `ISolverResolver` must be passed through DI to the `ChallengeCommand`,
which will be responsible for creating the solver, fetching input, and running your solver.