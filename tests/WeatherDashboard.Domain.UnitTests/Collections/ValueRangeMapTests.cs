namespace WeatherDashboard.Domain.UnitTests.Collections;

using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit3;
using AwesomeAssertions;
using Domain.Collections;
using Domain.ValueObjects;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "Collections")]
[Trait("Speed", "Fast")]
public sealed class ValueRangeMapTests
{
    [Theory]
    [AutoDataWithValidRanges]
    public void Add_AfterInitialization_ThrowsInvalidOperationException(ValueRange<int> range, string value, int key)
    {
        ValueRangeMap<int, string> map = [];

        // Trigger initialization
        _ = map.GetValueOrDefault(key, value);

        Action act = () => map.Add(range, value);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Cannot modify ValueRangeMap after initialization. Use With() to create a new instance.*");
    }

    [Theory]
    [AutoDataWithValidRanges]
    public void Add_BeforeInitialization_AddsRangeValuePair(ValueRange<int> range, string value)
    {
        ValueRangeMap<int, string> map = [];

        map.Add(range, value);

        map.Count.Should().Be(1);
    }

    [Theory]
    [AutoDataWithValidRanges]
    public void Add_WithExistingRange_UpdatesValue(ValueRange<int> range, string value1, string value2, string defaultValue)
    {
        ValueRangeMap<int, string> map = [];

        map.Add(range, value1);
        map.Add(range, value2);

        map.Count.Should().Be(1);
        int keyInRange = range.Min; // Use min value to ensure it's in range
        map.GetValueOrDefault(keyInRange, defaultValue).Should().Be(value2);
    }

    [Fact]
    public void CollectionInitializer_CreatesMapWithMultipleRanges()
    {
        ValueRangeMap<int, string> map = new()
        {
            { new ValueRange<int>(0, 10), "Low" },
            { new ValueRange<int>(11, 20), "Medium" },
            { new ValueRange<int>(21, 30), "High" },
        };

        map.Count.Should().Be(3);
        map.GetValueOrDefault(5, "default").Should().Be("Low");
        map.GetValueOrDefault(15, "default").Should().Be("Medium");
        map.GetValueOrDefault(25, "default").Should().Be("High");
    }

    [Fact]
    public void Constructor_WithDuplicateRanges_ThrowsArgumentException()
    {
        (ValueRange<int>, string)[] items =
        [
            ( new ValueRange<int>(0, 10), "Low" ),
            ( new ValueRange<int>(5, 15), "Medium" ), // Overlapping range
        ];

        Action act = () => _ = new ValueRangeMap<int, string>(items);

        act.Should().Throw<ArgumentException>()
           .WithMessage("Range [0, 10] overlaps with range [5, 15]. (Parameter 'items')");
    }

