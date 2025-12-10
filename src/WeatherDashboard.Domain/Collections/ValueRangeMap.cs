#pragma warning disable CA1710

namespace WeatherDashboard.Domain.Collections;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using ValueObjects;

/// <summary>
///     Represents an immutable collection that maps ranges of comparable values to associated values,
///     enabling efficient lookup of values based on whether a key falls within a defined range.
/// </summary>
/// <typeparam name="TKey">The type of the range boundaries, which must implement <see cref="IComparable{T}" />.</typeparam>
/// <typeparam name="TValue">The type of the values associated with each range. Must be a non-nullable reference type.</typeparam>
/// <remarks>
///     <para>
///         This collection supports a mutable construction phase via <see cref="Add" />, after which it becomes immutable.
///         Initialization occurs lazily on the first enumeration or lookup operation. After initialization,
///         use <see cref="With" /> to create new instances with modifications.
///     </para>
///     <para>
///         When a key is looked up using <see cref="GetValueOrDefault" /> or <see cref="TryGetValue" />, the map searches
///         sequentially through all ranges to find one that contains the key and returns the associated value.
///     </para>
/// </remarks>
public sealed class ValueRangeMap<TKey, TValue> : IReadOnlyCollection<KeyValuePair<ValueRange<TKey>, TValue>>
    where TKey : IComparable<TKey>
    where TValue : notnull
{
    private readonly Dictionary<ValueRange<TKey>, TValue> _map;

    private bool _isInitialized;

    private ValueRange<TKey>[] _ranges = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValueRangeMap{TRange,TValue}" /> class with the specified items.
    /// </summary>
    /// <param name="items">The collection of range-value pairs to initialize the map with.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown when duplicate ranges are provided in <paramref name="items" />,
    ///     or when any ranges overlap with each other.
    /// </exception>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public ValueRangeMap(IEnumerable<(ValueRange<TKey> range, TValue value)> items)
    {
        ( ValueRange<TKey> range, TValue value )[] itemsArray = items.ToArray();

        for ( int i = 0; i < itemsArray.Length; i++ )
        {
            for ( int j = i + 1; j < itemsArray.Length; j++ )
            {
                if ( itemsArray[i].range.OverlapsWith(itemsArray[j].range) )
                {
                    throw new ArgumentException(
                        $"Range {itemsArray[i].range} overlaps with range {itemsArray[j].range}.",
                        nameof(items));
                }
            }
        }

        _map = itemsArray.ToDictionary(d => d.range, d => d.value);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValueRangeMap{TRange,TValue}" /> class with an empty collection.
    /// </summary>
    [SuppressMessage("ReSharper", "UseCollectionExpression")]
    public ValueRangeMap()
        : this(Enumerable.Empty<(ValueRange<TKey> range, TValue value)>())
    {
    }

    /// <summary>
    ///     Gets the number of range-value pairs in the <see cref="ValueRangeMap{TKey,TValue}" />.
    /// </summary>
    public int Count
    {
        get
        {
            EnsureInitialized();
            return _map.Count;
        }
    }

    /// <summary>
    ///     Gets a read-only span of all ranges currently in the map.
    /// </summary>
    private ReadOnlySpan<ValueRange<TKey>> Ranges
    {
        get
        {
            EnsureInitialized();
            return _ranges;
        }
    }

    /// <summary>
    ///     Adds or updates a range-value pair in the map. This operation is only allowed during construction.
    /// </summary>
    /// <param name="valueRange">The range to add or update.</param>
    /// <param name="value">The value to associate with the range.</param>
    /// <remarks>
    ///     <para>
    ///         This method can only be called before the map is finalized (i.e., before any lookup operation).
    ///         Once initialized through enumeration or lookup, attempting to call this method will throw an exception.
    ///     </para>
    ///     <para>
    ///         If the range already exists, its value will be updated. After the map is finalized,
    ///         use the <see cref="With" /> method to create a new instance with additional ranges.
    ///     </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when attempting to add after the map has been initialized through enumeration or lookup operations.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="valueRange" /> overlaps with an existing range in the map.
    /// </exception>
    public void Add(ValueRange<TKey> valueRange, TValue value)
    {
        if ( _isInitialized )
        {
            throw new InvalidOperationException($"Cannot modify ValueRangeMap after initialization. Use {nameof(With)}() to create a new instance.");
        }

        foreach ( ValueRange<TKey> existingRange in _map.Keys )
        {
            if ( existingRange.Equals(valueRange) )
            {
                continue;
            }

            if ( valueRange.OverlapsWith(existingRange) )
            {
                throw new ArgumentException($"Range {valueRange} overlaps with existing range {existingRange}.", nameof(valueRange));
            }
        }

        _map[valueRange] = value;
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the range-value pairs in the map.
    /// </summary>
    /// <returns>An enumerator for the collection of range-value pairs.</returns>
    public IEnumerator<KeyValuePair<ValueRange<TKey>, TValue>> GetEnumerator()
    {
        EnsureInitialized();
        return _map.GetEnumerator();
    }

    /// <summary>
    ///     Gets the value associated with the range that contains the specified key, or returns a default value if no range
    ///     contains the key.
    /// </summary>
    /// <param name="key">The key to search for within the ranges.</param>
    /// <param name="defaultValue">The default value to return if no range contains the key.</param>
    /// <returns>The value associated with the matching range, or <paramref name="defaultValue" /> if no match is found.</returns>
    public TValue GetValueOrDefault(TKey key, TValue defaultValue)
    {
        return TryGetValue(key, out TValue? value) ? value! : defaultValue;
    }

    /// <summary>
    ///     Attempts to get the value associated with the range that contains the specified key.
    /// </summary>
    /// <param name="key">The key to search for within the ranges.</param>
    /// <param name="value">
    ///     When this method returns, contains the value associated with the matching range if found;
    ///     otherwise, the default value for <typeparamref name="TValue" />.
    /// </param>
    /// <returns><c>true</c> if a range containing the key was found; otherwise, <c>false</c>.</returns>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public bool TryGetValue(TKey key, out TValue? value)
    {
        foreach ( ValueRange<TKey> range in Ranges )
        {
            if ( !range.Contains(key) )
            {
                continue;
            }

            value = _map[range];
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    ///     Creates a new <see cref="ValueRangeMap{TRange,TValue}" /> with the specified range-value pair added.
    ///     This is an immutable operation.
    /// </summary>
    /// <param name="valueRange">The range to add.</param>
    /// <param name="value">The value to associate with the range.</param>
    /// <returns>A new range map containing all existing items plus the new range-value pair.</returns>
    /// <remarks>
    ///     The original map is not modified. If <paramref name="valueRange" /> already exists in the map,
    ///     the new value will replace the existing one in the returned map.
    /// </remarks>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="valueRange" /> overlaps with an existing range in the map.
    /// </exception>
    public ValueRangeMap<TKey, TValue> With(ValueRange<TKey> valueRange, TValue value)
    {
        foreach ( ValueRange<TKey> existingRange in _map.Keys )
        {
            if ( existingRange.Equals(valueRange) )
            {
                continue;
            }

            if ( valueRange.OverlapsWith(existingRange) )
            {
                throw new ArgumentException($"Range {valueRange} overlaps with existing range {existingRange}.", nameof(valueRange));
            }
        }

        IEnumerable<(ValueRange<TKey> Key, TValue Value)> items =
            _map.Append(new KeyValuePair<ValueRange<TKey>, TValue>(valueRange, value))
                .Select(kvp => ( kvp.Key, kvp.Value ));

        return new ValueRangeMap<TKey, TValue>(items);
    }

    private void EnsureInitialized()
    {
        if ( _isInitialized )
        {
            return;
        }

        _ranges = [.. _map.Keys,];
        _isInitialized = true;
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
