using Challenge.Utils.Extensions.Arrays;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Challenge.Solvers.Specialized;

/// <summary>
/// Array solver base
/// </summary>
[PublicAPI]
public abstract class ArraySolver<T> : Solver<T[]>
{
    /// <summary>
    /// Creates a new <see cref="GridSolver{T}"/> Solver
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="splitters">Splitting characters, defaults to newline only</param>
    /// <param name="options">Input parsing options, defaults to removing empty entries and trimming entries</param>
    /// <exception cref="InvalidOperationException">Thrown if the conversion to <typeparamref name="T"/><c>[]</c> fails</exception>
    protected ArraySolver(ILogger logger, char[]? splitters = null, StringSplitOptions options = DEFAULT_OPTIONS) : base(logger, splitters, options) { }

    /// <inheritdoc />
    protected sealed override T[] Convert(string[] rawInput) => rawInput.ConvertAll(ConvertLine);

    /// <summary>
    /// Converts an input line into an array member<br/>
    /// <b>NOTE</b>: This method <b>must</b> be pure as it initializes the base class
    /// </summary>
    /// <param name="line">Line to convert</param>
    /// <returns>The created member</returns>
    [Pure]
    protected abstract T ConvertLine(string line);
}
