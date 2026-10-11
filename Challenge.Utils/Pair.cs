using JetBrains.Annotations;

namespace Challenge.Utils;

/// <summary>
/// Ordered pair
/// </summary>
/// <param name="First">First element</param>
/// <param name="Second">Second element</param>
/// <typeparam name="T">Element type</typeparam>
[PublicAPI]
public readonly record struct Pair<T>(T? First, T? Second) where T : IEquatable<T>
{
    /// <summary>
    /// Enumerate the members of this pair
    /// </summary>
    /// <returns>An enumerable containing the first and second element in sequence</returns>
    public IEnumerable<T?> Enumerate()
    {
        yield return this.First;
        yield return this.Second;
    }

    /// <inheritdoc />
    public override string ToString() => $"({this.First?.ToString() ?? "null"}, {this.Second?.ToString() ?? "null"})";

    /// <summary>
    /// Converts a Tuple to a <see cref="Pair{T}"/>
    /// </summary>
    /// <param name="tuple">Tuple to create the Pair from</param>
    /// <returns>The equivalent <see cref="Pair{T}"/></returns>
    public static implicit operator Pair<T>((T? first, T? second) tuple) => new(tuple.first, tuple.second);

    /// <summary>
    /// Converts an <see cref="UnorderedPair{T}"/> to a <see cref="Pair{T}"/>
    /// </summary>
    /// <param name="unorderedPair">UnorderedPair to create the Pair from</param>
    /// <returns>The equivalent unordered <see cref="Pair{T}"/></returns>
    public static explicit operator Pair<T>(UnorderedPair<T> unorderedPair) => new(unorderedPair.First, unorderedPair.Second);

    /// <summary>
    /// Converts a  <see cref="Pair{T}"/> to a Tuple
    /// </summary>
    /// <param name="pair">Pair to create the Tuple from</param>
    /// <returns>The equivalent Tuple</returns>
    public static implicit operator (T? first, T? second)(Pair<T> pair) => (pair.First, pair.Second);
}

/// <summary>
/// Unordered pair, such as (a, b) == (b, a) on all checks
/// </summary>
/// <param name="First">First element</param>
/// <param name="Second">Second element</param>
/// <typeparam name="T">Element type</typeparam>
[PublicAPI]
public readonly record struct UnorderedPair<T>(T? First, T? Second)
    where T : IEquatable<T>
{
    private static readonly EqualityComparer<T> Comparer = EqualityComparer<T>.Default;

    /// <inheritdoc />
    public bool Equals(UnorderedPair<T> other) => (Comparer.Equals(this.First, other.First) && Comparer.Equals(this.Second, other.Second))
                                               || (Comparer.Equals(this.First, other.Second) && Comparer.Equals(this.Second, other.First));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        int first  = this.First?.GetHashCode()  ?? 0;
        int second = this.Second?.GetHashCode() ?? 0;
        return first <= second
                   ? HashCode.Combine(first, second)
                   : HashCode.Combine(second, first);
    }

    /// <inheritdoc />
    public override string ToString() => $"({this.First?.ToString() ?? "null"}, {this.Second?.ToString() ?? "null"})";

    /// <summary>
    /// Converts a Tuple to an <see cref="UnorderedPair{T}"/>
    /// </summary>
    /// <param name="tuple">Tuple to create the pair from</param>
    /// <returns>The equivalent <see cref="UnorderedPair{T}"/></returns>
    public static implicit operator UnorderedPair<T>((T? first, T? second) tuple) => new(tuple.first, tuple.second);

    /// <summary>
    /// Converts an <see cref="Pair{T}"/> to an <see cref="UnorderedPair{T}"/>
    /// </summary>
    /// <param name="pair">UnorderedPair to create the Pair from</param>
    /// <returns>The equivalent unordered <see cref="Pair{T}"/></returns>
    public static explicit operator UnorderedPair<T>(Pair<T> pair) => new(pair.First, pair.Second);

    /// <summary>
    /// Converts an <see cref="UnorderedPair{T}"/> to a Tuple
    /// </summary>
    /// <param name="unorderedPair">Pair to create the Tuple from</param>
    /// <returns>The equivalent Tuple</returns>
    public static implicit operator (T? first, T? second)(in UnorderedPair<T> unorderedPair) => (unorderedPair.First, unorderedPair.Second);
}
