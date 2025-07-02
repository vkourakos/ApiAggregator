namespace ApiAggregator.Domain;

public class AggregatedData
{
    public string SourceApi { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Url { get; set; } = null!;
    public DateTime? PublishedDate { get; set; }
}
