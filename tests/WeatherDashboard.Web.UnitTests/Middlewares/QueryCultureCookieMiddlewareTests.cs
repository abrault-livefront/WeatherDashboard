namespace WeatherDashboard.Web.UnitTests.Middlewares;

using System.Globalization;
using AwesomeAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using Web.Middlewares;

[Trait("Category", "Unit")]
[Trait("Layer", "Web")]
[Trait("Feature", "Localization")]
[Trait("Component", "Middleware")]
[Trait("Speed", "Fast")]
public sealed class QueryCultureCookieMiddlewareTests
{
    private const string EnvironmentName = "Development";
    private const string ExpectedCookieName = "WeatherDashboard.Web.Culture.Development";

    [Fact]
    public void Constructor_WithNullEnvironment_ThrowsArgumentNullException()
    {
        IWebHostEnvironment? env = null;

        #pragma warning disable CA1806 // Constructor expected to throw
        Action act = () => _ = new QueryCultureCookieMiddleware(env!);
        #pragma warning restore CA1806

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("env");
    }

    [Fact]
    public void Constructor_WithValidEnvironment_CreatesMiddleware()
    {
        IWebHostEnvironment env = CreateMockEnvironment(EnvironmentName);

        QueryCultureCookieMiddleware middleware = new(env);

        middleware.Should().NotBeNull();
    }

