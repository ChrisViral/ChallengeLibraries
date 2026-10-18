using System.Numerics;
using JetBrains.Annotations;

namespace Challenge.Maths.Vectors;

/// <summary>
/// Adjacent vector options
/// </summary>
[PublicAPI, Flags]
public enum AdjacentOptions
{
    /// <summary>
    /// Cardinal offsets only
    /// </summary>
    CARDINAL_ONLY  = 0b000,
    /// <summary>
    /// Diagonally adjacent vectors including
    /// </summary>
    WITH_DIAGONALS = 0b001,
    /// <summary>
    /// Self vector included
    /// </summary>
    WITH_SELF      = 0b010,
    /// <summary>
    /// No cardinal offsets
    /// </summary>
    NO_CARDINAL    = 0b100,

    /// <summary>
    /// Diagonal offsets only
    /// </summary>
    DIAGONAL_ONLY = WITH_DIAGONALS | NO_CARDINAL
}

/// <summary>
/// Vector base interface
/// </summary>
[PublicAPI]
public interface IVector
{
    /// <summary>
    /// Vector dimension
    /// </summary>
    static abstract int Dimension { get; }

    /// <summary>
    /// Length of the Vector
    /// </summary>
    double Length { get; }

    /// <summary>
    /// Gets the vector's dimension
    /// </summary>
    /// <returns><see cref="Dimension"/></returns>
    int GetDimension();

    /// <summary>
    /// Tries to get the component in the specified type, with a checked conversion
    /// </summary>
    /// <param name="index">Component index</param>
    /// <param name="result">Resulting value</param>
    /// <typeparam name="T">Component output type</typeparam>
    /// <returns><see langword="true"/> if the componentn was successfully converted, otherwise <see langword="false"/></returns>
    bool TryGetComponentChecked<T>(int index, out T result) where T : unmanaged, IBinaryNumber<T>, IMinMaxValue<T>;

    /// <summary>
    /// Tries to get the component in the specified type, with a saturating conversion
    /// </summary>
    /// <param name="index">Component index</param>
    /// <param name="result">Resulting value</param>
    /// <typeparam name="T">Component output type</typeparam>
    /// <returns><see langword="true"/> if the componentn was successfully converted, otherwise <see langword="false"/></returns>
    bool TryGetComponentSaturating<T>(int index, out T result) where T : unmanaged, IBinaryNumber<T>, IMinMaxValue<T>;

    /// <summary>
    /// Tries to get the component in the specified type, with a truncating conversion
    /// </summary>
    /// <param name="index">Component index</param>
    /// <param name="result">Resulting value</param>
    /// <typeparam name="T">Component output type</typeparam>
    /// <returns><see langword="true"/> if the componentn was successfully converted, otherwise <see langword="false"/></returns>
    bool TryGetComponentTruncating<T>(int index, out T result) where T : unmanaged, IBinaryNumber<T>, IMinMaxValue<T>;
}

/// <summary>
/// Vector interface
/// </summary>
/// <typeparam name="T">Vector component type</typeparam>
[PublicAPI]
public interface IVector<out T> : IVector
    where T : unmanaged, IBinaryNumber<T>, IMinMaxValue<T>
{
    /// <summary>
    /// Gets the component at the given index
    /// </summary>
    /// <param name="index">Component's index</param>
    T this[int index] { get; }

    /// <summary>
    /// Gets the component at the given index
    /// </summary>
    /// <param name="index">Component's index</param>
    T this[Index index] { get; }
}

/// <summary>
/// Vector interface
/// </summary>
/// <typeparam name="TComponent">Vector component type</typeparam>
/// <typeparam name="TSelf">Vector self type</typeparam>
[PublicAPI]
public interface IVector<TSelf, out TComponent> : IVector<TComponent>, INumber<TSelf>, IMinMaxValue<TSelf>
    where TSelf : IVector<TSelf, TComponent>
    where TComponent : unmanaged, IBinaryNumber<TComponent>, IMinMaxValue<TComponent>
{
    /// <summary>
    /// Calculates the distance between two vectors
    /// </summary>
    /// <param name="a">First vector</param>
    /// <param name="b">Second vector</param>
    /// <returns>The distance between both vectors</returns>
    static abstract double Distance(TSelf a, TSelf b);

    /// <summary>
    /// Calculates the cross product of both vectors
    /// </summary>
    /// <param name="a">First vector</param>
    /// <param name="b">Second vector</param>
    /// <returns>The cross product of both vectors</returns>
    static abstract TComponent Dot(TSelf a, TSelf b);
}

/// <summary>
/// Vector cross product operator interface
/// </summary>
/// <typeparam name="TVector">Vector type</typeparam>
/// <typeparam name="TComponent">Vector component type</typeparam>
/// <typeparam name="TResult">Cross product result type</typeparam>
[PublicAPI]
public interface ICrossProductOperator<in TVector, TComponent, out TResult>
    where TVector : IVector<TVector, TComponent>
    where TComponent : unmanaged, IBinaryNumber<TComponent>, IMinMaxValue<TComponent>
    where TResult : INumber<TResult>
{
    /// <summary>
    /// Calculates the cross product of both vectors
    /// </summary>
    /// <param name="a">First vector</param>
    /// <param name="b">Second vector</param>
    /// <returns>The cross product of both vectors</returns>
    static abstract TResult Cross(TVector a, TVector b);
}
