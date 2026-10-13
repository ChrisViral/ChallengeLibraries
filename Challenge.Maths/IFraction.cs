using System.Numerics;
using JetBrains.Annotations;

namespace Challenge.Maths;

/// <summary>
/// Fraction base interface
/// </summary>
[PublicAPI]
public interface IFraction
{
    /// <summary>
    /// Converts the fraction numerator and denominator in a checked way
    /// </summary>
    /// <param name="numerator">Converter numerator</param>
    /// <param name="denominator">Converted denominator</param>
    /// <typeparam name="TTarget">Target number type</typeparam>
    /// <returns><see langword="true"/> if the conversion was successful, otherwise <see langword="false"/></returns>
    bool TryConvertChecked<TTarget>(out TTarget numerator, out TTarget denominator)
        where TTarget : unmanaged, IBinaryInteger<TTarget>, ISignedNumber<TTarget>, IMinMaxValue<TTarget>;

    /// <summary>
    /// Converts the fraction numerator and denominator in a saturating way
    /// </summary>
    /// <param name="numerator">Converter numerator</param>
    /// <param name="denominator">Converted denominator</param>
    /// <typeparam name="TTarget">Target number type</typeparam>
    /// <returns><see langword="true"/> if the conversion was successful, otherwise <see langword="false"/></returns>
    bool TryConvertSaturating<TTarget>(out TTarget numerator, out TTarget denominator)
        where TTarget : unmanaged, IBinaryInteger<TTarget>, ISignedNumber<TTarget>, IMinMaxValue<TTarget>;

    /// <summary>
    /// Converts the fraction numerator and denominator in a truncating way
    /// </summary>
    /// <param name="numerator">Converter numerator</param>
    /// <param name="denominator">Converted denominator</param>
    /// <typeparam name="TTarget">Target number type</typeparam>
    /// <returns><see langword="true"/> if the conversion was successful, otherwise <see langword="false"/></returns>
    bool TryConvertTruncating<TTarget>(out TTarget numerator, out TTarget denominator)
        where TTarget : unmanaged, IBinaryInteger<TTarget>, ISignedNumber<TTarget>, IMinMaxValue<TTarget>;
}

/// <summary>
/// Fraction base interface
/// </summary>
/// <typeparam name="T">Fraction number type</typeparam>
[PublicAPI]
public interface IFraction<out T> : IFraction
    where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T>, IMinMaxValue<T>
{
    /// <summary>
    /// Fraction numerator
    /// </summary>
    T Numerator { get; }

    /// <summary>
    /// Fraction denominator
    /// </summary>
    T Denominator { get; }

    /// <summary>
    /// Decimal representation of this fraction
    /// </summary>
    public decimal Decimal { get; }

    /// <summary>
    /// If this fraction is already in it's reduced form
    /// </summary>
    public bool IsReduced { get; }
}

/// <summary>
/// Fraction base interface
/// </summary>
/// <typeparam name="TSelf">Self type</typeparam>
/// <typeparam name="TValue">Fraction number type</typeparam>
[PublicAPI]
public interface IFraction<TSelf, TValue> : IFraction<TValue>, INumber<TSelf>, ISignedNumber<TSelf>, IMinMaxValue<TSelf>
    where TSelf : unmanaged, IFraction<TSelf, TValue>
    where TValue : unmanaged, IBinaryInteger<TValue>, ISignedNumber<TValue>, IMinMaxValue<TValue>
{
    /// <summary>
    /// Reduced version of this fraction
    /// </summary>
    public TSelf Reduced { get; }

    /// <summary>
    /// Tries to create a fraction with the given numerator and denominator
    /// </summary>
    /// <param name="numerator">Fraction numerator</param>
    /// <param name="denominator">Fraction denominator</param>
    /// <param name="result">Created fraction</param>
    /// <returns><see langword="true"/> if the fraction could be created, otherwise <see langword="false"/></returns>
    static abstract bool TryCreate(TValue numerator, TValue denominator, out TSelf result);

    /// <summary>
    /// Creates the fraction in a reduced form
    /// </summary>
    /// <param name="numerator">Fraction numerator</param>
    /// <param name="denominator">fraction denominator</param>
    /// <returns>the created fraction, reduced</returns>
    static abstract TSelf CreateReduced(TValue numerator, TValue denominator);
}
