using Lueben.Microservice.OpenApi.Version.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.OpenApi.Version.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenApiVersion(this IServiceCollection services)
        {
            return services
                .AddTransient<IVersionService, VersionService>()
                .AddTransient<IVersionedMethodsQuery, VersionedMethodsQuery>()
                .AddTransient<IDocumentHelper, DocumentHelper>(_ => new DocumentHelper(new RouteConstraintFilter(), new OpenApiSchemaAcceptor()));
        }
    }
}
