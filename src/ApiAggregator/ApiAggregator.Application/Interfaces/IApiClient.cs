using ApiAggregator.Domain;

namespace ApiAggregator.Application.Interfaces;

public interface IApiClient
{
    string SourceName { get; }
    Task<IEnumerable<AggregatedData>> GetData(string query, CancellationToken cancellationToken);
}
