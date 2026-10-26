using System.Numerics;
using System.Runtime.CompilerServices;
using Challenge.Maths.Vectors;
using Challenge.Utils;
using JetBrains.Annotations;

namespace Challenge.Maths;

/// <summary>
/// Orthogonal line segment
/// </summary>
[PublicAPI]
public readonly struct OrthogonalLine : IEquatable<OrthogonalLine>, IEqualityOperators<OrthogonalLine, OrthogonalLine, bool>
{
    /// <summary>Line start point</summary>
    public Vector2<int> Start { get; }

    /// <summary>Line end point</summary>
    public Vector2<int> End { get; }

    /// <summary>
    /// If this line is fully horizontal
    /// </summary>
    public bool IsHorizontal { get; }

    /// <summary>
    /// If this line is fully vertical
    /// </summary>
    public bool IsVertical { get; }

    /// <summary>
    /// Orthogonal line segment
    /// </summary>
    /// <param name="start">Line start point</param>
    /// <param name="end">Line end point</param>
    /// <exception cref="InvalidOperationException">If the points do not form an orthogonal line</exception>
    public OrthogonalLine(Vector2<int> start, Vector2<int> end)
    {
        this.IsHorizontal = start.Y == end.Y;
        this.IsVertical   = start.X == end.X;
        if (!this.IsHorizontal && !this.IsVertical) throw new InvalidOperationException("Orthogonal line requires the start and end point to share an axis");

        SwapUtils.SwapOrdered(ref start, ref end, ComponentComparer<int>.Instance);
        this.Start = start;
        this.End   = end;
    }

    /// <summary>
    /// Checks if a given point is on the specified line
    /// </summary>
    /// <param name="point">Point to check</param>
    /// <returns><see langword="true"/> if <paramref name="point"/> is on the line, otherwise <see langword="false"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOnLine(Vector2<int> point)
    {
        return this.IsHorizontal
                   ? point.Y == this.Start.Y && point.X >= this.Start.X && point.X <= this.End.X
                   : point.X == this.Start.X && point.Y >= this.Start.Y && point.Y <= this.End.Y;
    }

    /// <summary>
    /// Deconstructs this line into it's start and end points
    /// </summary>
    /// <param name="start">Start point</param>
    /// <param name="end">End point</param>
    public void Deconstruct(out Vector2<int> start, out Vector2<int> end)
    {
        start = this.Start;
        end   = this.End;
    }

    /// <inheritdoc />
    public bool Equals(OrthogonalLine other) => this.Start == other.Start && this.End == other.End;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is OrthogonalLine other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(this.Start, this.End);

    /// <inheritdoc />
    public override string ToString() => $"{this.Start} <=> {this.End}";

    /// <summary>
    /// Checks if two lines intersect
    /// </summary>
    /// <param name="a">First line to check</param>
    /// <param name="b">Second line to check</param>
    /// <returns><see langword="true"/> if <paramref name="a"/> intersects <paramref name="b"/>, otherwise <see langword="false"/></returns>
    public static bool Intersects(OrthogonalLine a, OrthogonalLine b)
    {
        if (a == b) return true;
        if (a.IsHorizontal == b.IsHorizontal) return a.IsOnLine(b.Start) || b.IsOnLine(a.Start);

        // Make sure a is the vertical line
        if (!a.IsVertical)
        {
            SwapUtils.Swap(ref a, ref b);
        }

        Vector2<int> intersection = new(a.Start.X, b.Start.Y);
        return a.IsOnLine(intersection) && b.IsOnLine(intersection);
    }

    /// <inheritdoc />
    public static bool operator ==(OrthogonalLine left, OrthogonalLine right) => left.Equals(right);

    /// <inheritdoc />
    public static bool operator !=(OrthogonalLine left, OrthogonalLine right) => !left.Equals(right);
}
