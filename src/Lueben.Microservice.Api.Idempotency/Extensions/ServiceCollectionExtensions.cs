using Lueben.Microservice.Api.Idempotency.FunctionWrappers;
using Lueben.Microservice.Api.Idempotency.IdempotencyDataProviders;
using Lueben.Microservice.Api.Idempotency.Models;
using Lueben.Microservice.Api.Idempotency.Options;
using Lueben.Microservice.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.Api.Idempotency.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureTableIdempotency(this IServiceCollection services)
        {
            services.RegisterConfiguration<IdempotencyAzureTableOptions>(nameof(IdempotencyAzureTableOptions));

            return services
                .AddHttpContextAccessor()
                .AddTransient<IIdempotencyDataProvider<IdempotencyEntity>, AzureTableIdempotencyDataProvider>()
                .AddTransient<IFunctionWrapper, IdempotencyFunctionWrapper>();
#pragma warning restore 618
        }
    }
}