    [Fact]
    public void Constructor_WithEmptyCollection_CreateEmptyMap()
    {
        ValueRangeMap<int, string> map = [];

        map.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithItems_CreatesMapWithItems()
    {
        (ValueRange<int>, string)[] items =
        [
            ( new ValueRange<int>(0, 10), "Low" ),
            ( new ValueRange<int>(11, 20), "Medium" ),
            ( new ValueRange<int>(21, 30), "High" ),
        ];

        ValueRangeMap<int, string> map = new(items);

        map.Count.Should().Be(3);
    }

    [Fact]
    public void GetEnumerator_ReturnsAllRangeValuePairs()
    {
        ValueRangeMap<int, string> map = new()
        {
            { new ValueRange<int>(0, 10), "Low" },
            { new ValueRange<int>(11, 20), "Medium" },
            { new ValueRange<int>(21, 30), "High" },
        };

        List<KeyValuePair<ValueRange<int>, string>> items = [.. map,];

        items.Should().HaveCount(3);
        items.Should().Contain(new KeyValuePair<ValueRange<int>, string>(new ValueRange<int>(0, 10), "Low"));
        items.Should().Contain(new KeyValuePair<ValueRange<int>, string>(new ValueRange<int>(11, 20), "Medium"));
        items.Should().Contain(new KeyValuePair<ValueRange<int>, string>(new ValueRange<int>(21, 30), "High"));
    }

    [Fact]
    public void GetEnumerator_WithEmptyMap_ReturnsEmptyCollection()
    {
        ValueRangeMap<int, string> map = [];

        List<KeyValuePair<ValueRange<int>, string>> items = [.. map,];

        items.Should().BeEmpty();
    }

    [Theory]
    [AutoDataWithValidRanges]
    public void GetValueOrDefault_WithKeyNotInRange_ReturnsDefaultValue(ValueRange<int> range, string rangeValue, string defaultValue)
    {
        ValueRangeMap<int, string> map = [];
        map.Add(range, rangeValue);

        int keyOutOfRange = range.Max + 100; // Ensure it's outside the range
        string value = map.GetValueOrDefault(keyOutOfRange, defaultValue);

        value.Should().Be(defaultValue);
    }

    [Theory]
    [AutoDataWithValidRanges]
    public void GetValueOrDefault_WithKeyWithinRange_ReturnsValue(ValueRange<int> range, string rangeValue, string defaultValue)
    {
        ValueRangeMap<int, string> map = [];
        map.Add(range, rangeValue);

        int keyInRange = range.Min; // Use min value to ensure it's in range
        string value = map.GetValueOrDefault(keyInRange, defaultValue);

        value.Should().Be(rangeValue);
    }

    [Theory]
    [AutoDataWithValidRanges]
    public void TryGetValue_KeyWithinRange_ReturnsTrueAndValue(ValueRange<int> range, string rangeValue)
    {
        ValueRangeMap<int, string> map = [];
        map.Add(range, rangeValue);

        int keyInRange = range.Min; // Use min value to ensure it's in range
        bool result = map.TryGetValue(keyInRange, out string? value);

        result.Should().BeTrue();
        value.Should().Be(rangeValue);
    }

    [Fact]
    public void TryGetValue_WithKeyInRange_FindsCorrectRange()
    {
        ValueRangeMap<int, string> map = new()
        {
            { new ValueRange<int>(0, 10), "Low" },
            { new ValueRange<int>(11, 20), "Medium" },
            { new ValueRange<int>(21, 30), "High" },
        };

        bool result = map.TryGetValue(10, out string? value);

        result.Should().BeTrue();
        value.Should().NotBeNull().And.Be("Low");
    }

    [Theory]
    [AutoDataWithValidRanges]
    public void TryGetValue_WithKeyNotInRange_ReturnsFalseAndDefaultValue(ValueRange<int> range, string rangeValue)
    {
        ValueRangeMap<int, string> map = [];
        map.Add(range, rangeValue);

        int keyOutOfRange = range.Max + 100; // Ensure it's outside the range
        bool result = map.TryGetValue(keyOutOfRange, out string? value);

        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void With_CreatesNewInstance_WithAddedRangeValuePair(string value1, string value2, string defaultValue)
    {
        ValueRangeMap<int, string> originalMap = [];
        ValueRange<int> range1 = new(0, 10);
        originalMap.Add(range1, value1);

        ValueRange<int> range2 = new(11, 20);
        ValueRangeMap<int, string> newMap = originalMap.With(range2, value2);

        originalMap.Count.Should().Be(1);
        newMap.Count.Should().Be(2);
        newMap.GetValueOrDefault(15, defaultValue).Should().Be(value2);
    }

    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Test attribute")]
    [SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Test attribute")]
    [SuppressMessage("Performance", "CA1813:Avoid unsealed attributes", Justification = "Test attribute")]
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class AutoDataWithValidRangesAttribute : AutoDataAttribute
    {
        public AutoDataWithValidRangesAttribute() : base(() => new Fixture().Customize(new ValidValueRangeCustomization()))
        {
        }
    }

    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Test helper class")]
    [SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Test helper class")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class ValidValueRangeCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            ArgumentNullException.ThrowIfNull(fixture);
            fixture.Customizations.Add(new ValueRangeIntSpecimenBuilder());
        }

        [SuppressMessage("Security", "CA5394:Do not use insecure randomness",
            Justification = "Random is sufficient for test data generation")]
        private sealed class ValueRangeIntSpecimenBuilder : ISpecimenBuilder
        {
            private readonly Random _random = new();

            public object Create(object request, ISpecimenContext context)
            {
                if ( request is not Type type || type != typeof(ValueRange<int>) )
                {
                    return new NoSpecimen();
                }

                // Generate a valid range where min < max
                int min = _random.Next(0, 100);
                int max = _random.Next(min + 1, min + 101); // Ensure max > min

                return new ValueRange<int>(min, max);
            }
        }
    }
}
