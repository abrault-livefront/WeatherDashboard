namespace WeatherDashboard.Web;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Application.Common.Serialization.MessagePack;
using Configuration;
using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using Infrastructure.Configuration;
using Infrastructure.Extensions;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Middlewares;
using NeoSmart.Caching.Sqlite;
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
    private const string CertificateFileName = "WeatherDashboard.Web.DataProtection.pfx";

    private const string DataProtectionRedisKey = "DataProtection-Keys";

    private const string RedisClientName = "WeatherDashboard.Web";

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

            builder.Services.Configure<RedisOptions>(RedisOptions.SectionNameCache,
                builder.Configuration.GetSection(RedisOptions.SectionNameCache)
            );

            builder.Services.Configure<RedisOptions>(RedisOptions.SectionNameConfig,
                builder.Configuration.GetSection(RedisOptions.SectionNameConfig)
            );

            ConfigureLogging(builder);
            ConfigureServices(builder);

            WebApplication app = builder.Build();

            ConfigureMiddleware(app);

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
    ///     Configures distributed caching using SQLite for development and Redis for production.
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

        if ( builder.Environment.IsDevelopment() )
        {
            DirectoryInfo cacheDirectory = GetCacheDirectory();
            EnsureCacheDirectory(cacheDirectory);

            #pragma warning disable CA2000 // Dispose handled by FusionCache
            builder.Services.AddFusionCache()
                   .WithSerializer(serializer)
                   .WithDistributedCache(new SqliteCache(new SqliteCacheOptions
                    {
                        CachePath = Path.Combine(cacheDirectory.FullName, "cache.db"),
                    }));
            #pragma warning restore CA2000
        }
        else
        {
            RedisOptions options = builder.Configuration.GetSection(RedisOptions.SectionNameCache).Get<RedisOptions>()
                                ?? throw new InvalidOperationException("Redis cache configuration is missing");

            ConfigurationOptions redisConfigurationOptions = CreateRedisConfigurationOptions(options);

            #pragma warning disable CA2000 // Dispose handled by FusionCache
            builder.Services.AddFusionCache()
                   .WithSerializer(serializer)
                   .WithDistributedCache(new RedisCache(new RedisCacheOptions
                    {
                        ConfigurationOptions = redisConfigurationOptions,
                    }))
                   .WithBackplane(new RedisBackplane(new RedisBackplaneOptions
                    {
                        ConfigurationOptions = redisConfigurationOptions,
                    }));
            #pragma warning restore CA2000
        }
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
    ///     In production, data protection keys are persisted to Redis.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    private static void ConfigureSecurity(WebApplicationBuilder builder)
    {
        CookieSettings cookieSettings = GetCookieSettings(builder.Configuration);

        // note: prefer ProtectKeysWithAzureKeyVault() over ProtectKeysWithCertificate()
        // unless you can guarantee the same certificate is deployed to all instances securely.
        string certificatePath = Path.Combine(AppContext.BaseDirectory, CertificateFileName);
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

        if ( !builder.Environment.IsDevelopment() )
        {
            RedisOptions options = builder.Configuration.GetSection(RedisOptions.SectionNameConfig).Get<RedisOptions>()
                                ?? throw new InvalidOperationException("Redis config configuration is missing");

            ConfigurationOptions redisConfig = CreateRedisConfigurationOptions(options);

            builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConfig));

            IConnectionMultiplexer connection = builder.Services
                                                       .BuildServiceProvider()
                                                       .GetRequiredService<IConnectionMultiplexer>();

            dataProtectionBuilder.PersistKeysToStackExchangeRedis(connection, DataProtectionRedisKey);
        }

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
    ///     Configures all application services including localization, security, caching, state management,
    ///     and infrastructure services.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        ConfigureLocalization(builder);
        ConfigureSecurity(builder);
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
    ///     Creates a Redis <see cref="ConfigurationOptions" /> instance from the specified configuration section.
    /// </summary>
    /// <param name="options">The Redis options containing endpoint and timeout settings.</param>
    /// <returns>A configured <see cref="ConfigurationOptions" /> instance.</returns>
    private static ConfigurationOptions CreateRedisConfigurationOptions(RedisOptions options)
    {
        EndPoint[] endPoints = [.. options.EndPoints.Select(ep => new DnsEndPoint(ep.Host, ep.Port)),];

        return new ConfigurationOptions
        {
            AllowAdmin = false,
            ClientName = RedisClientName,
            ConnectTimeout = options.ConnectTimeoutMilliseconds,
            EndPoints = new EndPointCollection(endPoints),
            ResolveDns = options.ResolveDns,
            SyncTimeout = options.SyncTimeoutMilliseconds,
        };
    }

    /// <summary>
    ///     Ensures the cache directory exists, creating it if necessary.
    /// </summary>
    private static void EnsureCacheDirectory(DirectoryInfo cacheDirectory)
    {
        if ( !cacheDirectory.Exists )
        {
            cacheDirectory.Create();
        }
    }

    /// <summary>
    ///     Gets the cache directory path for development environments where SQLite caching is used.
    /// </summary>
    /// <returns>A <see cref="DirectoryInfo" /> representing the cache directory.</returns>
    private static DirectoryInfo GetCacheDirectory()
    {
        string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return new DirectoryInfo(Path.Combine(basePath, "WeatherDashboard", "Cache"));
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
