using System.Threading.Tasks;
using Lueben.Microservice.Api.PipelineFunction.Constants;
using Lueben.Microservice.Api.ValidationFunction.Exceptions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Lueben.Microservice.Api.ValidationFunction.Middleware
{
    public class RequiredHeadersMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
                var httpContext = context.GetHttpContext();
                if (httpContext != null)
                {
                    if (!httpContext.Request.Headers.ContainsKey(Headers.SourceConsumer))
                    {
                        throw new ModelNotValidException($"{nameof(Headers.SourceConsumer)} header", $"'{Headers.SourceConsumer}' header is not set", "request headers");
                    }
                }

                await next(context);
        }
    }
}
