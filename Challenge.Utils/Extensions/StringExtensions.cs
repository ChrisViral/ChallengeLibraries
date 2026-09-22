using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Challenge.Utils.Extensions.Spans;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Challenge.Utils.Extensions.Strings;

/// <summary>
/// <see cref="string"/> and other related classes extensions
/// </summary>
[PublicAPI]
public static class StringExtensions
{
    /// <param name="value">Character value</param>
    extension(char value)
    {
        /// <summary>
        /// Converts this character to a letter index with 'a' as 0<br/>
        /// This handles uppercase and lowercase ASCII letters as well as ASCII digits
        /// </summary>
        /// <exception cref="ArgumentException">If the value is neither a lower or upper ASCII letter, and not an ASCII digit</exception>
        public int AsIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (char.IsAsciiLetterUpper(value)) return value - StringUtils.ASCII_UPPER[0];
                if (char.IsAsciiLetterLower(value)) return value - StringUtils.ASCII_LOWER[0];
                if (char.IsAsciiDigit(value)) return value - StringUtils.ASCII_DIGITS[0];

                throw new ArgumentException("Value must be an ASCII letter to convert to index", nameof(value));
            }
        }

        /// <inheritdoc cref="char.IsAsciiLetter" />
        public bool IsLetterChar
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => char.IsAsciiLetter(value);
        }

        /// <inheritdoc cref="char.IsAsciiLetterLower" />
        public bool IsLowerChar
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => char.IsAsciiLetterLower(value);
        }

        /// <inheritdoc cref="char.IsAsciiLetterUpper" />
        public bool IsUpperChar
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => char.IsAsciiLetterUpper(value);
        }

        /// <inheritdoc cref="char.IsAsciiDigit" />
        public bool IsDigitChar
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => char.IsAsciiDigit(value);
        }

        /// <inheritdoc cref="char.IsAsciiLetterOrDigit" />
        public bool IsLetterOrDigitChar
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => char.IsAsciiLetterOrDigit(value);
        }

        /// <inheritdoc cref="char.IsWhiteSpace(char)" />
        public bool IsWhiteSpaceChar
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => char.IsWhiteSpace(value);
        }

        /// <inheritdoc cref="char.ToLowerInvariant" />
        public char ToLowerChar
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => char.ToLowerInvariant(value);
        }

        /// <inheritdoc cref="char.ToUpperInvariant" />
        public char ToUpperChar
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => char.ToUpperInvariant(value);
        }
    }

    /// <param name="value">Numerical value</param>
    /// <typeparam name="T">Integer type</typeparam>
    extension<T>(T value) where T : IBinaryInteger<T>
    {
        /// <summary>
        /// Converts this character to it's matching lower ASCII letter
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the value is less than zero, or greater than the highest letter value</exception>
        public char AsAsciiLower
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (value < T.Zero || value >= T.CreateChecked(StringUtils.LETTER_COUNT)) throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be within ASCII letter values range");
                return (char)(StringUtils.ASCII_LOWER[0] + int.CreateChecked(value));
            }
        }

        /// <summary>
        /// Converts this character to it's matching upper ASCII letter
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the value is less than zero, or greater than the highest letter value</exception>
        public char AsAsciiUpper
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (value < T.Zero || value >= T.CreateChecked(StringUtils.LETTER_COUNT)) throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be within ASCII letter values range");
                return (char)(StringUtils.ASCII_UPPER[0] + int.CreateChecked(value));
            }
        }

        /// <summary>
        /// Converts this character to it's matching ASCII digit
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the value is less than zero, or greater than the highest digit value</exception>
        public char AsAsciiDigit
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (value < T.Zero || value >= T.CreateChecked(StringUtils.DIGIT_COUNT)) throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be within ASCII letter values range");
                return (char)(StringUtils.ASCII_DIGITS[0] + int.CreateChecked(value));
            }
        }
    }

    /// <param name="value">String value</param>
    extension(string value)
    {
        /// <summary>
        /// If this string is empty
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value.Length is 0;
        }

        /// <summary>
        /// If this string is a palindrome or not (same values when read in either direction)
        /// </summary>
        /// <returns><see langword="true"/> if this string contains a palindrome, otherwise <see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPalindrome() => value.AsSpan().IsPalindrome();
    }

    /// <param name="stringBuilder">StringBuilder value</param>
    extension(StringBuilder stringBuilder)
    {
        /// <summary>
        /// Compiles the StringBuilder to it's contained value, then clears it
        /// </summary>
        /// <returns>The compiled string contained in this StringBuilder</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToStringAndClear()
        {
            string toString = stringBuilder.ToString();
            stringBuilder.Clear();
            return toString;
        }

        /// <summary>
        /// Copies data from the start of the StringBuilder to fill the given span
        /// </summary>
        /// <param name="destination">Span to fill</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<char> destination) => stringBuilder.CopyTo(0, destination, destination.Length);

        /// <summary>
        /// Counts the amount of instance of the specified character in the  StringBuilder
        /// </summary>
        /// <param name="value">Value to count</param>
        /// <returns>The amount of times <paramref name="value"/> is found within the StringBuilder</returns>
        public int Count(char value)
        {
            int count = 0;
            for (int i = 0; i < stringBuilder.Length; i++)
            {
                if (stringBuilder[i] == value)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
