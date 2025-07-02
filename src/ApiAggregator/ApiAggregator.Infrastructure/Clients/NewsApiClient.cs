using ApiAggregator.Application.Interfaces;
using ApiAggregator.Domain;
using ApiAggregator.Infrastructure.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace ApiAggregator.Infrastructure.Clients;

public class NewsApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly string _apiKey;
    public string SourceName => "NewsAPI";

    public NewsApiClient(HttpClient httpClient, IConfiguration configuration, IMapper mapper)
    {
        _httpClient = httpClient;
        _mapper = mapper;
        _httpClient.BaseAddress = new Uri(configuration["ApiSettings:NewsApi:BaseUrl"]!);
        _apiKey = configuration["ApiSettings:NewsApi:ApiKey"]!;
    }

    public async Task<IEnumerable<AggregatedData>> GetData(string query, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetFromJsonAsync<NewsApiResponse>(
            $"everything?q={query}&sortBy=popularity&apiKey={_apiKey}", cancellationToken);

        return _mapper.Map<IEnumerable<AggregatedData>>(response?.Articles) ?? Enumerable.Empty<AggregatedData>();
    }
}
