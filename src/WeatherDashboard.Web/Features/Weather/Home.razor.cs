namespace WeatherDashboard.Web.Features.Weather;

using Configuration;
using Domain.Entities.Documents;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using StateManagement;
using StateManagement.Actions;

/// <summary>
///     Code-behind for the Home page component that serves as the main weather dashboard.
/// </summary>
/// <remarks>
///     This component loads the last selected location from protected browser storage on first render
///     and displays the weather forecast and location search interface. If no location is stored,
///     it defaults to a configured location from application settings.
/// </remarks>
[StreamRendering]
[UsedImplicitly]
public sealed partial class Home : FluxorComponent
{
    private readonly IOptions<DefaultLocationSettings> _defaultLocationSettings;

    private readonly IDispatcher _dispatcher;

    private readonly IStringLocalizer<Home> _localizer;

    private readonly ProtectedLocalStorage _localStorage;

    private readonly IOptions<LocalStorageSettings> _localStorageSettings;

    private readonly IState<LocationState> _locationState;

    private readonly IState<WeatherState> _weatherState;

    private readonly IWebHostEnvironment _webHostEnvironment;

    private bool _isLoadedFromLocalStorage;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Home" /> class.
    /// </summary>
    /// <param name="defaultLocationSettings">The default location configuration settings.</param>
    /// <param name="dispatcher">The Fluxor dispatcher for dispatching actions.</param>
    /// <param name="localizer">The localizer for component-specific strings.</param>
    /// <param name="localStorage">The protected local storage for retrieving the last selected location.</param>
    /// <param name="localStorageSettings">
    ///     The local storage configuration settings containing the prefix and environment isolation options.
    /// </param>
    /// <param name="locationState">The Fluxor location state.</param>
    /// <param name="weatherState">The Fluxor weather state.</param>
    /// <param name="webHostEnvironment">The web host environment for accessing the current environment name.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when any parameter is <see langword="null" />.
    /// </exception>
    public Home(IOptions<DefaultLocationSettings> defaultLocationSettings,
                IDispatcher dispatcher,
                IStringLocalizer<Home> localizer,
                ProtectedLocalStorage localStorage,
                IOptions<LocalStorageSettings> localStorageSettings,
                IState<LocationState> locationState,
                IState<WeatherState> weatherState,
                IWebHostEnvironment webHostEnvironment)
    {
        _defaultLocationSettings = defaultLocationSettings ?? throw new ArgumentNullException(nameof(defaultLocationSettings));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
        _localStorageSettings = localStorageSettings ?? throw new ArgumentNullException(nameof(localStorageSettings));
        _locationState = locationState ?? throw new ArgumentNullException(nameof(locationState));
        _weatherState = weatherState ?? throw new ArgumentNullException(nameof(weatherState));
        _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
    }

    /// <summary>
    ///     Gets the currently selected location from the Fluxor state.
    /// </summary>
    private LocationDocument? CurrentLocation => _locationState.Value.CurrentLocation;

    /// <summary>
    ///     Lifecycle method that runs after the component renders.
    ///     On first render, loads the last selected location from browser storage and dispatches a selection action.
    /// </summary>
    /// <param name="firstRender">Indicates whether this is the first render of the component.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if ( !firstRender || _isLoadedFromLocalStorage )
        {
            return;
        }

        LocationDocument document = await GetLocationFromStorageOrDefault();
        
        _dispatcher.Dispatch(new SelectLocationAction(document));

        _isLoadedFromLocalStorage = true;
    }

    /// <summary>
    ///     Retrieves the default location from configuration settings.
    /// </summary>
    /// <returns>A <see cref="LocationDocument" /> representing the configured default location.</returns>
    private LocationDocument GetDefaultLocation()
    {
        DefaultLocationSettings settings = _defaultLocationSettings.Value;

        return new LocationDocument(
            Guid.Empty,
            settings.Locality,
            settings.Province,
            settings.ProvinceCode,
            [],
            settings.Latitude,
            settings.Longitude
        );
    }

    /// <summary>
    ///     Attempts to retrieve the last selected location from protected browser storage,
    ///     falling back to the default location if retrieval fails or no location is stored.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     the stored <see cref="LocationDocument" /> or the default location.
    /// </returns>
    private async ValueTask<LocationDocument> GetLocationFromStorageOrDefault()
    {
        string keyName = _localStorageSettings.Value.GetLocalStorageKey(WeatherConstants.LastSelectedLocationKey,
            _webHostEnvironment.EnvironmentName);

        ProtectedBrowserStorageResult<LocationDocument> document = await _localStorage.GetAsync<LocationDocument>(keyName);

        return document is { Success: true, Value: not null, }
                   ? document.Value
                   : GetDefaultLocation();
    }
}
