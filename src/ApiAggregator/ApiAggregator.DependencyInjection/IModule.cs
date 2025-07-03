using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiAggregator.DependencyInjection;

public interface IModule
{
    void ConfigureDependencyInjection(IServiceCollection services, IConfiguration configuration);
}