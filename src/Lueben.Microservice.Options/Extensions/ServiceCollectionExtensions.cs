using Lueben.Microservice.Options.OptionManagers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Lueben.Microservice.Options.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLuebenAzureAppConfigurationWithNoRefresher(this IServiceCollection services)
        {
            return services
                .AddAzureAppConfiguration()
                .AddSingleton<IRefreshedOptionsManager, RefreshedOptionsManagerWithNoRefresher>()
                .AddFeatureManagement()
                .Services;
        }

        public static IServiceCollection AddLuebenAzureAppConfiguration(this IServiceCollection services)
        {
            return services
                .AddAzureAppConfiguration()
                .AddSingleton<IRefreshedOptionsManager, RefreshedOptionsManager>()
                .AddFeatureManagement()
                .Services;
        }
    }
}
