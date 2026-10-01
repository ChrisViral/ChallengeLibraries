using System.Numerics;
using Challenge.Utils.ValueEnumerators;
using CommunityToolkit.HighPerformance.Enumerables;
using JetBrains.Annotations;
using ZLinq;

// ReSharper disable once CheckNamespace
namespace Challenge.Utils.Extensions.ValueEnumerables;

/// <summary>
/// ValueEnumerable extensions
/// </summary>
[PublicAPI]
public static class ValueEnumerableExtensions
{
        /// <param name="enumerable">Enumerable to multiply</param>
    /// <typeparam name="TEnumerator">Enumerator type</typeparam>
    /// <typeparam name="TSource">Type of value to multiply</typeparam>
    extension<TEnumerator, TSource>(ValueEnumerable<TEnumerator, TSource> enumerable)
        where TEnumerator : struct, IValueEnumerator<TSource>, allows ref struct
    {
        /// <summary>
        /// Applies an action to each member of the enumerable
        /// </summary>
        /// <param name="action">Action to apply</param>
        public void ForEach([InstantHandle] Action<TSource> action)
        {
            using TEnumerator enumerator = enumerable.Enumerator;
            while (enumerator.TryGetNext(out TSource element))
            {
                action(element);
            }
        }

        /// <summary>
        /// Adds all the elements in this ValueEnumerable to a collection
        /// </summary>
        /// <param name="collection">Collection to add to</param>
        public void AddTo([InstantHandle] ICollection<TSource> collection)
        {
            using TEnumerator enumerator = enumerable.Enumerator;
            while (enumerator.TryGetNext(out TSource element))
            {
                collection.Add(element);
            }
        }

        /// <summary>
        /// Counts the instance of a value in the enumerable
        /// </summary>
        /// <param name="value">Value to count</param>
        /// <returns>The amount of times <paramref name="value"/> is found in the enumerable</returns>
        [Pure]
        public int Count(TSource value) => enumerable.Count(value, EqualityComparer<TSource>.Default);

        /// <summary>
        /// Counts the instance of a value in the enumerable
        /// </summary>
        /// <param name="value">Value to count</param>
        /// <param name="comparer">Equality comparer</param>
        /// <returns>The amount of times <paramref name="value"/> is found in the enumerable</returns>
        [Pure]
        public int Count(TSource value, IEqualityComparer<TSource> comparer)
        {
            int count = 0;
            using TEnumerator enumerator = enumerable.Enumerator;
            while (enumerator.TryGetNext(out TSource current))
            {
                if (comparer.Equals(value, current))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Multiplies the given values and returns the result
        /// </summary>
        /// <returns>The product of all the values</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="enumerable"/> is null</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="enumerable"/> is empty</exception>
        [Pure]
        public TResult Sum<TResult>([InstantHandle] Func<TSource, TResult> selector)
            where TResult : IAdditionOperators<TResult, TResult, TResult>
        {
            using TEnumerator enumerator = enumerable.Enumerator;
            if (!enumerator.TryGetNext(out TSource element)) throw new InvalidOperationException("Cannot multiply an empty collection");

            TResult result = selector(element);
            while (enumerator.TryGetNext(out element))
            {
                result += selector(element);
            }
            return result;
        }

        /// <summary>
        /// Multiplies the given values and returns the result
        /// </summary>
        /// <returns>The product of all the values</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="enumerable"/> is null</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="enumerable"/> is empty</exception>
        [Pure]
        public TResult Multiply<TResult>([InstantHandle] Func<TSource, TResult> selector)
            where TResult : IMultiplyOperators<TResult, TResult, TResult>
        {
            using TEnumerator enumerator = enumerable.Enumerator;
            if (!enumerator.TryGetNext(out TSource element)) throw new InvalidOperationException("Cannot multiply an empty collection");

            TResult result = selector(element);
            while (enumerator.TryGetNext(out element))
            {
                result *= selector(element);
            }
            return result;
        }
    }

    /// <param name="enumerable">Enumerable to multiply</param>
    /// <typeparam name="TEnumerator">Enumerator type</typeparam>
    /// <typeparam name="TSource">Type of value to multiply</typeparam>
    extension<TEnumerator, TSource>(ValueEnumerable<TEnumerator, TSource> enumerable)
        where TEnumerator : struct, IValueEnumerator<TSource>, allows ref struct
        where TSource : IAdditionOperators<TSource, TSource, TSource>
    {
        /// <summary>
        /// Multiplies the given values and returns the result
        /// </summary>
        /// <returns>The product of all the values</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="enumerable"/> is null</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="enumerable"/> is empty</exception>
        [Pure]
        public TSource Sum()
        {
            using TEnumerator enumerator = enumerable.Enumerator;
            if (!enumerator.TryGetNext(out TSource result)) throw new InvalidOperationException("Cannot multiply an empty collection");

            while (enumerator.TryGetNext(out TSource current))
            {
                result += current;
            }
            return result;
        }
    }

    /// <param name="enumerable">Enumerable to multiply</param>
    /// <typeparam name="TEnumerator">Enumerator type</typeparam>
    /// <typeparam name="TSource">Type of value to multiply</typeparam>
    extension<TEnumerator, TSource>(ValueEnumerable<TEnumerator, TSource> enumerable)
        where TEnumerator : struct, IValueEnumerator<TSource>, allows ref struct
        where TSource : IMultiplyOperators<TSource, TSource, TSource>
    {
        /// <summary>
        /// Multiplies the given values and returns the result
        /// </summary>
        /// <returns>The product of all the values</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="enumerable"/> is null</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="enumerable"/> is empty</exception>
        [Pure]
        public TSource Multiply()
        {
            using TEnumerator enumerator = enumerable.Enumerator;
            if (!enumerator.TryGetNext(out TSource result)) throw new InvalidOperationException("Cannot multiply an empty collection");

            while (enumerator.TryGetNext(out TSource current))
            {
                result *= current;
            }
            return result;
        }
    }
}
