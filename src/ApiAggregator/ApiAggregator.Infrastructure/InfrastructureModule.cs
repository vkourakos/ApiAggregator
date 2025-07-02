using ApiAggregator.Application.Interfaces;
using ApiAggregator.DependencyInjection;
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
    }
}
