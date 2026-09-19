using System.Runtime.CompilerServices;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Challenge.Utils.Extensions.TimeSpans;

/// <summary>
/// <see cref="TimeSpan"/> extensions
/// </summary>
[PublicAPI]
public static class TimeSpanExtensions
{
    /// <param name="timeSpan">TimeSpan value</param>
    extension(TimeSpan timeSpan)
    {
        /// <summary>
        /// Produces a interval-based formatted time string
        /// </summary>
        /// <returns>An elapsed time string whose units are based on the total elapsed duration</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetElapsedString() => timeSpan switch
        {
            { TotalNanoseconds:  <  1000d } => $"{timeSpan.Nanoseconds}ns",
            { TotalMicroseconds: <  10d   } => $"{timeSpan.Microseconds}µs {timeSpan.Nanoseconds}ns",
            { TotalMicroseconds: <= 1000d } => $"{timeSpan.Microseconds}μs",
            { TotalMilliseconds: <  10d   } => $"{timeSpan.Milliseconds}ms {timeSpan.Microseconds}μs",
            { TotalMilliseconds: <= 1000d } => $"{timeSpan.Milliseconds}ms",
            { TotalSeconds:      <  10d   } => $"{timeSpan.Seconds}s {timeSpan.Milliseconds}ms",
            { TotalSeconds:      <= 60d   } => $"{timeSpan.Seconds}s",
            _                               => $"{timeSpan.Minutes}m {timeSpan.Seconds}s"
        };
    }
}
