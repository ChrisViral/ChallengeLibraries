using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Challenge.Maths.Directions;

/// <summary>
/// Movement direction and distance instruction
/// </summary>
/// <param name="Direction">Movement direction</param>
/// <param name="Distance">Movement distance</param>
[PublicAPI]
public readonly record struct DirectionInstruction(Direction Direction, int Distance) : ISpanParsable<DirectionInstruction>
{
    /// <inheritdoc cref="IParsable{TSelf}.Parse" />
    public static DirectionInstruction Parse(string s) => Parse(s.AsSpan(), null);

    /// <inheritdoc />
    public static DirectionInstruction Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), provider);

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse" />
    public static DirectionInstruction Parse(ReadOnlySpan<char> s) => Parse(s, null);

    /// <inheritdoc />
    public static DirectionInstruction Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        Direction direction = Direction.ParseDirection(s[0]);
        int distance = int.Parse(s[1..], provider);
        return new DirectionInstruction(direction, distance);
    }

    /// <inheritdoc cref="IParsable{TSelf}.TryParse" />
    public static bool TryParse([NotNullWhen(true)] string? s, out DirectionInstruction result) => TryParse(s, null, out result);

    /// <inheritdoc />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out DirectionInstruction result)
    {
        if (!string.IsNullOrEmpty(s)) return TryParse(s.AsSpan(), provider, out result);

        result = default;
        return false;
    }

    /// <inheritdoc cref="ISpanParsable{TSelf}.TryParse" />
    public static bool TryParse(ReadOnlySpan<char> s, out DirectionInstruction result) => TryParse(s, null, out result);

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out DirectionInstruction result)
    {
        if (Direction.TryParseDirection(s[0], out Direction direction)
         && int.TryParse(s[1..], provider, out int distance))
        {
            result = new DirectionInstruction(direction, distance);
            return true;
        }

        result = default;
        return false;
    }
}
