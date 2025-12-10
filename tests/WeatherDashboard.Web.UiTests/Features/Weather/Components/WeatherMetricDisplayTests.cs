namespace WeatherDashboard.Web.UiTests.Features.Weather.Components;

using AngleSharp.Dom;
using AwesomeAssertions;
using Bunit;
using Radzen;
using Radzen.Blazor;
using Web.Features.Weather.Components;

[Trait("Category", "UI")]
[Trait("Layer", "Web")]
[Trait("Feature", "Weather")]
[Trait("Component", "BlazorComponent")]
[Trait("Speed", "Slow")]
public sealed class WeatherMetricDisplayTests : BunitContext
{
    [Fact]
    public void Both_Texts_Use_Body1_Style()
    {
        IRenderedComponent<WeatherMetricDisplay> cut =
            Render<WeatherMetricDisplay>(parameters => parameters
                                                      .Add(p => p.Label, "Test")
                                                      .AddChildContent("Value"));

        IReadOnlyList<IRenderedComponent<RadzenText>> texts = cut.FindComponents<RadzenText>();
        texts.Should().AllSatisfy(text =>
            text.Instance.TextStyle.Should().Be(TextStyle.Body1));
    }

    [Fact]
    public void Inner_Stack_Is_Horizontal()
    {
        IRenderedComponent<WeatherMetricDisplay> cut =
            Render<WeatherMetricDisplay>(parameters => parameters
                                                      .Add(p => p.Label, "Test")
                                                      .AddChildContent("Value"));

        IRenderedComponent<RadzenStack> innerStack = cut.FindComponents<RadzenStack>()[1];
        innerStack.Instance.Orientation.Should().Be(Orientation.Horizontal);
    }

    [Fact]
    public void Outer_Stack_Is_Vertical_With_Gap()
    {
        IRenderedComponent<WeatherMetricDisplay> cut =
            Render<WeatherMetricDisplay>(parameters => parameters
                                                      .Add(p => p.Label, "Test")
                                                      .AddChildContent("Value"));

        IRenderedComponent<RadzenStack> outerStack = cut.FindComponents<RadzenStack>()[0];
        outerStack.Instance.Orientation.Should().Be(Orientation.Vertical);
        outerStack.Instance.Gap.Should().Be("0.3rem");
    }

    [Fact]
    public void Renders_Complex_ChildContent()
    {
        IRenderedComponent<WeatherMetricDisplay> cut =
            Render<WeatherMetricDisplay>(parameters => parameters
                                                      .Add(p => p.Label, "Conditions")
                                                      .AddChildContent("<span class='icon'>☀️</span><span>Sunny</span>"));

        IEnumerable<IElement> spans = cut.FindAll("span");
        spans.Should().HaveCountGreaterThanOrEqualTo(2);
        spans.Should().Contain(s => s.TextContent == "☀️");
        spans.Should().Contain(s => s.TextContent == "Sunny");
    }

    [Fact]
    public void Renders_HTML_ChildContent()
    {
        IRenderedComponent<WeatherMetricDisplay> cut =
            Render<WeatherMetricDisplay>(parameters => parameters
                                                      .Add(p => p.Label, "Wind Speed")
                                                      .AddChildContent("<strong>15 km/h</strong>"));

        cut.Find("strong").TextContent.Should().Be("15 km/h");
    }

    [Fact]
    public void Renders_Label_With_Colon()
    {
        IRenderedComponent<WeatherMetricDisplay> cut =
            Render<WeatherMetricDisplay>(parameters => parameters
                                                      .Add(p => p.Label, "Temperature")
                                                      .AddChildContent("25°C"));

        IEnumerable<IElement> textElements = cut.FindAll("p");
        textElements.Should().Contain(p => p.TextContent == "Temperature:");
    }

    [Fact]
    public void Renders_Plain_Text_ChildContent()
    {
        IRenderedComponent<WeatherMetricDisplay> cut =
            Render<WeatherMetricDisplay>(parameters => parameters
                                                      .Add(p => p.Label, "Humidity")
                                                      .AddChildContent("65%"));

        IEnumerable<IElement> textElements = cut.FindAll("p");
        textElements.Should().Contain(p => p.TextContent == "65%");
    }

    [Theory]
    [InlineData("Temperature", "25°C")]
    [InlineData("Humidity", "60%")]
    [InlineData("Wind Speed", "10 km/h")]
    [InlineData("Pressure", "1013 hPa")]
    public void Renders_Various_Label_And_Content_Combinations(string label, string content)
    {
        IRenderedComponent<WeatherMetricDisplay> cut =
            Render<WeatherMetricDisplay>(parameters => parameters
                                                      .Add(p => p.Label, label)
                                                      .AddChildContent(content));

        IEnumerable<IElement> textElements = cut.FindAll("p");
        textElements.Should().Contain(p => p.TextContent == $"{label}:");
        textElements.Should().Contain(p => p.TextContent == content);
    }

    [Fact]
    public void Uses_Correct_Component_Structure()
    {
        IRenderedComponent<WeatherMetricDisplay> cut =
            Render<WeatherMetricDisplay>(parameters => parameters
                                                      .Add(p => p.Label, "Test")
                                                      .AddChildContent("Value"));

        IReadOnlyList<IRenderedComponent<RadzenStack>> stacks = cut.FindComponents<RadzenStack>();
        stacks.Should().HaveCount(2, "component should have outer and inner stacks");

        IReadOnlyList<IRenderedComponent<RadzenText>> texts = cut.FindComponents<RadzenText>();
        texts.Should().HaveCount(2, "component should have label and content texts");
    }
}
