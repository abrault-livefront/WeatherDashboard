namespace WeatherDashboard.Web.Features.Weather.StateManagement.Effects;

using System.Diagnostics.CodeAnalysis;
using Actions;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities.Documents;
using Fluxor;

/// <summary>
///     Fluxor effect that handles searching for locations using the Lucene.NET search service.
/// </summary>
/// <remarks>
///     This effect responds to <see cref="SearchLocationAction" /> by querying the location search service
///     with a minimum query length of 3 characters, then dispatching <see cref="SearchLocationSuccessAction" />
///     with the search results.
/// </remarks>
[SuppressMessage("Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Effect instantiated by Fluxor")]
internal sealed class SearchLocationEffect : Effect<SearchLocationAction>
{
    /// <summary>
    ///     The minimum number of characters required in the search query to perform a search.
    /// </summary>
    private const int MinimumQueryLength = 3;

    private readonly ISearchService<LocationDocument> _searchService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SearchLocationEffect" /> class.
    /// </summary>
    /// <param name="searchService">The search service used to query location documents.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="searchService" /> is <see langword="null" />.
    /// </exception>
    public SearchLocationEffect(ISearchService<LocationDocument> searchService)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
    }

    /// <summary>
    ///     Handles the <see cref="SearchLocationAction" /> by searching for locations
    ///     and dispatching the results.
    /// </summary>
    /// <param name="action">The action containing the search query.</param>
    /// <param name="dispatcher">The Fluxor dispatcher used to dispatch result actions.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="action" /> or <paramref name="dispatcher" /> is <see langword="null" />.
    /// </exception>
    public override Task HandleAsync(SearchLocationAction action, IDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(dispatcher);

        if ( !string.IsNullOrWhiteSpace(action.Query) && action.Query.Length >= MinimumQueryLength )
        {
            return HandleInternalAsync(action, dispatcher);
        }

        dispatcher.Dispatch(new SearchLocationSuccessAction([]));
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Internal handler that performs the location search and dispatches the results.
    /// </summary>
    /// <param name="action">The action containing the search query.</param>
    /// <param name="dispatcher">The Fluxor dispatcher.</param>
    /// <returns>A completed task.</returns>
    private Task HandleInternalAsync(SearchLocationAction action, IDispatcher dispatcher)
    {
        SearchResult<LocationDocument> results = _searchService.Search(action.Query);

        dispatcher.Dispatch(new SearchLocationSuccessAction(results.Results));

        return Task.CompletedTask;
    }
}
