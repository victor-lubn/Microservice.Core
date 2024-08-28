using System;
using System.Linq;
using Lueben.Microservice.OpenApi.DocumentFilters;
using Lueben.Microservice.OpenApi.Options;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Configurations;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.OpenApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenApi(this IServiceCollection services, Action<LuebenOpenApiHttpTiggerContextOptions> builder = null)
        {
            return services.AddOpenApi((_, options) => builder(options));
        }

        public static IServiceCollection AddOpenApi(this IServiceCollection services, Action<IServiceProvider, LuebenOpenApiHttpTiggerContextOptions> builder = null)
        {
            SetAuthorizationLevelForOpenApiFunctions();

            services.AddSingleton<OpenApiHttpTriggerContext, LuebenOpenApiHttpTriggerContext>(serviceProvider =>
            {
                var options = new LuebenOpenApiHttpTiggerContextOptions();

                builder?.Invoke(serviceProvider, options);
                PostConfigureOpenApiHttpTiggerContextOptions(options, serviceProvider);

                return new LuebenOpenApiHttpTriggerContext(options);
            });

            return services;
        }

        private static void SetAuthorizationLevelForOpenApiFunctions(AuthorizationLevel level = AuthorizationLevel.Function)
        {
            var levelString = level.ToString();

            Environment.SetEnvironmentVariable($"{OpenApiSettings.Name}:{nameof(OpenApiSettings.AuthLevel)}:{nameof(OpenApiAuthLevelSettings.Document)}", levelString);
            Environment.SetEnvironmentVariable($"{OpenApiSettings.Name}:{nameof(OpenApiSettings.AuthLevel)}:{nameof(OpenApiAuthLevelSettings.UI)}", levelString);
        }

        private static void PostConfigureOpenApiHttpTiggerContextOptions(LuebenOpenApiHttpTiggerContextOptions options, IServiceProvider serviceProvider)
        {
            options.UseOpenApiConfigurationFile("openapisettings.json", serviceProvider.GetRequiredService<IConfiguration>())
                .AddDocumentFilter(opt => new CommonApiResponsesFilter(opt.NamingStrategy))
                .AddDocumentFilter(_ => new EndpointsByApiVersionDocumentFilter())
                .AddDocumentFilter(_ => new EndpointsByOperationIdsDocumentFilter());

            if (options.CommonOpenApiParameters?.Any() == true)
            {
                options.AddDocumentFilter(new CommonApiParametersFilter(options.CommonOpenApiParameters));
            }
        }
    }
}