    [Theory]
    [InlineData("pt-BR")]
    [InlineData("ja-JP")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    public async Task InvokeAsync_UsesCorrectCookieName(string cultureName)
    {
        IWebHostEnvironment env = CreateMockEnvironment(EnvironmentName);
        QueryCultureCookieMiddleware middleware = new(env);
        HttpContext context = CreateMockHttpContext(cultureName);
        RequestDelegate next = Substitute.For<RequestDelegate>();

        await middleware.InvokeAsync(context, next);

        context.Response.Cookies.Received(1).Append(
            ExpectedCookieName,
            Arg.Any<string>(),
            Arg.Any<CookieOptions>());
    }

    [Fact]
    public async Task InvokeAsync_WithCaseInsensitiveCultureMatch_DoesNotUpdateCookie()
    {
        const string cultureLowerCase = "en-us";
        const string cultureUpperCase = "EN-US";

        IWebHostEnvironment env = CreateMockEnvironment(EnvironmentName);
        QueryCultureCookieMiddleware middleware = new(env);
        HttpContext context = CreateMockHttpContext(
            cultureUpperCase,
            cultureLowerCase);
        RequestDelegate next = Substitute.For<RequestDelegate>();

        await middleware.InvokeAsync(context, next);

        context.Response.Cookies.DidNotReceive().Append(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CookieOptions>());
    }

    [Theory]
    [InlineData("it-IT")]
    [InlineData("es-ES")]
    [InlineData("ru-RU")]
    public async Task InvokeAsync_WithCultureInQueryString_AlwaysCallsNextMiddleware(string cultureName)
    {
        IWebHostEnvironment env = CreateMockEnvironment(EnvironmentName);
        QueryCultureCookieMiddleware middleware = new(env);
        HttpContext context = CreateMockHttpContext(cultureName);
        RequestDelegate next = Substitute.For<RequestDelegate>();

        await middleware.InvokeAsync(context, next);

        await next.Received(1).Invoke(context);
    }

    [Theory]
    [InlineData("ja-JP")]
    [InlineData("ko-KR")]
    [InlineData("ar-SA")]
    public async Task InvokeAsync_WithCultureInQueryString_SetsCookieWithOneYearExpiration(string cultureName)
    {
        DateTimeOffset beforeTest = DateTimeOffset.UtcNow;
        IWebHostEnvironment env = CreateMockEnvironment(EnvironmentName);
        QueryCultureCookieMiddleware middleware = new(env);
        HttpContext context = CreateMockHttpContext(cultureName);
        RequestDelegate next = Substitute.For<RequestDelegate>();

        await middleware.InvokeAsync(context, next);

        DateTimeOffset afterTest = DateTimeOffset.UtcNow;

        context.Response.Cookies.Received(1).Append(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<CookieOptions>(opts =>
                opts.Expires.HasValue &&
                opts.Expires.Value >= beforeTest.AddYears(1) &&
                opts.Expires.Value <= afterTest.AddYears(1)));
    }

    [Theory]
    [InlineData("fr-FR")]
    [InlineData("nl-NL")]
    [InlineData("sv-SE")]
    public async Task InvokeAsync_WithCultureInQueryStringAndNoCookie_SetsCookie(string cultureName)
    {
        IWebHostEnvironment env = CreateMockEnvironment(EnvironmentName);
        QueryCultureCookieMiddleware middleware = new(env);
        HttpContext context = CreateMockHttpContext(cultureName);
        RequestDelegate next = Substitute.For<RequestDelegate>();

        await middleware.InvokeAsync(context, next);

        string expectedValue = CookieRequestCultureProvider.MakeCookieValue(
            new RequestCulture(new CultureInfo(cultureName)));

        context.Response.Cookies.Received(1).Append(
            ExpectedCookieName,
            expectedValue,
            Arg.Is<CookieOptions>(opts =>
                opts.IsEssential &&
                !opts.HttpOnly &&
                opts.SameSite == SameSiteMode.Strict));
    }

    [Theory]
    [InlineData("zh-CN")]
    [InlineData("pl-PL")]
    [InlineData("tr-TR")]
    public async Task InvokeAsync_WithDifferentEnvironment_UsesCorrectCookieName(string cultureName)
    {
        const string environmentName = "Production";
        const string expectedCookieName = "WeatherDashboard.Web.Culture.Production";

        IWebHostEnvironment env = CreateMockEnvironment(environmentName);
        QueryCultureCookieMiddleware middleware = new(env);
        HttpContext context = CreateMockHttpContext(cultureName);
        RequestDelegate next = Substitute.For<RequestDelegate>();

        await middleware.InvokeAsync(context, next);

        context.Response.Cookies.Received(1).Append(
            expectedCookieName,
            Arg.Any<string>(),
            Arg.Any<CookieOptions>());
    }

    [Theory]
    [InlineData("en-US", "fr-FR")]
    [InlineData("pt-BR", "es-ES")]
    [InlineData("de-DE", "it-IT")]
    public async Task InvokeAsync_WithDifferentExistingCookie_UpdatesCookie(string oldCultureName, string newCultureName)
    {
        IWebHostEnvironment env = CreateMockEnvironment(EnvironmentName);
        QueryCultureCookieMiddleware middleware = new(env);
        HttpContext context = CreateMockHttpContext(
            newCultureName,
            oldCultureName);
        RequestDelegate next = Substitute.For<RequestDelegate>();

        await middleware.InvokeAsync(context, next);

        string expectedValue = CookieRequestCultureProvider.MakeCookieValue(
            new RequestCulture(new CultureInfo(newCultureName)));

        context.Response.Cookies.Received(1).Append(
            ExpectedCookieName,
            expectedValue,
            Arg.Any<CookieOptions>());
    }

    [Fact]
    public async Task InvokeAsync_WithNoCultureInQueryString_DoesNotSetCookie()
    {
        IWebHostEnvironment env = CreateMockEnvironment(EnvironmentName);
        QueryCultureCookieMiddleware middleware = new(env);
        HttpContext context = CreateMockHttpContext();
        RequestDelegate next = Substitute.For<RequestDelegate>();
        bool nextCalled = false;
        next.Invoke(Arg.Any<HttpContext>()).Returns(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, next);

        context.Response.Cookies.DidNotReceive().Append(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CookieOptions>());
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithNullContext_ThrowsArgumentNullException()
    {
        IWebHostEnvironment env = CreateMockEnvironment(EnvironmentName);
        QueryCultureCookieMiddleware middleware = new(env);
        RequestDelegate next = Substitute.For<RequestDelegate>();

        Func<Task> act = async () => await middleware.InvokeAsync(null!, next).ConfigureAwait(true);

        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("context")
                 .ConfigureAwait(true);
    }

    [Fact]
    public async Task InvokeAsync_WithNullNext_ThrowsArgumentNullException()
    {
        IWebHostEnvironment env = CreateMockEnvironment(EnvironmentName);
        QueryCultureCookieMiddleware middleware = new(env);
        HttpContext context = CreateMockHttpContext();

        Func<Task> act = async () => await middleware.InvokeAsync(context, null!).ConfigureAwait(true);

        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("next")
                 .ConfigureAwait(true);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("zh-CN")]
    [InlineData("hi-IN")]
    public async Task InvokeAsync_WithSameExistingCookieValue_DoesNotUpdateCookie(string cultureName)
    {
        IWebHostEnvironment env = CreateMockEnvironment(EnvironmentName);
        QueryCultureCookieMiddleware middleware = new(env);
        HttpContext context = CreateMockHttpContext(
            cultureName,
            cultureName);
        RequestDelegate next = Substitute.For<RequestDelegate>();

        await middleware.InvokeAsync(context, next);

        context.Response.Cookies.DidNotReceive().Append(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CookieOptions>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task InvokeAsync_WithWhitespaceCulture_DoesNotSetCookie(string whitespaceCulture)
    {
        IWebHostEnvironment env = CreateMockEnvironment(EnvironmentName);
        QueryCultureCookieMiddleware middleware = new(env);
        HttpContext context = CreateMockHttpContext(whitespaceCulture);
        RequestDelegate next = Substitute.For<RequestDelegate>();

        await middleware.InvokeAsync(context, next);

        context.Response.Cookies.DidNotReceive().Append(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CookieOptions>());
    }

    private static IWebHostEnvironment CreateMockEnvironment(string environmentName)
    {
        IWebHostEnvironment env = Substitute.For<IWebHostEnvironment>();
        env.EnvironmentName.Returns(environmentName);
        return env;
    }

    private static HttpContext CreateMockHttpContext(
        string? queryStringCulture = null,
        string? existingCookieValue = null,
        bool isHttps = false)
    {
        HttpContext context = Substitute.For<HttpContext>();

        // Setup Request
        HttpRequest request = Substitute.For<HttpRequest>();
        QueryString queryString = queryStringCulture is not null
                                      ? new QueryString($"?culture={queryStringCulture}")
                                      : new QueryString();
        request.QueryString.Returns(queryString);
        request.Query.Returns(new QueryCollection(
            queryStringCulture is not null
                ? new Dictionary<string, StringValues>(StringComparer.Ordinal)
                {
                    ["culture"] = queryStringCulture,
                }
                : new Dictionary<string, StringValues>(StringComparer.Ordinal)));
        request.IsHttps.Returns(isHttps);

        // Setup Request Cookies
        IRequestCookieCollection requestCookies = Substitute.For<IRequestCookieCollection>();
        if ( existingCookieValue is not null )
        {
            requestCookies[ExpectedCookieName].Returns(existingCookieValue);
        }
        else
        {
            requestCookies[Arg.Any<string>()].Returns((string?)null);
        }

        request.Cookies.Returns(requestCookies);
        context.Request.Returns(request);

        // Setup Response
        HttpResponse response = Substitute.For<HttpResponse>();
        IResponseCookies responseCookies = Substitute.For<IResponseCookies>();
        response.Cookies.Returns(responseCookies);
        context.Response.Returns(response);

        return context;
    }
}
