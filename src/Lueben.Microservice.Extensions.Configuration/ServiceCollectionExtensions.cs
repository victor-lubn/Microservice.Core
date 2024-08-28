using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.Extensions.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterConfiguration<TOptions>(this IServiceCollection services, string sectionName)
            where TOptions : class, new()
        {
            return services
                .AddOptions<TOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.Bind(sectionName, settings);
                }).Services;
        }

        public static IServiceCollection RegisterNamedOptions<TOptions>(this IServiceCollection services, IList<string> sections = null)
            where TOptions : class
        {
            var provider = services.BuildServiceProvider();
            var configuration = provider.GetService<IConfiguration>();
            var namedSections = configuration.GetSection(typeof(TOptions).Name)?.GetChildren();

            if (namedSections == null)
            {
                return services;
            }

            foreach (var section in namedSections)
            {
                if (sections == null || sections.Contains(section.Key))
                {
                    services.Configure<TOptions>(section.Key, section);
                }
            }

            return services;
        }
    }
}