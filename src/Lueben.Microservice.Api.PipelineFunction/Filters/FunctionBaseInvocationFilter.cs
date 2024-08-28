using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;

#pragma warning disable 618

namespace Lueben.Microservice.Api.PipelineFunction.Filters
{
    public abstract class FunctionBaseInvocationFilter : IFunctionInvocationFilter
    {
        public Task OnExecutingAsync(FunctionExecutingContext context, CancellationToken cancellationToken)
        {
            if (!PipelineFunction.IsPipelineEnabled(context))
            {
                return Task.CompletedTask;
            }

            return OnPipelineExecutingAsync(context, cancellationToken);
        }

        public Task OnExecutedAsync(FunctionExecutedContext context, CancellationToken cancellationToken)
        {
            if (!PipelineFunction.IsPipelineEnabled(context))
            {
                return Task.CompletedTask;
            }

            return OnPipelineExecutedAsync(context, cancellationToken);
        }

        public virtual Task OnPipelineExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnPipelineExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}