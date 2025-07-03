using ApiAggregator.Application.Interfaces;
using ApiAggregator.Domain;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ApiAggregator.Infrastructure.Services;

public class AggregationService : IAggregationService
{
    private readonly IEnumerable<IApiClient> _apiClients;
    private readonly ILogger<AggregationService> _logger;
    private readonly IMemoryCache _cache;

    public AggregationService(
        IEnumerable<IApiClient> apiClients,
        ILogger<AggregationService> logger,
        IMemoryCache cache)
    {
        _apiClients = apiClients;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<AggregatedData>> AggregateData(string query, CancellationToken cancellationToken)
    {
        var cacheKey = $"aggregation_result_{query.ToLowerInvariant()}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<AggregatedData>? cachedData))
        {
            _logger.LogInformation("Returning cached data for query: {Query}", query);
            return cachedData ?? [];
        }

        _logger.LogInformation("Cache miss. Starting data aggregation for query: {Query}", query);

        var tasks = _apiClients.Select(client => GetData(client, query, cancellationToken)).ToList();

        var results = await Task.WhenAll(tasks);
        var allData = results.SelectMany(result => result).ToList();

        _logger.LogInformation("Aggregated {Count} items successfully.", allData.Count);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

        _cache.Set(cacheKey, allData, cacheEntryOptions);

        return allData;
    }


    private async Task<IEnumerable<AggregatedData>> GetData(IApiClient client, string query, CancellationToken cancellationToken)
    {
        try
        {
            return await client.GetData(query, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from {SourceApi} for query: {Query}", client.SourceName, query);
            return [];
        }
    }
}
