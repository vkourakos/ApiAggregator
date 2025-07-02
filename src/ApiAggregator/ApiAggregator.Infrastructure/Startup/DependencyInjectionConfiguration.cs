using ApiAggregator.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiAggregator.Infrastructure.Startup;

public static class DependencyInjectionConfiguration
{
    public static void ConfigureDependencyInjection(this IServiceCollection services,
        IConfiguration configuration)
    {
        var modules = ModuleLoader.LoadAll();

        foreach (var module in modules)
            module.ConfigureDependencyInjection(services, configuration);
    }
}