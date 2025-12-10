namespace WeatherDashboard.Web.Features.Weather.Components;

using Configuration;
using Domain.Entities.Documents;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Radzen;
using StateManagement;
using StateManagement.Actions;

/// <summary>
///     Code-behind for the LocationSearchField component that provides location search functionality
///     with autocomplete and persists the last selected location to browser storage.
/// </summary>
public sealed partial class LocationSearchField : FluxorComponent
{
    private readonly IDispatcher _dispatcher;

    private readonly IStringLocalizer<LocationSearchField> _localizer;

    private readonly ProtectedLocalStorage _localStorage;

    private readonly IOptions<LocalStorageSettings> _localStorageSettings;

    private readonly IState<LocationState> _locationState;

    private readonly IWebHostEnvironment _webHostEnvironment;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LocationSearchField" /> class.
    /// </summary>
    /// <param name="dispatcher">The Fluxor dispatcher for dispatching actions.</param>
    /// <param name="localizer">The localizer for component-specific strings.</param>
    /// <param name="localStorage">The protected local storage for persisting location selection.</param>
    /// <param name="localStorageSettings">
    ///     The local storage configuration settings containing the prefix and environment
    ///     isolation options.
    /// </param>
    /// <param name="locationState">The Fluxor location state.</param>
    /// <param name="webHostEnvironment">The web host environment for accessing the current environment name.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when any parameter is <see langword="null" />.
    /// </exception>
    public LocationSearchField(IDispatcher dispatcher,
                               IStringLocalizer<LocationSearchField> localizer,
                               ProtectedLocalStorage localStorage,
                               IOptions<LocalStorageSettings> localStorageSettings,
                               IState<LocationState> locationState,
                               IWebHostEnvironment webHostEnvironment)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
        _localStorageSettings = localStorageSettings ?? throw new ArgumentNullException(nameof(localStorageSettings));
        _locationState = locationState ?? throw new ArgumentNullException(nameof(locationState));
        _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
    }

    /// <summary>
    ///     Gets the collection of location search results from the Fluxor state.
    /// </summary>
    private IReadOnlyCollection<LocationDocument> SearchResults => _locationState.Value.SearchResults;

    /// <summary>
    ///     Handles the search event by dispatching a <see cref="SearchLocationAction" /> with the search query.
    /// </summary>
    /// <param name="arguments">The load data arguments containing the search filter.</param>
    private void OnSearchLocation(LoadDataArgs arguments)
    {
        _dispatcher.Dispatch(new SearchLocationAction(arguments.Filter));
    }

    /// <summary>
    ///     Handles the selection of a location by persisting it to local storage and dispatching
    ///     a <see cref="SelectLocationAction" />.
    /// </summary>
    /// <param name="obj">The selected object, expected to be a <see cref="LocationDocument" />.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task OnSelectedItemChangedAsync(object obj)
    {
        if ( obj is not LocationDocument document )
        {
            return;
        }

        string keyName = _localStorageSettings.Value.GetLocalStorageKey(WeatherConstants.LastSelectedLocationKey,
            _webHostEnvironment.EnvironmentName);

        await _localStorage.SetAsync(keyName, document);

        _dispatcher.Dispatch(new SelectLocationAction(document));
    }
}
