using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Challenge.Utils.Extensions.Numbers;
using JetBrains.Annotations;

namespace Challenge.Maths;

/// <summary>
/// Integer fraction
/// </summary>
/// <typeparam name="T">Fraction integer type</typeparam>
[PublicAPI]
public readonly struct Fraction<T> : IFraction<Fraction<T>, T>,
                                     IAdditionOperators<Fraction<T>, T, Fraction<T>>,
                                     ISubtractionOperators<Fraction<T>, T, Fraction<T>>,
                                     IMultiplyOperators<Fraction<T>, T, Fraction<T>>,
                                     IDivisionOperators<Fraction<T>, T, Fraction<T>>
    where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T>, IMinMaxValue<T>
{
    /// <summary>
    /// Default parse separator
    /// </summary>
    private const string DEFAULT_SEPARATOR = "/";

    /// <inheritdoc />
    public static Fraction<T> Zero { get; } = new(T.Zero, T.One);

    /// <inheritdoc />
    public static Fraction<T> One { get; } = new(T.One, T.One);

    /// <inheritdoc />
    public static Fraction<T> NegativeOne { get; } = new(-T.One, T.One);

    /// <inheritdoc />
    public static Fraction<T> MaxValue { get; } = new(T.MaxValue, T.One);

    /// <inheritdoc />
    public static Fraction<T> MinValue { get; } = new(T.MinValue, T.One);

    /// <summary>
    /// If this fraction holds <see cref="BigInteger"/>
    /// </summary>
    private static bool IsBigInt = typeof(T) == typeof(BigInteger);

    /// <summary>
    /// If this fraction holds <see cref="System.Int128"/>
    /// </summary>
    private static bool IsInt128 = typeof(T) == typeof(Int128);

    /// <summary>
    /// If this fraction holds <see cref="long"/>
    /// </summary>
    private static bool IsInt64 = typeof(T) == typeof(long);

    /// <inheritdoc />
    public T Numerator { get; }

    /// <inheritdoc />
    public T Denominator { get; }

    /// <inheritdoc />
    public decimal Decimal
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => decimal.CreateChecked(this.Numerator) / decimal.CreateChecked(this.Denominator);
    }

    /// <inheritdoc />
    public T Whole
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.Numerator / this.Denominator;
    }

    /// <inheritdoc />
    public decimal Fractional
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => decimal.CreateChecked(this.Numerator % this.Denominator) / decimal.CreateChecked(this.Denominator);
    }

    /// <inheritdoc />
    public Fraction<T> Reduced
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CreateReduced(this.Numerator, this.Denominator);
    }

    /// <inheritdoc />
    public Fraction<T> Invert
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.Numerator != T.Zero
                   ? new Fraction<T>(this.Denominator, this.Numerator)
                   : throw new DivideByZeroException("Cannot invert the fraction zero");
    }

    /// <inheritdoc />
    public bool IsReduced
    {
        get
        {
            if (this.Numerator == T.Zero) return true;

            if (IsBigInt)
            {
                return MathUtils.GCD(this.Numerator, this.Denominator) == T.One;
            }

            if (IsInt128)
            {
                UInt128 numeratorMagnitude   = MathUtils.Magnitude<T, UInt128>(this.Numerator);
                UInt128 denominatorMagnitude = MathUtils.Magnitude<T, UInt128>(this.Denominator);
                return UInt128.GCD(numeratorMagnitude, denominatorMagnitude) == UInt128.One;
            }

            if (IsInt64)
            {
                ulong numeratorMagnitude   = MathUtils.Magnitude<T, ulong>(this.Numerator);
                ulong denominatorMagnitude = MathUtils.Magnitude<T, ulong>(this.Denominator);
                return ulong.GCD(numeratorMagnitude, denominatorMagnitude) == 1UL;
            }

            return long.GCD(long.CreateChecked(this.Numerator), long.CreateChecked(this.Denominator)) == 1L;
        }
    }

    /// <summary>
    /// Creates a new fraction equal to <see cref="Zero"/>
    /// </summary>
    public Fraction()
    {
        this.Numerator   = T.Zero;
        this.Denominator = T.One;
    }

    /// <summary>
    /// Creates a new fraction from a numerator only
    /// </summary>
    /// <param name="numerator">Fraction numerator</param>
    public Fraction(T numerator)
    {
        this.Numerator   = numerator;
        this.Denominator = T.One;
    }

    /// <summary>
    /// Creates a new integer fraction
    /// </summary>
    /// <param name="numerator">Fraction numerator</param>
    /// <param name="denominator">Fraction denominator</param>
    /// <exception cref="DivideByZeroException">If <paramref name="denominator"/> is <c>0</c></exception>
    /// <exception cref="OverflowException">If the fraction cannot be represented with a positive denominator in <typeparamref name="T"/></exception>
    public Fraction(T numerator, T denominator)
    {
        if (denominator == T.Zero) throw new DivideByZeroException("Fraction denominator cannot be zero");

        if (numerator == T.Zero)
        {
            this.Numerator   = T.Zero;
            this.Denominator = T.One;
        }
        else if (denominator < T.Zero)
        {
            this.Numerator   = checked(-numerator);
            this.Denominator = checked(-denominator);
        }
        else
        {
            this.Numerator   = numerator;
            this.Denominator = denominator;
        }
    }

    /// <inheritdoc />
    public bool Equals(Fraction<T> other)
    {
        if (IsInt128 || IsBigInt)
        {
            BigInteger left  = MathUtils.BigMul<T, BigInteger>(this.Numerator, other.Denominator);
            BigInteger right = MathUtils.BigMul<T, BigInteger>(other.Numerator, this.Denominator);
            return left == right;
        }
        if (IsInt64)
        {
            Int128 left  = MathUtils.BigMul<T, Int128>(this.Numerator, other.Denominator);
            Int128 right = MathUtils.BigMul<T, Int128>(other.Numerator, this.Denominator);
            return left == right;
        }
        else
        {
            long left  = MathUtils.BigMul<T, long>(this.Numerator, other.Denominator);
            long right = MathUtils.BigMul<T, long>(other.Numerator, this.Denominator);
            return left == right;
        }
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Fraction<T> fraction && Equals(fraction);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (this.Numerator == T.Zero) return 0;
        if (this.Denominator == T.One) return HashCode.Combine(this.Denominator, T.One);

        Fraction<T> reduced = this.Reduced;
        return HashCode.Combine(reduced.Numerator, reduced.Denominator);
    }

    /// <inheritdoc />
    public int CompareTo(Fraction<T> other)
    {
        if (IsInt128 || IsBigInt)
        {
            BigInteger left  = MathUtils.BigMul<T, BigInteger>(this.Numerator, other.Denominator);
            BigInteger right = MathUtils.BigMul<T, BigInteger>(other.Numerator, this.Denominator);
            return left.CompareTo(right);
        }
        if (IsInt64)
        {
            Int128 left  = MathUtils.BigMul<T, Int128>(this.Numerator, other.Denominator);
            Int128 right = MathUtils.BigMul<T, Int128>(other.Numerator, this.Denominator);
            return left.CompareTo(right);
        }
        else
        {
            long left  = MathUtils.BigMul<T, long>(this.Numerator, other.Denominator);
            long right = MathUtils.BigMul<T, long>(other.Numerator, this.Denominator);
            return left.CompareTo(right);
        }
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(object? obj) => obj is Fraction<T> fraction ? CompareTo(fraction) : -1;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return this.Denominator != T.One ? $"{this.Numerator} / {this.Denominator}" : this.Numerator.ToString() ?? string.Empty;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return this.Denominator != T.One
                   ? $"{this.Numerator.ToString(format, formatProvider)} / {this.Denominator.ToString(format, formatProvider)}"
                   : this.Numerator.ToString(format, formatProvider);
    }

    /// <inheritdoc />
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (this.Denominator == T.One) return this.Numerator.TryFormat(destination, out charsWritten, format, provider);

        const string SEPARATOR = $" {DEFAULT_SEPARATOR} ";
        if (!this.Numerator.TryFormat(destination, out int numeratorWritten, format, provider))
        {
            charsWritten = 0;
            return false;
        }

        destination = destination[numeratorWritten..];
        if (!SEPARATOR.TryCopyTo(destination))
        {
            charsWritten = 0;
            return false;
        }

        destination = destination[SEPARATOR.Length..];
        if (!this.Denominator.TryFormat(destination, out int denominatorWritten, format, provider))
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = numeratorWritten + denominatorWritten + SEPARATOR.Length;
        return true;
    }

    /// <summary>
    /// Deconstructs this fraction into a numerator/denominator pair
    /// </summary>
    /// <param name="numerator">Numerator output</param>
    /// <param name="denominator">Denominator output</param>
    public void Deconstruct(out T numerator, out T denominator)
    {
        numerator   = this.Numerator;
        denominator = this.Denominator;
    }

    /// <inheritdoc />
    public static bool TryCreate(T numerator, T denominator, out Fraction<T> result)
    {
        if (denominator == T.Zero || (denominator < T.Zero && (numerator == T.MinValue || denominator == T.MinValue)))
        {
            result = default;
            return false;
        }

        result = new Fraction<T>(numerator, denominator);
        return true;
    }

    /// <inheritdoc />
    /// <exception cref="DivideByZeroException">If <paramref name="denominator"/> is <c>0</c></exception>
    /// <exception cref="OverflowException">If the reduced fraction cannot be represented with a positive denominator in <typeparamref name="T"/></exception>
    public static Fraction<T> CreateReduced(T numerator, T denominator)
    {
        if (denominator == T.Zero) throw new DivideByZeroException("Fraction denominator cannot be zero");
        if (numerator == T.Zero) return Zero;

        T gcd;
        if (IsBigInt)
        {
            gcd = MathUtils.GCD(numerator, denominator);
        }
        else if (IsInt128)
        {
            UInt128 numeratorMagnitude   = MathUtils.Magnitude<T, UInt128>(numerator);
            UInt128 denominatorMagnitude = MathUtils.Magnitude<T, UInt128>(denominator);
            gcd = T.CreateChecked(UInt128.GCD(numeratorMagnitude, denominatorMagnitude));
        }
        else if (IsInt64)
        {
            ulong numeratorMagnitude   = MathUtils.Magnitude<T, ulong>(numerator);
            ulong denominatorMagnitude = MathUtils.Magnitude<T, ulong>(denominator);
            gcd = T.CreateChecked(ulong.GCD(numeratorMagnitude, denominatorMagnitude));
        }
        else
        {
            gcd = T.CreateChecked(long.GCD(long.CreateChecked(numerator), long.CreateChecked(denominator)));
        }

        if (gcd != T.One)
        {
            numerator   /= gcd;
            denominator /= gcd;
        }

        if (denominator < T.Zero)
        {
            numerator   = checked(-numerator);
            denominator = checked(-denominator);
        }

        return new Fraction<T>(numerator, denominator);
    }

    /// <summary>
    /// Creates a new integer fraction from a decimal value
    /// </summary>
    /// <param name="value">Decimal value to create the fraction from</param>
    /// <exception cref="OverflowException">If the fraction values to not fit within <typeparamref name="T"/></exception>
    public static Fraction<T> FromDecimal(decimal value)
    {
        // Avoid expensive math for zero
        if (value is 0m) return Zero;

        // Get bits from decimal
        Span<int> bits = stackalloc int[4];
        decimal.GetBits(value, bits);

        // Get numerator
        UInt128 numerator = (uint)bits[2];
        numerator = (numerator << 32) | (uint)bits[1];
        numerator = (numerator << 32) | (uint)bits[0];

        // Get scale and negative flag
        int flags = bits[3];
        int scale = (flags >> 16) & 0xFF;
        bool negative = flags < 0;

        // Nothing else to compute for integer values
        if (scale is 0) return new Fraction<T>(negative ? checked(-T.CreateChecked(numerator)) : T.CreateChecked(numerator));

        // Calculate denominator
        UInt128 denominator = MathUtils.IntegerPow<UInt128>(10UL, scale);

        // Normalize the value
        UInt128 gcd = UInt128.GCD(numerator, denominator);
        numerator   /= gcd;
        denominator /= gcd;

        // Create final values
        return new Fraction<T>(negative ? checked(-T.CreateChecked(numerator)) : T.CreateChecked(numerator), T.CreateChecked(denominator));
    }

    /// <inheritdoc />
    public static Fraction<T> MultiplyReduced(Fraction<T> left, Fraction<T> right)
    {
        checked
        {
            T numerator   = left.Numerator * right.Numerator;
            T denominator = left.Denominator * right.Denominator;
            T gcd = MathUtils.GCD(numerator, denominator);
            return new Fraction<T>(numerator / gcd, denominator / gcd);
        }
    }

    /// <inheritdoc />
    public static Fraction<T> DivideReduced(Fraction<T> left, Fraction<T> right)
    {
        checked
        {
            T numerator   = left.Numerator * right.Denominator;
            T denominator = left.Denominator * right.Numerator;
            T gcd = MathUtils.GCD(numerator, denominator);
            return new Fraction<T>(numerator / gcd, denominator / gcd);
        }
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fraction<T> Abs(Fraction<T> value) => new(T.Abs(value.Numerator), value.Denominator);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInteger(Fraction<T> value) => value.Denominator == T.One;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEvenInteger(Fraction<T> value) => value.Denominator == T.One && value.Numerator.IsEven;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOddInteger(Fraction<T> value) => value.Denominator == T.One && value.Numerator.IsOdd;

    /// <summary>
    /// Parses the fraction using the given value and separator
    /// </summary>
    /// <param name="value">Value to parse</param>
    /// <param name="separator">Number separator, defaults to "/"</param>
    /// <param name="style">Number style</param>
    /// <param name="provider">Format provider</param>
    /// <returns>The parsed fraction</returns>
    /// <exception cref="ArgumentException">If <paramref name="value"/> is empty</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="separator"/> is null or empty</exception>
    /// <exception cref="FormatException">If there isn't exactly two values present after the split</exception>
    public static Fraction<T> Parse(string? value, string separator = DEFAULT_SEPARATOR, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null)
    {
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("Value cannot be empty", nameof(value));
        return Parse(value.AsSpan(), separator, style, provider);
    }

    /// <summary>
    /// Parses the fraction using the given value and separator
    /// </summary>
    /// <param name="value">Value to parse</param>
    /// <param name="separator">Number separator, defaults to "/"</param>
    /// <param name="style">Number style</param>
    /// <param name="provider">Format provider</param>
    /// <returns>The parsed fraction</returns>
    /// <exception cref="ArgumentException">If <paramref name="value"/> is empty</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="separator"/> is null or empty</exception>
    /// <exception cref="FormatException">If there isn't exactly two values present after the split</exception>
    public static Fraction<T> Parse(ReadOnlySpan<char> value, string separator = DEFAULT_SEPARATOR, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null)
    {
        if (value.IsEmpty || value.IsWhiteSpace()) throw new ArgumentException("Value cannot be empty", nameof(value));
        if (string.IsNullOrEmpty(separator)) throw new ArgumentNullException(nameof(separator), "Separator cannot be null or empty");

        int separatorCount = value.Count(separator);
        if (separatorCount is 0) return new Fraction<T>(T.Parse(value, style, provider));
        if (separatorCount is not 1) throw new FormatException("String to parse not properly formatted");

        Span<Range> ranges = stackalloc Range[2];
        int written = value.Split(ranges, separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (written is not 2) throw new FormatException("String to parse not properly formatted");

        T numerator   = T.Parse(value[ranges[0]], style, provider);
        T denominator = T.Parse(value[ranges[1]], style, provider);
        return new Fraction<T>(numerator, denominator);
    }

    /// <summary>
    /// Tries to parse the two fraction using the given value and returns the success
    /// </summary>
    /// <param name="value">Value to parse</param>
    /// <param name="result">Resulting fraction, if any</param>
    /// <param name="separator">Number separator, defaults to "/"</param>
    /// <param name="style">Number style</param>
    /// <param name="provider">Format provider</param>
    /// <returns><see langword="true"/> if the parse succeeded, otherwise <see langword="false"/></returns>
    public static bool TryParse(string? value, out Fraction<T> result, string separator = DEFAULT_SEPARATOR, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = Zero;
            return false;
        }

        return TryParse(value.AsSpan(), out result, separator, style, provider);
    }

    /// <summary>
    /// Tries to parse the two fraction using the given value and returns the success
    /// </summary>
    /// <param name="value">Value to parse</param>
    /// <param name="result">Resulting fraction, if any</param>
    /// <param name="separator">Number separator, defaults to "/"</param>
    /// <param name="style">Number style</param>
    /// <param name="provider">Format provider</param>
    /// <returns><see langword="true"/> if the parse succeeded, otherwise <see langword="false"/></returns>
    public static bool TryParse(ReadOnlySpan<char> value, out Fraction<T> result, string separator = DEFAULT_SEPARATOR, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null)
    {
        if (value.IsEmpty || value.IsWhiteSpace() || string.IsNullOrEmpty(separator))
        {
            result = Zero;
            return false;
        }

        int separatorCount = value.Count(separator);
        if (separatorCount is 0)
        {
            if (T.TryParse(value, style, provider, out T number))
            {
                result = new Fraction<T>(number);
                return true;
            }

            result = Zero;
            return false;
        }

        if (separatorCount is not 1)
        {
            result = Zero;
            return false;
        }

        Span<Range> ranges = stackalloc Range[2];
        int written = value.Split(ranges, separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (written is not 2
         || !T.TryParse(value[ranges[0]], style, provider, out T numerator)
         || !T.TryParse(value[ranges[1]], style, provider, out T denominator))
        {
            result = Zero;
            return false;
        }

        return TryCreate(numerator, denominator, out result);
    }

    /// <summary>
    /// Converts the integer value to a unit fraction
    /// </summary>
    /// <param name="value">Value to convert</param>
    /// <returns>The fraction representation of the given value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Fraction<T>(T value) => new(value);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fraction<T> operator +(Fraction<T> value) => new(+value.Numerator, value.Denominator);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fraction<T> operator -(Fraction<T> value) => new(-value.Numerator, value.Denominator);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fraction<T> operator ++(Fraction<T> value) => new(value.Numerator + value.Denominator, value.Denominator);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fraction<T> operator --(Fraction<T> value) => new(value.Numerator - value.Denominator, value.Denominator);

    /// <inheritdoc />
    public static Fraction<T> operator +(Fraction<T> left, Fraction<T> right)
    {
        if (left.Denominator == right.Denominator) return new Fraction<T>(left.Numerator + right.Numerator, left.Denominator);

        T gcd = MathUtils.GCD(left.Denominator, right.Denominator);
        T leftMultiple  = right.Denominator / gcd;
        T rightMultiple = left.Denominator / gcd;
        return new Fraction<T>((left.Numerator * leftMultiple) + (right.Numerator * rightMultiple), left.Denominator * leftMultiple);
    }

    /// <inheritdoc />
    public static Fraction<T> operator -(Fraction<T> left, Fraction<T> right)
    {
        if (left.Denominator == right.Denominator) return new Fraction<T>(left.Numerator - right.Numerator, left.Denominator);

        T gcd = MathUtils.GCD(left.Denominator, right.Denominator);
        T leftMultiple  = right.Denominator / gcd;
        T rightMultiple = left.Denominator / gcd;
        return new Fraction<T>((left.Numerator * leftMultiple) - (right.Numerator * rightMultiple), left.Denominator * leftMultiple);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fraction<T> operator *(Fraction<T> left, Fraction<T> right) => new(left.Numerator * right.Numerator, left.Denominator * right.Denominator);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fraction<T> operator /(Fraction<T> left, Fraction<T> right) => new(left.Numerator * right.Denominator, left.Denominator * right.Numerator);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fraction<T> operator %(Fraction<T> left, Fraction<T> right)
    {
        T quotient = (left.Numerator * right.Denominator) / (left.Denominator * right.Numerator);
        return new Fraction<T>((left.Numerator * right.Denominator) - (quotient * left.Denominator * right.Numerator), left.Denominator * right.Denominator);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fraction<T> operator +(Fraction<T> left, T right) => new(left.Numerator + (right * left.Denominator), left.Denominator);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fraction<T> operator -(Fraction<T> left, T right) => new(left.Numerator - (right * left.Denominator), left.Denominator);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fraction<T> operator *(Fraction<T> left, T right) => new(left.Numerator * right, left.Denominator);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fraction<T> operator /(Fraction<T> left, T right) => new(left.Numerator, left.Denominator * right);

    /// <summary>
    /// Divides an integer value by a fraction
    /// </summary>
    /// <param name="left">Integer value</param>
    /// <param name="right">Fraction value</param>
    /// <returns>Result of the division of the integer by the fraction</returns>
    public static Fraction<T> operator /(T left, Fraction<T> right) => new(left * right.Denominator, right.Numerator);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Fraction<T> left, Fraction<T> right) => left.Equals(right);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Fraction<T> left, Fraction<T> right) => !left.Equals(right);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Fraction<T> left, Fraction<T> right) => left.CompareTo(right) > 0;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Fraction<T> left, Fraction<T> right) => left.CompareTo(right) >= 0;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Fraction<T> left, Fraction<T> right) => left.CompareTo(right) < 0;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Fraction<T> left, Fraction<T> right) => left.CompareTo(right) <= 0;

    /// <inheritdoc />
    static int INumberBase<Fraction<T>>.Radix
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => T.Radix;
    }

    /// <inheritdoc />
    static Fraction<T> IAdditiveIdentity<Fraction<T>, Fraction<T>>.AdditiveIdentity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Zero;
    }

    /// <inheritdoc />
    static Fraction<T> IMultiplicativeIdentity<Fraction<T>, Fraction<T>>.MultiplicativeIdentity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => One;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Fraction<T>>.IsZero(Fraction<T> value) => value.Numerator == T.Zero;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Fraction<T>>.IsNegative(Fraction<T> value) => value.Numerator < T.Zero;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Fraction<T>>.IsPositive(Fraction<T> value) => value.Numerator >= T.Zero;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Fraction<T>>.IsCanonical(Fraction<T> value) => value.IsReduced;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Fraction<T>>.IsNormal(Fraction<T> value) => value.Numerator != T.Zero;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Fraction<T>>.IsSubnormal(Fraction<T> value) => false;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Fraction<T>>.IsRealNumber(Fraction<T> value) => true;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Fraction<T>>.IsComplexNumber(Fraction<T> value) => false;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Fraction<T>>.IsImaginaryNumber(Fraction<T> value) => false;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Fraction<T>>.IsNaN(Fraction<T> value) => false;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Fraction<T>>.IsFinite(Fraction<T> value) => true;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Fraction<T>>.IsInfinity(Fraction<T> value) => false;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Fraction<T>>.IsNegativeInfinity(Fraction<T> value) => false;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Fraction<T>>.IsPositiveInfinity(Fraction<T> value) => false;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Fraction<T> INumberBase<Fraction<T>>.MaxMagnitude(Fraction<T> x, Fraction<T> y) => Fraction<T>.Max(x, y);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Fraction<T> INumberBase<Fraction<T>>.MaxMagnitudeNumber(Fraction<T> x, Fraction<T> y) => Fraction<T>.Max(x, y);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Fraction<T> INumberBase<Fraction<T>>.MinMagnitude(Fraction<T> x, Fraction<T> y) => Fraction<T>.Min(x, y);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Fraction<T> INumberBase<Fraction<T>>.MinMagnitudeNumber(Fraction<T> x, Fraction<T> y) => Fraction<T>.Min(x, y);

    /// <inheritdoc />
    bool IFraction.TryConvertChecked<TTarget>(out TTarget numerator, out TTarget denominator)
    {
        if (TTarget.TryConvertFromChecked(this.Numerator, out TTarget convertedNumerator)
         && TTarget.TryConvertFromChecked(this.Denominator, out TTarget convertedDenominator)
         && convertedDenominator > TTarget.Zero)
        {
            numerator   = convertedNumerator;
            denominator = convertedDenominator;
            return true;
        }

        numerator   = default;
        denominator = default;
        return false;
    }

    /// <inheritdoc />
    bool IFraction.TryConvertSaturating<TTarget>(out TTarget numerator, out TTarget denominator)
    {
        if (TTarget.TryConvertFromSaturating(this.Numerator, out TTarget convertedNumerator)
         && TTarget.TryConvertFromSaturating(this.Denominator, out TTarget convertedDenominator)
         && convertedDenominator > TTarget.Zero)
        {
            numerator   = convertedNumerator;
            denominator = convertedDenominator;
            return true;
        }

        numerator   = default;
        denominator = default;
        return false;
    }

    /// <inheritdoc />
    bool IFraction.TryConvertTruncating<TTarget>(out TTarget numerator, out TTarget denominator)
    {
        if (TTarget.TryConvertFromTruncating(this.Numerator, out TTarget convertedNumerator)
         && TTarget.TryConvertFromTruncating(this.Denominator, out TTarget convertedDenominator)
         && convertedDenominator > TTarget.Zero)
        {
            numerator   = convertedNumerator;
            denominator = convertedDenominator;
            return true;
        }

        numerator   = default;
        denominator = default;
        return false;
    }

    /// <inheritdoc />
    static bool INumberBase<Fraction<T>>.TryConvertFromChecked<TOther>(TOther value, out Fraction<T> result)
    {
        if (value is IFraction fraction)
        {
            if (fraction.TryConvertChecked(out T numerator, out T denominator) && TryCreate(numerator, denominator, out result))
            {
                return true;
            }

            result = default;
            return false;
        }

        if (typeof(IFloatingPoint<>).MakeGenericType(typeof(TOther)).IsAssignableFrom(typeof(TOther))
         && TOther.TryConvertToChecked(value, out decimal decimalValue))
        {
            try
            {
                result = FromDecimal(decimalValue);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        if (TOther.TryConvertToChecked(value, out T number))
        {
            result = new Fraction<T>(number);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    /// ReSharper disable once CognitiveComplexity
    static bool INumberBase<Fraction<T>>.TryConvertFromSaturating<TOther>(TOther value, out Fraction<T> result)
    {
        if (value is IFraction fraction)
        {
            if (fraction.TryConvertSaturating(out T numerator, out T denominator) && TryCreate(numerator, denominator, out result))
            {
                return true;
            }

            result = default;
            return false;
        }

        if (typeof(IFloatingPoint<>).MakeGenericType(typeof(TOther)).IsAssignableFrom(typeof(TOther))
         && TOther.TryConvertToSaturating(value, out decimal decimalValue))
        {
            try
            {
                result = FromDecimal(decimalValue);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        if (TOther.TryConvertToSaturating(value, out T number))
        {
            result = new Fraction<T>(number);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    static bool INumberBase<Fraction<T>>.TryConvertFromTruncating<TOther>(TOther value, out Fraction<T> result)
    {
        if (value is IFraction fraction)
        {
            if (fraction.TryConvertTruncating(out T numerator, out T denominator) && TryCreate(numerator, denominator, out result))
            {
                return true;
            }

            result = default;
            return false;
        }

        if (typeof(IFloatingPoint<>).MakeGenericType(typeof(TOther)).IsAssignableFrom(typeof(TOther))
         && TOther.TryConvertToTruncating(value, out decimal decimalValue))
        {
            try
            {
                result = FromDecimal(decimalValue);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        if (TOther.TryConvertToTruncating(value, out T number))
        {
            result = new Fraction<T>(number);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    static bool INumberBase<Fraction<T>>.TryConvertToChecked<TOther>(Fraction<T> value, [MaybeNullWhen(false)] out TOther result)
    {
        if (typeof(IFraction).IsAssignableFrom(typeof(TOther)))
        {
            return TOther.TryConvertFromChecked(value, out result);
        }

        if (value.Denominator == T.One)
        {
            return TOther.TryConvertFromChecked(value.Numerator, out result);
        }

        if (typeof(IFloatingPoint<>).MakeGenericType(typeof(TOther)).IsAssignableFrom(typeof(TOther)))
        {
            return TOther.TryConvertFromChecked(value.Decimal, out result);
        }

        return TOther.TryConvertFromChecked(value.Numerator / value.Denominator, out result);
    }

    /// <inheritdoc />
    static bool INumberBase<Fraction<T>>.TryConvertToSaturating<TOther>(Fraction<T> value, [MaybeNullWhen(false)] out TOther result)
    {
        if (typeof(IFraction).IsAssignableFrom(typeof(TOther)))
        {
            return TOther.TryConvertFromSaturating(value, out result);
        }

        if (value.Denominator == T.One)
        {
            return TOther.TryConvertFromSaturating(value.Numerator, out result);
        }

        if (typeof(IFloatingPoint<>).MakeGenericType(typeof(TOther)).IsAssignableFrom(typeof(TOther)))
        {
            return TOther.TryConvertFromSaturating(value.Decimal, out result);
        }

        return TOther.TryConvertFromSaturating(value.Numerator / value.Denominator, out result);
    }

    /// <inheritdoc />
    static bool INumberBase<Fraction<T>>.TryConvertToTruncating<TOther>(Fraction<T> value, [MaybeNullWhen(false)] out TOther result)
    {
        if (typeof(IFraction).IsAssignableFrom(typeof(TOther)))
        {
            return TOther.TryConvertFromTruncating(value, out result);
        }

        if (value.Denominator == T.One)
        {
            return TOther.TryConvertFromTruncating(value.Numerator, out result);
        }

        if (typeof(IFloatingPoint<>).MakeGenericType(typeof(TOther)).IsAssignableFrom(typeof(TOther)))
        {
            return TOther.TryConvertFromTruncating(value.Decimal, out result);
        }

        return TOther.TryConvertFromTruncating(value.Numerator / value.Denominator, out result);
    }

    /// <inheritdoc />
    static Fraction<T> IParsable<Fraction<T>>.Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), provider: provider);

    /// <inheritdoc />
    static Fraction<T> INumberBase<Fraction<T>>.Parse(string s, NumberStyles style, IFormatProvider? provider) => Parse(s.AsSpan(), style: style, provider: provider);

    /// <inheritdoc />
    static Fraction<T> ISpanParsable<Fraction<T>>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s, provider: provider);

    /// <inheritdoc />
    static Fraction<T> INumberBase<Fraction<T>>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => Parse(s, style: style, provider: provider);

    /// <inheritdoc />
    static bool IParsable<Fraction<T>>.TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Fraction<T> result)
    {
        if (string.IsNullOrEmpty(s))
        {
            result = Zero;
            return false;
        }

        return TryParse(s.AsSpan(), out result, provider: provider);
    }

    /// <inheritdoc />
    static bool INumberBase<Fraction<T>>.TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out Fraction<T> result)
    {
        if (string.IsNullOrEmpty(s))
        {
            result = Zero;
            return false;
        }

        return TryParse(s.AsSpan(), out result, style: style, provider: provider);
    }

    /// <inheritdoc />
    static bool ISpanParsable<Fraction<T>>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Fraction<T> result)
    {
        return TryParse(s, out result, provider: provider);
    }

    /// <inheritdoc />
    static bool INumberBase<Fraction<T>>.TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Fraction<T> result)
    {
        return TryParse(s, out result, style: style, provider: provider);
    }
}
