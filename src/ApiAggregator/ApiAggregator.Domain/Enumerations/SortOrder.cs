using System.Text.Json.Serialization;

namespace ApiAggregator.Domain.Enumerations;

public enum SortOrder
{
    [JsonPropertyName("asc")]
    Ascending,

    [JsonPropertyName("desc")]
    Descending
}
