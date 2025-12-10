namespace WeatherDashboard.Web.Middlewares;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

/// <summary>
///     Middleware that persists culture selection from query string parameters to a cookie
///     for maintaining localization preferences across requests.
/// </summary>
/// <remarks>
///     This middleware detects culture changes via query string (e.g., ?culture=es)
///     and stores the preference in an environment-specific cookie that expires after one year.
///     The cookie name includes the environment name for isolation between development and production.
/// </remarks>
[SuppressMessage("Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Middleware instantiated by DI")]
internal sealed class QueryCultureCookieMiddleware : IMiddleware
{
    /// <summary>
    ///     Base cookie options used for the culture cookie.
    ///     The cookie is essential, not HTTP-only (accessible to JavaScript), and uses strict same-site mode.
    /// </summary>
    private static readonly CookieOptions BaseCookieOptions = new()
    {
        IsEssential = true,
        HttpOnly = false,
        SameSite = SameSiteMode.Strict,
    };

    /// <summary>
    ///     The prefix used for the culture cookie name before the environment name is appended.
    /// </summary>
    private const string CookieNamePrefix = "WeatherDashboard.Web.Culture.";

    /// <summary>
    ///     The provider used to detect culture selection from query string parameters.
    /// </summary>
    private static readonly QueryStringRequestCultureProvider Provider = new();

    /// <summary>
    ///     The fully qualified cookie name including the environment name.
    /// </summary>
    private readonly string _cookieName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="QueryCultureCookieMiddleware" /> class.
    /// </summary>
    /// <param name="env">The web host environment used to generate an environment-specific cookie name.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="env" /> is <see langword="null" />.</exception>
    public QueryCultureCookieMiddleware(IWebHostEnvironment env)
    {
        ArgumentNullException.ThrowIfNull(env);
        _cookieName = $"{CookieNamePrefix}{env.EnvironmentName}";
    }

    /// <summary>
    ///     Processes the HTTP request, detecting culture changes from the query string
    ///     and persisting the selection to a cookie if changed.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="context" /> or <paramref name="next" /> is <see langword="null" />.
    /// </exception>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        ProviderCultureResult? result = await Provider.DetermineProviderCultureResult(context)
                                                      .ConfigureAwait(false);

        if ( result is not null && result.Cultures.Count > 0 )
        {
            string cultureName = result.Cultures[0].ToString();
            if ( !string.IsNullOrWhiteSpace(cultureName) )
            {
                string existing = context.Request.Cookies[_cookieName] ?? string.Empty;
                if ( !existing.Equals(cultureName, StringComparison.OrdinalIgnoreCase) )
                {
                    CultureInfo culture = new(cultureName);
                    string value = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));

                    CookieOptions opts = new()
                    {
                        IsEssential = BaseCookieOptions.IsEssential,
                        HttpOnly = BaseCookieOptions.HttpOnly,
                        SameSite = BaseCookieOptions.SameSite,
                        Secure = context.Request.IsHttps,
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                    };

                    context.Response.Cookies.Append(_cookieName, value, opts);
                }
            }
        }

        await next(context).ConfigureAwait(false);
    }
}
