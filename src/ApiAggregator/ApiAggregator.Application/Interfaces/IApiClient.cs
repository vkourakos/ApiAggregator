using ApiAggregator.Domain;

namespace ApiAggregator.Application.Interfaces;

public interface IApiClient
{
    string SourceName { get; }
    Task<IEnumerable<AggregatedData>> GetDataAsync(string query, CancellationToken cancellationToken);
}
