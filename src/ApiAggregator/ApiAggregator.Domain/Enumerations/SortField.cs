using System.Text.Json.Serialization;

namespace ApiAggregator.Domain.Enumerations;

public enum SortField
{
    [JsonPropertyName("date")]
    PublishedDate,

    [JsonPropertyName("title")]
    Title,

    [JsonPropertyName("source")]
    SourceApi
}
