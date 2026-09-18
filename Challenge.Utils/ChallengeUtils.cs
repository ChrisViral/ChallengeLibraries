using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using JetBrains.Annotations;

namespace Challenge.Utils;

/// <summary>
/// General Advent of Code utility methods
/// </summary>
[PublicAPI]
public static class ChallengeUtils
{
    /// <summary>
    /// Combines input lines into sequences, separated by empty lines
    /// </summary>
    /// <param name="input">Input lines</param>
    /// <returns>An enumerable of the packed input</returns>
    public static IEnumerable<List<string>> CombineLines([InstantHandle] IEnumerable<string> input)
    {
        List<string> pack = [];
        foreach (string line in input)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (pack.Count is 0) continue;

                yield return pack;
                pack = [];
            }
            else
            {
                pack.Add(line);
            }
        }

        if (pack.Count is not 0)
        {
            yield return pack;
        }
    }

    /// <summary>
    /// Produces a interval-based formatted time string
    /// </summary>
    /// <param name="timespan">Timespan to get the elapsed time string for</param>
    /// <returns>An elapsed time string whose units are based on the total elapsed duration</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetElapsedString(in TimeSpan timespan) => timespan switch
    {
        { TotalNanoseconds:  <  1000d } => $"{timespan.Nanoseconds}ns",
        { TotalMicroseconds: <  10d }   => $"{timespan.Microseconds}µs {timespan.Nanoseconds}ns",
        { TotalMicroseconds: <= 1000d } => $"{timespan.Microseconds}μs",
        { TotalMilliseconds: <  10d }   => $"{timespan.Milliseconds}ms {timespan.Microseconds}μs",
        { TotalMilliseconds: <= 1000d } => $"{timespan.Milliseconds}ms",
        { TotalSeconds:      <  10d }   => $"{timespan.Seconds}s {timespan.Milliseconds}ms",
        { TotalSeconds:      <= 60d }   => $"{timespan.Seconds}s",
        _                               => $"{timespan.Minutes}m {timespan.Seconds}s"
    };

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
