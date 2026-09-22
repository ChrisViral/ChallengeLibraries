using JetBrains.Annotations;
using ZLinq;

namespace Challenge.Utils.ValueEnumerators;

/// <summary>
/// SpanSplitEnumerator value enumerator
/// </summary>
/// <param name="enumerator">Splits enumeratorm to create the value enumerator from</param>
/// <typeparam name="T">Span element type</typeparam>
[PublicAPI]
public ref struct FromSpanSplitEnumerator<T>(MemoryExtensions.SpanSplitEnumerator<T> enumerator) : IValueEnumerator<Range> where T : IEquatable<T>
{
    private MemoryExtensions.SpanSplitEnumerator<T> enumerator = enumerator;

    /// <inheritdoc />
    public bool TryGetNext(out Range current)
    {
        if (this.enumerator.MoveNext())
        {
            current = this.enumerator.Current;
            return true;
        }

        current = default;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetNonEnumeratedCount(out int count)
    {
        count = 0;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetSpan(out ReadOnlySpan<Range> span)
    {
        span = default;
        return false;
    }

    /// <inheritdoc />
    public bool TryCopyTo(scoped Span<Range> destination, Index offset) => false;

    /// <inheritdoc />
    void IDisposable.Dispose() { }
}
