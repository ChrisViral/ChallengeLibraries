using Challenge.Utils.ValueEnumerators;
using CommunityToolkit.HighPerformance.Enumerables;
using ZLinq;

// ReSharper disable once CheckNamespace
namespace Challenge.Utils.Extensions.RefEnumerables;

/// <summary>
/// RefEnumerable extensions
/// </summary>
public static class RefEnumerableExtensions
{
    /// <param name="enumerable">Ref enumerable</param>
    /// <typeparam name="T">Enumerable value type</typeparam>
    extension<T>(RefEnumerable<T> enumerable)
    {
        /// <summary>
        /// Returns this RefEnumerable as a ValueEnumerable
        /// </summary>
        /// <returns>ValueEnumerable instance</returns>
        public ValueEnumerable<FromRefEnumerable<T>, T> AsValueEnumerable()
        {
            return new ValueEnumerable<FromRefEnumerable<T>, T>(new FromRefEnumerable<T>(enumerable));
        }
    }
}
