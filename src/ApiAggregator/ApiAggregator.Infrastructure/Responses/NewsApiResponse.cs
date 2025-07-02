using System.Text.Json.Serialization;

namespace ApiAggregator.Infrastructure.Responses;

public class NewsApiResponse
{
    [JsonPropertyName("articles")]
    public List<Article> Articles { get; set; } = new();
}

public class Article
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    [JsonPropertyName("publishedAt")]
    public DateTime PublishedAt { get; set; }
}
