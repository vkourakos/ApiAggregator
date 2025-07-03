using ApiAggregator.Domain;
using ApiAggregator.Domain.Enumerations;

namespace ApiAggregator.Application.Interfaces;

public interface IAggregationService
{
    Task<IEnumerable<AggregatedData>> AggregateData(
        string query,
        SortField sortBy,
        SortOrder sortOrder,
        string? sources,
        CancellationToken cancellationToken);
}
