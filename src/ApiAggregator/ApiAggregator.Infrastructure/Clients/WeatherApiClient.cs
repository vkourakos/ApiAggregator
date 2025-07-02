using ApiAggregator.Application.Interfaces;
using ApiAggregator.Domain;
using ApiAggregator.Infrastructure.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace ApiAggregator.Infrastructure.Clients;

public class WeatherApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly string _apiKey;
    public string SourceName => "WeatherAPI.com";

    public WeatherApiClient(HttpClient httpClient, IConfiguration configuration, IMapper mapper)
    {
        _httpClient = httpClient;
        _mapper = mapper;
        _httpClient.BaseAddress = new Uri(configuration["ApiSettings:WeatherApi:BaseUrl"]!);
        _apiKey = configuration["ApiSettings:WeatherApi:ApiKey"]!;
    }

    public async Task<IEnumerable<AggregatedData>> GetData(string query, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetFromJsonAsync<WeatherApiResponse>(
            $"current.json?key={_apiKey}&q={query}", cancellationToken);

        if (response is null)
        {
            return [];
        }

        var mappedResponse = _mapper.Map<AggregatedData>(response);
        return new[] { mappedResponse };
    }
}
