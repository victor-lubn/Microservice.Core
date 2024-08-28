using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.Api.PipelineFunction
{
    [ExcludeFromCodeCoverage]
    public abstract class PipelineFunctionsStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
            ConfigurePipeline(builder.Services);
        }

        public virtual IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services;
        }

        public abstract IServiceCollection ConfigurePipeline(IServiceCollection services);
    }
}
