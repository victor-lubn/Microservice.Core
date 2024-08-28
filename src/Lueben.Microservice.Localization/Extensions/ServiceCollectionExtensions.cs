using Lueben.Microservice.Extensions.Configuration;
using Lueben.Microservice.Localization.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.Localization.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLuebenLocalization(this IServiceCollection services, bool enableFilter = true)
        {
            services.RegisterConfiguration<LocalizationOptions>(nameof(LocalizationOptions));
            var serviceProvider = services.BuildServiceProvider();
            var localizationOptions = serviceProvider.GetService<IOptions<LocalizationOptions>>().Value;
            LocalizationOptions.Value = localizationOptions;

            return services;
        }
    }
}
