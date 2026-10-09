using System.Collections.Immutable;
using Challenge.Utils.Extensions.Enums;
using Challenge.Utils.Extensions.Ranges;
using JetBrains.Annotations;

namespace Challenge.Utils;

/// <summary>
/// Parsing utility methods
/// </summary>
[PublicAPI]
public static class ParseUtils
{
    /// <summary>
    /// Default string splitting options
    /// </summary>
    private const StringSplitOptions DEFAULT_OPTIONS = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator character</param>
    /// <param name="options">String split options</param>
    /// <typeparam name="T">Output type</typeparam>
    /// <returns>An array of the parsed data</returns>
    public static string[] ParseArray<T>(ReadOnlySpan<char> line, char separator, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;
        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            return !line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries) ? [line.ToString()] : [];
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);

        string[] result = new string[count];
        foreach (int i in ..count)
        {
            result[i] = line[splits[i]].ToString();
        }

        return result;
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator string</param>
    /// <param name="options">String split options</param>
    /// <typeparam name="T">Output type</typeparam>
    /// <returns>An array of the parsed data</returns>
    public static string[] ParseArray<T>(ReadOnlySpan<char> line, ReadOnlySpan<char> separator, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;
        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            return !line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries) ? [line.ToString()] : [];
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);

        string[] result = new string[count];
        foreach (int i in ..count)
        {
            result[i] = line[splits[i]].ToString();
        }

        return result;
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator character</param>
    /// <param name="converter">Final output conversion function</param>
    /// <param name="options">String split options</param>
    /// <typeparam name="T">Output type</typeparam>
    /// <returns>An array of the parsed data</returns>
    public static T[] ParseArray<T>(ReadOnlySpan<char> line, char separator, Converter<ReadOnlySpan<char>, T> converter, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;
        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            return !line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries) ? [converter(line)] : [];
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);

        T[] result = new T[count];
        foreach (int i in ..count)
        {
            result[i] = converter(line[splits[i]]);
        }

        return result;
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator string</param>
    /// <param name="converter">Final output conversion function</param>
    /// <param name="options">String split options</param>
    /// <typeparam name="T">Output type</typeparam>
    /// <returns>An array of the parsed data</returns>
    public static T[] ParseArray<T>(ReadOnlySpan<char> line, ReadOnlySpan<char> separator, Converter<ReadOnlySpan<char>, T> converter, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;
        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            return !line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries) ? [converter(line)] : [];
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);

        T[] result = new T[count];
        foreach (int i in ..count)
        {
            result[i] = converter(line[splits[i]]);
        }

        return result;
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator character</param>
    /// <param name="options">String split options</param>
    /// <typeparam name="T">Output type</typeparam>
    /// <returns>An array of the parsed data</returns>
    public static ImmutableArray<string> ParseImmutableArray<T>(ReadOnlySpan<char> line, char separator, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;
        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            return !line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries) ? [line.ToString()] : [];
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);

        ImmutableArray<string>.Builder result = ImmutableArray.CreateBuilder<string>(count);
        foreach (int i in ..count)
        {
            result.Add(line[splits[i]].ToString());
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator string</param>
    /// <param name="options">String split options</param>
    /// <typeparam name="T">Output type</typeparam>
    /// <returns>An array of the parsed data</returns>
    public static ImmutableArray<string> ParseImmutableArray<T>(ReadOnlySpan<char> line, ReadOnlySpan<char> separator, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;
        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            return !line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries) ? [line.ToString()] : [];
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);

        ImmutableArray<string>.Builder result = ImmutableArray.CreateBuilder<string>(count);
        foreach (int i in ..count)
        {
            result.Add(line[splits[i]].ToString());
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator character</param>
    /// <param name="converter">Final output conversion function</param>
    /// <param name="options">String split options</param>
    /// <typeparam name="T">Output type</typeparam>
    /// <returns>An array of the parsed data</returns>
    public static ImmutableArray<T> ParseImmutableArray<T>(ReadOnlySpan<char> line, char separator, Converter<ReadOnlySpan<char>, T> converter, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;
        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            return !line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries) ? [converter(line)] : [];
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);

        ImmutableArray<T>.Builder result = ImmutableArray.CreateBuilder<T>(count);
        foreach (int i in ..count)
        {
            result.Add(converter(line[splits[i]]));
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator string</param>
    /// <param name="converter">Final output conversion function</param>
    /// <param name="options">String split options</param>
    /// <typeparam name="T">Output type</typeparam>
    /// <returns>An array of the parsed data</returns>
    public static ImmutableArray<T> ParseImmutableArray<T>(ReadOnlySpan<char> line, ReadOnlySpan<char> separator, Converter<ReadOnlySpan<char>, T> converter, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;
        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            return !line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries) ? [converter(line)] : [];
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);

        ImmutableArray<T>.Builder result = ImmutableArray.CreateBuilder<T>(count);
        foreach (int i in ..count)
        {
            result.Add(converter(line[splits[i]]));
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator character</param>
    /// <param name="result">Output result span</param>
    /// <param name="options">String split options</param>
    /// <returns>The parsed data count</returns>
    /// <exception cref="ArgumentException">If <paramref name="result"/> is too small to accomodate the parsed data</exception>
    public static int ParseSpan(ReadOnlySpan<char> line, char separator, scoped Span<string> result, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;
        if (result.Length < count) throw new ArgumentException("Result span too small for data", nameof(result));

        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            if (!line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries))
            {
                result[0] = line.ToString();
                return 1;
            }

            return 0;
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);
        foreach (int i in ..count)
        {
            result[i] = line[splits[i]].ToString();
        }

        return count;
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator string</param>
    /// <param name="result">Output result span</param>
    /// <param name="options">String split options</param>
    /// <returns>The parsed data count</returns>
    /// <exception cref="ArgumentException">If <paramref name="result"/> is too small to accomodate the parsed data</exception>
    public static int ParseSpan(ReadOnlySpan<char> line, ReadOnlySpan<char> separator, scoped Span<string> result, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;
        if (result.Length < count) throw new ArgumentException("Result span too small for data", nameof(result));

        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            if (!line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries))
            {
                result[0] = line.ToString();
                return 1;
            }

            return 0;
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);
        foreach (int i in ..count)
        {
            result[i] = line[splits[i]].ToString();
        }

        return count;
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator character</param>
    /// <param name="converter">Final output conversion function</param>
    /// <param name="result">Output result span</param>
    /// <param name="options">String split options</param>
    /// <typeparam name="T">Output type</typeparam>
    /// <returns>The parsed data count</returns>
    /// <exception cref="ArgumentException">If <paramref name="result"/> is too small to accomodate the parsed data</exception>
    public static int ParseSpan<T>(ReadOnlySpan<char> line, char separator, Converter<ReadOnlySpan<char>, T> converter, scoped Span<T> result, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;
        if (result.Length < count) throw new ArgumentException("Result span too small for data", nameof(result));

        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            if (!line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries))
            {
                result[0] = converter(line);
                return 1;
            }

            return 0;
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);
        foreach (int i in ..count)
        {
            result[i] = converter(line[splits[i]]);
        }

        return count;
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator string</param>
    /// <param name="converter">Final output conversion function</param>
    /// <param name="result">Output result span</param>
    /// <param name="options">String split options</param>
    /// <typeparam name="T">Output type</typeparam>
    /// <returns>The parsed data count</returns>
    /// <exception cref="ArgumentException">If <paramref name="result"/> is too small to accomodate the parsed data</exception>
    public static int ParseSpan<T>(ReadOnlySpan<char> line, ReadOnlySpan<char> separator, Converter<ReadOnlySpan<char>, T> converter, scoped Span<T> result, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;
        if (result.Length < count) throw new ArgumentException("Result span too small for data", nameof(result));

        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            if (!line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries))
            {
                result[0] = converter(line);
                return 1;
            }

            return 0;
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);
        foreach (int i in ..count)
        {
            result[i] = converter(line[splits[i]]);
        }

        return count;
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator character</param>
    /// <param name="result">Output collection</param>
    /// <param name="options">String split options</param>
    /// <returns>The parsed data count</returns>
    /// <exception cref="ArgumentException">If <paramref name="result"/> is too small to accomodate the parsed data</exception>
    public static int ParseCollection(ReadOnlySpan<char> line, char separator, in ICollection<string> result, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;
        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            if (!line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries))
            {
                result.Add(line.ToString());
                return 1;
            }

            return 0;
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);
        foreach (int i in ..count)
        {
            result.Add(line[splits[i]].ToString());
        }

        return count;
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator string</param>
    /// <param name="result">Output collection</param>
    /// <param name="options">String split options</param>
    /// <returns>The parsed data count</returns>
    /// <exception cref="ArgumentException">If <paramref name="result"/> is too small to accomodate the parsed data</exception>
    public static int ParseCollection(ReadOnlySpan<char> line, ReadOnlySpan<char> separator, in ICollection<string> result, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;

        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            if (!line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries))
            {
                result.Add(line.ToString());
                return 1;
            }

            return 0;
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);
        foreach (int i in ..count)
        {
            result.Add(line[splits[i]].ToString());
        }

        return count;
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator character</param>
    /// <param name="converter">Final output conversion function</param>
    /// <param name="result">Output collection</param>
    /// <param name="options">String split options</param>
    /// <typeparam name="T">Output type</typeparam>
    /// <returns>The parsed data count</returns>
    /// <exception cref="ArgumentException">If <paramref name="result"/> is too small to accomodate the parsed data</exception>
    public static int ParseCollection<T>(ReadOnlySpan<char> line, char separator, Converter<ReadOnlySpan<char>, T> converter, in ICollection<T> result, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;

        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            if (!line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries))
            {
                result.Add(converter(line));
                return 1;
            }

            return 0;
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);
        foreach (int i in ..count)
        {
            result.Add(converter(line[splits[i]]));
        }

        return count;
    }

    /// <summary>
    /// Parses the given array from a string
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="separator">Separator string</param>
    /// <param name="converter">Final output conversion function</param>
    /// <param name="result">Output collection</param>
    /// <param name="options">String split options</param>
    /// <typeparam name="T">Output type</typeparam>
    /// <returns>The parsed data count</returns>
    /// <exception cref="ArgumentException">If <paramref name="result"/> is too small to accomodate the parsed data</exception>
    public static int ParseCollection<T>(ReadOnlySpan<char> line, ReadOnlySpan<char> separator, Converter<ReadOnlySpan<char>, T> converter, in ICollection<T> result, StringSplitOptions options = DEFAULT_OPTIONS)
    {
        int count = line.Count(separator) + 1;

        if (count is 1)
        {
            if (options.HasFlags(StringSplitOptions.TrimEntries))
            {
                line = line.Trim();
            }

            if (!line.IsEmpty || !options.HasFlags(StringSplitOptions.RemoveEmptyEntries))
            {
                result.Add(converter(line));
                return 1;
            }

            return 0;
        }

        Span<Range> splits = stackalloc Range[count];
        count = line.Split(splits, separator, options);
        foreach (int i in ..count)
        {
            result.Add(converter(line[splits[i]]));
        }

        return count;
    }
}
