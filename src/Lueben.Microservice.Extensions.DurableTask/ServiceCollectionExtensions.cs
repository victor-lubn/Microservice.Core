using System.Diagnostics.CodeAnalysis;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.Extensions.DurableTask
{
    public static class ServiceCollectionExtensions
    {
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddDurableFunctions(this IServiceCollection services)
        {
            services.AddDurableTaskClient(builder =>
            {
                builder.UseGrpc();
            });

            return services;
        }
    }
}
