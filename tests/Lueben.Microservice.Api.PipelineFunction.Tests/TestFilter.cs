using System.Threading;
using System.Threading.Tasks;
using Lueben.Microservice.Api.PipelineFunction.Filters;
using Microsoft.Azure.WebJobs.Host;
#pragma warning disable 618

namespace Lueben.Microservice.Api.PipelineFunction.Tests
{
    public class TestFilter : FunctionBaseInvocationFilter
    {
        public bool Executed { get; set; }

        public override Task OnPipelineExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            Executed = true;

            return base.OnPipelineExecutedAsync(executedContext, cancellationToken);
        }

        public override Task OnPipelineExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            Executed = true;

            return base.OnPipelineExecutingAsync(executingContext, cancellationToken);
        }
    }
}