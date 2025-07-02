using ApiAggregator.Application.Interfaces;
using ApiAggregator.Domain;
using Microsoft.Extensions.Logging;

namespace ApiAggregator.Infrastructure.Services;

public class AggregationService : IAggregationService
{
    private readonly IEnumerable<IApiClient> _apiClients;
    private readonly ILogger<AggregationService> _logger;

    public AggregationService(
        IEnumerable<IApiClient> apiClients,
        ILogger<AggregationService> logger)
    {
        _apiClients = apiClients;
        _logger = logger;
    }

    public async Task<IEnumerable<AggregatedData>> AggregateData(string query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting data aggregation for query: {Query}", query);

        var tasks = _apiClients.Select(client => GetData(client, query, cancellationToken)).ToList();

        var results = await Task.WhenAll(tasks);

        var allData = results.SelectMany(result => result).ToList();

        _logger.LogInformation("Aggregated {Count} items successfully.", allData.Count);
        return allData;
    }


    private async Task<IEnumerable<AggregatedData>> GetData(IApiClient client, string query, CancellationToken cancellationToken)
    {
        try
        {
            return await client.GetDataAsync(query, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from {SourceApi} for query: {Query}", client.SourceName, query);
            return [];
        }
    }
}
