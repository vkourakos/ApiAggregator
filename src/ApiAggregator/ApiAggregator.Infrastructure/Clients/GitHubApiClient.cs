using ApiAggregator.Application.Interfaces;
using ApiAggregator.Domain;
using ApiAggregator.Infrastructure.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace ApiAggregator.Infrastructure.Clients;

public class GitHubApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    public string SourceName => "GitHub";

    public GitHubApiClient(HttpClient httpClient, IConfiguration configuration, IMapper mapper)
    {
        _httpClient = httpClient;
        _mapper = mapper;
        _httpClient.BaseAddress = new Uri(configuration["ApiSettings:GitHubApi:BaseUrl"]!);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ApiAggregator");
        _httpClient.DefaultRequestHeaders.Accept.Add(new("application/vnd.github+json"));
    }

    public async Task<IEnumerable<AggregatedData>> GetData(string query, CancellationToken cancellationToken)
    {
        var repos = await _httpClient.GetFromJsonAsync<List<GitHubRepo>>($"users/{query}/repos", cancellationToken);
        return _mapper.Map<IEnumerable<AggregatedData>>(repos) ?? [];
    }
}
