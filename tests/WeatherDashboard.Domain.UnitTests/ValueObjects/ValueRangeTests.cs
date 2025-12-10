namespace WeatherDashboard.Domain.UnitTests.ValueObjects;

using AwesomeAssertions;
using Domain.ValueObjects;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "ValueObjects")]
[Trait("Speed", "Fast")]
public sealed class ValueRangeTests
{
    [Theory]
    [InlineData(20, 10)]
    [InlineData(100, 0)]
    [InlineData(1, -1)]
    [InlineData(50, 49)]
    public void Constructor_WithMaxLessThanMin_ShouldThrowArgumentException(int min, int max)
    {
        Action act = () => _ = new ValueRange<int>(min, max);

        act.Should().Throw<ArgumentException>()
           .WithParameterName(nameof(max));
    }

    [Theory]
    [InlineData(5.5, 10.5, 4.0, false)] // Below min
    [InlineData(5.5, 10.5, 11.0, false)] // Above max
    [InlineData(5.5, 10.5, 5.5, true)] // Equal to min
    [InlineData(5.5, 10.5, 10.5, true)] // Equal to max
    [InlineData(5.5, 10.5, 7.5, true)] // Within range
    [InlineData(0.0, 1.0, 0.5, true)] // Different range
    [InlineData(-10.5, -5.5, -7.5, true)] // Negative range
    public void Contains_WithDoubleValues_ShouldReturnExpectedResult(
        double min, double max, double value, bool expected)
    {
        ValueRange<double> range = new(min, max);

        bool result = range.Contains(value);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(10, 20, 25, false)] // Above max
    [InlineData(10, 20, 5, false)] // Below min
    [InlineData(10, 20, 20, true)] // Equal to max
    [InlineData(10, 20, 10, true)] // Equal to min
    [InlineData(10, 20, 15, true)] // Within range
    [InlineData(0, 100, 50, true)] // Different range - within
    [InlineData(0, 100, -1, false)] // Different range - below
    [InlineData(0, 100, 101, false)] // Different range - above
    public void Contains_WithIntegerValues_ShouldReturnExpectedResult(int min, int max, int value, bool expected)
    {
        ValueRange<int> range = new(min, max);

        bool result = range.Contains(value);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("apple", "banana", "aardvark", false)] // Below min
    [InlineData("apple", "banana", "cherry", false)] // Above max
    [InlineData("apple", "banana", "apple", true)] // Equal to min
    [InlineData("apple", "banana", "banana", true)] // Equal to max
    [InlineData("apple", "banana", "avocado", true)] // Within range
    [InlineData("cat", "dog", "cow", true)] // Different range
    [InlineData("alpha", "omega", "beta", true)] // Greek letters
    public void Contains_WithStringValues_ShouldReturnExpectedResult(
        string min, string max, string value, bool expected)
    {
        ValueRange<string> range = new(min, max);

        bool result = range.Contains(value);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(10, 20, 10, 20, true)] // Identical ranges
    [InlineData(10, 20, 30, 40, false)] // Non-overlapping ranges
    [InlineData(10, 20, 15, 25, true)] // Overlapping ranges
    [InlineData(10, 20, 20, 30, true)] // Ranges touching at boundary
    [InlineData(0, 10, 5, 15, true)] // Partial overlap
    [InlineData(0, 10, 11, 20, false)] // Adjacent but not touching
    [InlineData(5, 15, 0, 20, true)] // Range2 contains Range1
    public void OverlapsWith_WithVariousRanges_ShouldReturnExpectedResult(
        int min1, int max1, int min2, int max2, bool expected)
    {
        ValueRange<int> range1 = new(min1, max1);
        ValueRange<int> range2 = new(min2, max2);

        range1.OverlapsWith(range2).Should().Be(expected);
        range2.OverlapsWith(range1).Should().Be(expected); // Symmetry
    }
}
