using System.Numerics;
using JetBrains.Annotations;

namespace Challenge.Utils;

/// <summary>
/// Ordered pair
/// </summary>
/// <param name="First">First element</param>
/// <param name="Second">Second element</param>
/// <typeparam name="T">Element type</typeparam>
[PublicAPI]
public readonly record struct Pair<T>(T First, T Second)
    where T : IEquatable<T>?
{
    private static readonly EqualityComparer<T> Comparer = EqualityComparer<T>.Default;

    /// <summary>
    /// Copy of this pair, but with it's first and second element inverted
    /// </summary>
    public Pair<T> Inverted => new(this.Second, this.First);

    /// <summary>
    /// Enumerate the members of this pair
    /// </summary>
    /// <returns>An enumerable containing the first and second element in sequence</returns>
    public IEnumerable<T> Enumerate()
    {
        yield return this.First;
        yield return this.Second;
    }

    /// <summary>
    /// If this pair contains the specified value
    /// </summary>
    /// <param name="value">Value to find in the pair</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is a member of this pair, otherwise <see langword="false"/></returns>
    public bool HasValue(T value) => Comparer.Equals(this.First, value)
                                  || Comparer.Equals(this.Second, value);

    /// <summary>
    /// Gets the counterpart of the value in a pair
    /// </summary>
    /// <param name="value">Value to get the counterpart for</param>
    /// <returns>The found pair counterpart</returns>
    /// <exception cref="InvalidOperationException">If <paramref name="value"/> was not a member of the pair</exception>
    public T? GetCounterpart(T? value)
    {
        if (Comparer.Equals(this.First, value))
        {
            return this.Second;
        }

        if (Comparer.Equals(this.Second, value))
        {
            return this.First;
        }

        throw new InvalidOperationException("Could not find pair counterpart");
    }

    /// <summary>
    /// Tries to get the counterpart of the value if found in the pair
    /// </summary>
    /// <param name="value">Value to get the counterpart for</param>
    /// <param name="counterpart">Counterpart value, if found</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was a member of the pair, otherwise <see langword="false"/></returns>
    public bool TryGetCounterpart(T? value, out T? counterpart)
    {
        if (Comparer.Equals(this.First, value))
        {
            counterpart = this.Second;
            return true;
        }

        if (Comparer.Equals(this.Second, value))
        {
            counterpart = this.First;
            return true;
        }

        counterpart = default;
        return false;
    }

    /// <inheritdoc />
    public override string ToString() => $"({this.First?.ToString() ?? "null"}, {this.Second?.ToString() ?? "null"})";

    /// <summary>
    /// Converts a Tuple to a <see cref="Pair{T}"/>
    /// </summary>
    /// <param name="tuple">Tuple to create the Pair from</param>
    /// <returns>The equivalent <see cref="Pair{T}"/></returns>
    public static implicit operator Pair<T>((T first, T second) tuple) => new(tuple.first, tuple.second);

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
/// Pair extension methods
/// </summary>
[PublicAPI]
public static class PairExtensions
{
    /// <typeparam name="T">Type of element within the pair</typeparam>
    extension<T>(Pair<T>) where T : unmanaged, IEquatable<T>, ISpanParsable<T>
    {
        /// <summary>
        /// Parses a pair given the given separator
        /// </summary>
        /// <param name="value">Value to parse</param>
        /// <param name="separator">Separator character</param>
        /// <returns>The parsed Pair</returns>
        /// <exception cref="ArgumentException">If <paramref name="value"/> does not contain exactly two elements</exception>
        public static Pair<T> Parse(ReadOnlySpan<char> value, char separator = ' ')
        {
            Span<T> result = stackalloc T[2];
            int length = ParseUtils.ParseSpan(value, separator, v => T.Parse(v, null), result);
            return length is 2 ? new Pair<T>(result[0], result[1]) : throw new ArgumentException("Expected two elements to parse", nameof(value));
        }

        /// <summary>
        /// Parses a pair given the given separator
        /// </summary>
        /// <param name="value">Value to parse</param>
        /// <param name="separator">Separator characters</param>
        /// <returns>The parsed Pair</returns>
        /// <exception cref="ArgumentException">If <paramref name="value"/> does not contain exactly two elements</exception>
        public static Pair<T> Parse(ReadOnlySpan<char> value, ReadOnlySpan<char> separator)
        {
            Span<T> result = stackalloc T[2];
            int length = ParseUtils.ParseSpan(value, separator, v => T.Parse(v, null), result);
            return length is 2 ? new Pair<T>(result[0], result[1]) : throw new InvalidOperationException("Expected two elements to parse");
        }
    }

    /// <param name="pair">Pair instance</param>
    /// <typeparam name="T">Type of element within the pair</typeparam>
    extension<T>(Pair<T> pair) where T : unmanaged, INumber<T>
    {
        /// <summary>
        /// Minimum value of the pair
        /// </summary>
        /// <returns>The minimum value contained within the pair</returns>
        public T Min() => T.Min(pair.First, pair.Second);

        /// <summary>
        /// Maximum value of the pair
        /// </summary>
        /// <returns>The maximum value contained within the pair</returns>
        public T Max() => T.Max(pair.First, pair.Second);
    }
}
