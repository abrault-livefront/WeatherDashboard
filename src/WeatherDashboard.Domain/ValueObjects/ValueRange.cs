namespace WeatherDashboard.Domain.ValueObjects;

using System.Runtime.InteropServices;

/// <summary>
///     Represents an inclusive range between two comparable values.
/// </summary>
/// <typeparam name="T">The type of values in the range. Must implement <see cref="IComparable{T}" />.</typeparam>
[StructLayout(LayoutKind.Auto)]
public readonly record struct ValueRange<T>
    where T : IComparable<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ValueRange{T}" /> struct.
    /// </summary>
    /// <param name="min">The minimum value of the range.</param>
    /// <param name="max">The maximum value of the range.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max" /> is less than <paramref name="min" />.</exception>
    public ValueRange(T min, T max)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(max, min);

        Min = min;
        Max = max;
    }

    /// <summary>
    ///     Gets the maximum value of the range (inclusive).
    /// </summary>
    public T Max { get; }

    /// <summary>
    ///     Gets the minimum value of the range (inclusive).
    /// </summary>
    public T Min { get; }

    /// <summary>
    ///     Determines whether the specified value is within the range (inclusive).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    ///     <see langword="true" /> if the value is greater than or equal to <see cref="Min" /> and less than or equal to
    ///     <see cref="Max" />; otherwise, <see langword="false" />.
    /// </returns>
    public bool Contains(T value)
    {
        if ( value.CompareTo(Min) < 0 )
        {
            return false;
        }

        return value.CompareTo(Max) <= 0;
    }

    /// <summary>
    ///     Determines whether this range overlaps with another range.
    /// </summary>
    /// <param name="other">The range to check for overlap.</param>
    /// <returns>
    ///     <see langword="true" /> if the ranges overlap (share any values in common); otherwise,
    ///     <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     Two ranges overlap if: Min &lt;= other.Max AND other.Min &lt;= Max.
    ///     This includes cases where ranges share only boundary values.
    /// </remarks>
    public bool OverlapsWith(ValueRange<T> other)
    {
        return Min.CompareTo(other.Max) <= 0 && other.Min.CompareTo(Max) <= 0;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[{Min}, {Max}]";
    }
}
