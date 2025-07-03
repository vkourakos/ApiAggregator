using ApiAggregator.Application.Interfaces;
using ApiAggregator.Domain;
using ApiAggregator.Infrastructure.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Web;

namespace ApiAggregator.Infrastructure.Clients;

public class NewsApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    public string SourceName => "NewsAPI";

    public NewsApiClient(HttpClient httpClient, IConfiguration configuration, IMapper mapper)
    {
        _httpClient = httpClient;
        _mapper = mapper;
        _httpClient.BaseAddress = new Uri(configuration["ApiSettings:NewsApi:BaseUrl"]!);

        var apiKey = configuration["ApiSettings:NewsApi:ApiKey"]!;
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", apiKey);
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "ApiAggregator");
    }

    public async Task<IEnumerable<AggregatedData>> GetData(string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var encodedQuery = HttpUtility.UrlEncode(query);
        var fromDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var requestUri = $"everything?q={encodedQuery}&from={fromDate}&sortBy=relevancy&pageSize=5";

        var response = await _httpClient.GetFromJsonAsync<NewsApiResponse>(requestUri, cancellationToken);

        return _mapper.Map<IEnumerable<AggregatedData>>(response?.Articles) ?? [];
    }
}
