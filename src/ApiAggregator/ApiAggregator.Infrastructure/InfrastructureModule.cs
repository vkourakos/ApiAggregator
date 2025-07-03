using ApiAggregator.Application.Interfaces;
using ApiAggregator.DependencyInjection;
using ApiAggregator.Infrastructure.Clients;
using ApiAggregator.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiAggregator.Infrastructure;

public class InfrastructureModule : IModule
{
    public void ConfigureDependencyInjection(IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IAggregationService, AggregationService>();

        services.AddHttpClient<GitHubApiClient>();
        services.AddHttpClient<NewsApiClient>();
        services.AddHttpClient<WeatherApiClient>();

        services.AddScoped<IApiClient, GitHubApiClient>();
        services.AddScoped<IApiClient, NewsApiClient>();
        services.AddScoped<IApiClient, WeatherApiClient>();
    }
}
