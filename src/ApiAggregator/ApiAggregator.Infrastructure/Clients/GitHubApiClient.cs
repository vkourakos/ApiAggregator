using ApiAggregator.Application.Interfaces;
using ApiAggregator.Domain;
using ApiAggregator.Infrastructure.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Web;

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
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "ApiAggregator");
        _httpClient.DefaultRequestHeaders.Accept.Add(new("application/vnd.github+json"));
    }

    public async Task<IEnumerable<AggregatedData>> GetData(string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Contains(' '))
        {
            return [];
        }

        var encodedQuery = HttpUtility.UrlEncode(query);

        var requestUri = $"users/{encodedQuery}/repos?sort=pushed&per_page=5";

        var repos = await _httpClient.GetFromJsonAsync<List<GitHubRepo>>(requestUri, cancellationToken);

        return _mapper.Map<IEnumerable<AggregatedData>>(repos) ?? [];
    }
}