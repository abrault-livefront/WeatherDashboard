namespace WeatherDashboard.Web;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using Application.Common.Serialization.MessagePack;
using Configuration;
using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using Infrastructure.Extensions;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Middlewares;
using Radzen;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.NeueccMessagePack;

[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
[ExcludeFromCodeCoverage]
internal static class Program
{
    private const string s_certificateFileName = "WeatherDashboard.Web.DataProtection.pfx";

    private const string s_dataProtectionRedisKey = "DataProtection-Keys";

    public static async Task Main()
    {
        CreateBootstrapLogger();

        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();

            builder.Services.Configure<CookieSettings>(CookieSettings.SectionName,
                builder.Configuration.GetSection(CookieSettings.SectionName)
            );

            builder.Services.Configure<DefaultLocationSettings>(DefaultLocationSettings.SectionName,
                builder.Configuration.GetSection(DefaultLocationSettings.SectionName)
            );

            builder.Services.Configure<LocalStorageSettings>(LocalStorageSettings.SectionName,
                builder.Configuration.GetSection(LocalStorageSettings.SectionName)
            );

            IServiceProvider? services = null;

            ConfigureLogging(builder);
            ConfigureServices(builder, () => services!);

            WebApplication app = builder.Build();
            services = app.Services;

            ConfigureMiddleware(app);
            ConfigureSecurityHeaders(app);

            await app.RunAsync().ConfigureAwait(false);
        }
        catch ( Exception e )
        {
            Log.Fatal(e, "Host terminated unexpectedly.");
            Environment.Exit(1);
        }
        finally
        {
            await Log.CloseAndFlushAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Configures distributed caching using the Aspire-provisioned "redis-cache" Redis instance.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    private static void ConfigureCaching(WebApplicationBuilder builder)
    {
        IFormatterResolver resolver = CompositeResolver.Create(
            [
                new TimeZoneInfoFormatter(),
            ],
            [
                StandardResolver.Instance,
            ]
        );

        FusionCacheNeueccMessagePackSerializer serializer =
            new(new MessagePackSerializerOptions(resolver));

        #pragma warning disable CA2000 // Dispose handled by FusionCache
        builder.Services.AddFusionCache()
               .WithSerializer(serializer)
               .WithDistributedCache(sp => new RedisCache(new RedisCacheOptions
                {
                    ConnectionMultiplexerFactory = () => Task.FromResult(sp.GetRequiredKeyedService<IConnectionMultiplexer>("redis-cache")),
                }))
               .WithBackplane(sp => new RedisBackplane(new RedisBackplaneOptions
                {
                    ConnectionMultiplexerFactory = () => Task.FromResult(sp.GetRequiredKeyedService<IConnectionMultiplexer>("redis-cache")),
                }));
        #pragma warning restore CA2000
    }

    /// <summary>
    ///     Configures Fluxor state management with Redux DevTools in debug builds.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    private static void ConfigureFluxor(WebApplicationBuilder builder)
    {
        builder.Services.AddFluxor(o =>
        {
            o.ScanAssemblies(typeof(Program).Assembly);
#if DEBUG
            o.UseReduxDevTools();
#endif
        });
    }

    /// <summary>
    ///     Configures request localization with cookie-based culture persistence.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    private static void ConfigureLocalization(WebApplicationBuilder builder)
    {
        CultureInfo defaultCulture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo[] supportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

        CookieSettings cookieSettings = GetCookieSettings(builder.Configuration);

        builder.Services.Configure<RequestLocalizationOptions>(o =>
        {
            o.DefaultRequestCulture = new RequestCulture(defaultCulture);
            o.FallBackToParentUICultures = true;
            o.SupportedCultures = supportedCultures;
            o.SupportedUICultures = supportedCultures;

            o.RequestCultureProviders = (List<IRequestCultureProvider>)
            [
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider
                {
                    CookieName = cookieSettings.GetCultureCookieName(builder.Environment.EnvironmentName),
                },
                new AcceptLanguageHeaderRequestCultureProvider(),
            ];
        });

        builder.Services.AddLocalization(o => o.ResourcesPath = "Localizations");
    }

    /// <summary>
    ///     Configures structured logging using Serilog with enrichers and OpenTelemetry sink.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, provider, config) =>
            config.ReadFrom.Configuration(context.Configuration)
                  .ReadFrom.Services(provider)
                  .Enrich.FromLogContext()
                  .Enrich.WithCorrelationId()
                  .Enrich.WithMachineName()
                  .Enrich.WithEnvironmentUserName()
                  .Enrich.WithEnvironmentName()
                  .Enrich.WithThreadId()
                  .Enrich.WithProcessId()
                  .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture, theme: AnsiConsoleTheme.Code)
                  .WriteTo.OpenTelemetry()
        );
    }

    /// <summary>
    ///     Configures the HTTP request pipeline middleware.
    /// </summary>
    /// <param name="app">The web application.</param>
    private static void ConfigureMiddleware(WebApplication app)
    {
        app.UseMiddleware<QueryCultureCookieMiddleware>();

        app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

        if ( !app.Environment.IsDevelopment() )
        {
            app.UseExceptionHandler("/Error");
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAntiforgery();
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
           .AddInteractiveServerRenderMode();
    }

    /// <summary>
    ///     Configures data protection with certificate-based key encryption and antiforgery tokens.
    ///     Data protection keys are persisted to the Aspire-provisioned "redis-config" Redis instance.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="resolveServices">
    ///     Resolves the real application <see cref="IServiceProvider" />, available once the host
    ///     has been built.
    /// </param>
    private static void ConfigureSecurity(WebApplicationBuilder builder, Func<IServiceProvider> resolveServices)
    {
        CookieSettings cookieSettings = GetCookieSettings(builder.Configuration);

        // note: prefer ProtectKeysWithAzureKeyVault() over ProtectKeysWithCertificate()
        // unless you can guarantee the same certificate is deployed to all instances securely.
        string certificatePath = Path.Combine(AppContext.BaseDirectory, s_certificateFileName);
        ReadOnlySpan<byte> pfx = File.ReadAllBytes(certificatePath);

        X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12(pfx,
            [],
            X509KeyStorageFlags.EphemeralKeySet
        );

        IDataProtectionBuilder dataProtectionBuilder =
            builder.Services.AddDataProtection()
                   .ProtectKeysWithCertificate(certificate)
                   .SetApplicationName($"WeatherDashboard.Web.{builder.Environment.EnvironmentName}")
                   .SetDefaultKeyLifetime(TimeSpan.FromDays(365));

        // note: the database factory is deferred (via resolveServices) so the keyed Aspire Redis client is
        // resolved from the real DI container after the host has started, instead of eagerly via a throwaway
        // BuildServiceProvider() snapshot, which raced with Serilog's reloadable logger and crashed on startup.
        dataProtectionBuilder.PersistKeysToStackExchangeRedis(
            () => resolveServices().GetRequiredKeyedService<IConnectionMultiplexer>("redis-config").GetDatabase(),
            s_dataProtectionRedisKey);

        builder.Services.AddAntiforgery(o =>
        {
            o.Cookie.HttpOnly = true;
            o.Cookie.Name = cookieSettings.GetAntiForgeryCookieName(builder.Environment.EnvironmentName);
            o.Cookie.Path = "/";
            o.Cookie.SameSite = SameSiteMode.Strict;
            o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });
    }

    /// <summary>
    ///     Configures security headers including Content Security Policy (CSP) and frame options.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <remarks>
    ///     <para>
    ///         Configures the following security headers:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Content Security Policy (CSP) that restricts resource loading to prevent XSS attacks.
    ///                 Default sources are limited to self, with specific allowances for images (self and data URIs),
    ///                 scripts (with unsafe-eval and unsafe-inline for Blazor), and styles (with unsafe-inline).
    ///                 Connect sources are limited to self. Object sources are blocked entirely.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 X-Frame-Options set to SAMEORIGIN to prevent clickjacking attacks by disallowing
    ///                 the application from being embedded in frames on other domains.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    private static void ConfigureSecurityHeaders(WebApplication app)
    {
        app.UseSecurityHeaders(p =>
        {
            p.AddContentSecurityPolicy(o =>
            {
                o.AddDefaultSrc().Self();
                o.AddConnectSrc().Self();
                o.AddImgSrc().Data();
                o.AddObjectSrc().None();
                o.AddScriptSrc()
                 .Self()
                 .UnsafeEval()
                 .UnsafeInline();
                o.AddStyleSrc().Self().UnsafeInline();
            });

            p.AddFrameOptionsSameOrigin();
        });
    }

    /// <summary>
    ///     Configures all application services including localization, security, caching, state management,
    ///     and infrastructure services.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="resolveServices">
    ///     Resolves the real application <see cref="IServiceProvider" />, available once the host
    ///     has been built.
    /// </param>
    private static void ConfigureServices(WebApplicationBuilder builder, Func<IServiceProvider> resolveServices)
    {
        builder.AddKeyedRedisClient("redis-cache");
        builder.AddKeyedRedisClient("redis-config");
        builder.AddElasticsearchClient("elasticsearch");

        ConfigureLocalization(builder);
        ConfigureSecurity(builder, resolveServices);
        ConfigureCaching(builder);
        ConfigureFluxor(builder);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddRadzenComponents();
        builder.Services.AddRazorComponents()
               .AddInteractiveServerComponents();

        builder.Services.AddScoped<QueryCultureCookieMiddleware>();
        builder.Services.AddInfrastructureServices();
    }

    /// <summary>
    ///     Creates the bootstrap logger used during application startup before the full logging configuration is loaded.
    /// </summary>
    private static void CreateBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture, theme: AnsiConsoleTheme.Code)
                    .CreateBootstrapLogger();
    }

    /// <summary>
    ///     Retrieves and validates the cookie settings from configuration.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The cookie settings instance.</returns>
    private static CookieSettings GetCookieSettings(ConfigurationManager configuration)
    {
        return configuration.GetSection(CookieSettings.SectionName).Get<CookieSettings>()
            ?? new CookieSettings();
    }
}
