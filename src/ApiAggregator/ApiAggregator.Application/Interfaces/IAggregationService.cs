using ApiAggregator.Domain;

namespace ApiAggregator.Application.Interfaces;

public interface IAggregationService
{
    Task<IEnumerable<AggregatedData>> AggregateData(string query, CancellationToken cancellationToken);
}
