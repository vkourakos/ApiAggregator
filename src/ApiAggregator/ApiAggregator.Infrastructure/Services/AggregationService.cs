using ApiAggregator.Application.Interfaces;
using ApiAggregator.Domain;
using ApiAggregator.Domain.Enumerations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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

    public async Task<IEnumerable<AggregatedData>> AggregateData(
        string query,
        SortField sortBy,
        SortOrder sortOrder,
        string? sources,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"raw_aggregation_result_{query.ToLowerInvariant()}";

        if (!_cache.TryGetValue(cacheKey, out List<AggregatedData>? allData))
        {
            _logger.LogInformation("Cache miss. Starting data aggregation for query: {Query}", query);
            var tasks = _apiClients.Select(client => GetData(client, query, cancellationToken)).ToList();
            var results = await Task.WhenAll(tasks);
            allData = results.SelectMany(result => result).ToList();

            _logger.LogInformation("Aggregated {Count} items successfully.", allData.Count);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));
            _cache.Set(cacheKey, allData, cacheEntryOptions);
        }
        else
        {
            _logger.LogInformation("Cache hit for raw data for query: {Query}", query);
        }

        IEnumerable<AggregatedData> processedData = allData!;

        if (!string.IsNullOrWhiteSpace(sources))
        {
            var sourceList = sources.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (sourceList.Any())
            {
                processedData = processedData.Where(d => sourceList.Contains(d.SourceApi, StringComparer.OrdinalIgnoreCase));
            }
        }

        var sortedData = SortData(processedData, sortBy, sortOrder);

        return sortedData;
    }


    private async Task<IEnumerable<AggregatedData>> GetData(IApiClient client, string query, CancellationToken cancellationToken)
    {
        try
        {
            return await client.GetData(query, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP Error fetching data from {SourceApi}", client.SourceName);
            return [];
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON Deserialization Error from {SourceApi}", client.SourceName);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred for {SourceApi}", client.SourceName);
            return [];
        }
    }

    private static IEnumerable<AggregatedData> SortData(
    IEnumerable<AggregatedData> data,
    SortField sortBy,
    SortOrder sortOrder)
    {
        var sorted = sortBy switch
        {
            SortField.Title => sortOrder == SortOrder.Ascending
                ? data.OrderBy(d => d.Title, StringComparer.OrdinalIgnoreCase)
                : data.OrderByDescending(d => d.Title, StringComparer.OrdinalIgnoreCase),

            SortField.SourceApi => sortOrder == SortOrder.Ascending
                ? data.OrderBy(d => d.SourceApi, StringComparer.OrdinalIgnoreCase)
                : data.OrderByDescending(d => d.SourceApi, StringComparer.OrdinalIgnoreCase),

            _ => sortOrder == SortOrder.Ascending
                ? data.OrderBy(d => d.PublishedDate)
                : data.OrderByDescending(d => d.PublishedDate)
        };

        return sorted.ThenBy(d => d.Title);
    }
}
