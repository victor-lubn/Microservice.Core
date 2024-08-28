using Lueben.Microservice.Extensions.Configuration;
using Lueben.Microservice.RestSharpClient.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.RestSharpClient.Factory
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMicroserviceRestSharpClientFactory(this IServiceCollection services)
        {
            return services
                .RegisterNamedOptions<RestSharpClientOptions>()
                .AddTransient<IRestSharpClientFactory, MicroserviceRestSharpClientFactory>()
                .AddHttpClient();
        }
    }
}