using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using JetBrains.Annotations;

namespace Challenge.Utils;

/// <summary>
/// General Advent of Code utility methods
/// </summary>
[PublicAPI]
public static class SwapUtils
{
    /// <summary>
    /// Swaps two values in memory
    /// </summary>
    /// <typeparam name="T">Type of value to swap</typeparam>
    /// <param name="a">First value</param>
    /// <param name="b">Second value</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);

    /// <summary>
    /// Swaps two spans in memory
    /// </summary>
    /// <typeparam name="T">Type of element within the span</typeparam>
    /// <param name="a">First span</param>
    /// <param name="b">Second span</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap<T>(ref Span<T> a, ref Span<T> b)
    {
        Span<T> temp = a;
        a = b;
        b = temp;
    }

    /// <summary>
    /// Swaps two spans in memory
    /// </summary>
    /// <typeparam name="T">Type of element within the span</typeparam>
    /// <param name="a">First span</param>
    /// <param name="b">Second span</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap<T>(ref ReadOnlySpan<T> a, ref ReadOnlySpan<T> b)
    {
        ReadOnlySpan<T> temp = a;
        a = b;
        b = temp;
    }

    /// <summary>
    /// Swaps two spans in memory
    /// </summary>
    /// <typeparam name="T">Type of element within the span</typeparam>
    /// <param name="a">First span</param>
    /// <param name="b">Second span</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap<T>(ref Span2D<T> a, ref Span2D<T> b)
    {
        Span2D<T> temp = a;
        a = b;
        b = temp;
    }

    /// <summary>
    /// Swaps two spans in memory
    /// </summary>
    /// <typeparam name="T">Type of element within the span</typeparam>
    /// <param name="a">First span</param>
    /// <param name="b">Second span</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap<T>(ref ReadOnlySpan2D<T> a, ref ReadOnlySpan2D<T> b)
    {
        ReadOnlySpan2D<T> temp = a;
        a = b;
        b = temp;
    }
}
