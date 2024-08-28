using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Lueben.Microservice.Api.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Lueben.Microservice.Api.Middleware.Middleware
{
    public abstract class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                var errorResult = GetErrorResult(exception);
                if (errorResult == null)
                {
                    throw;
                }

                var httpReqData = await context.GetHttpRequestDataAsync();
                if (httpReqData != null)
                {
                    if (Activity.Current != null)
                    {
                        errorResult.DebugId = Activity.Current.RootId;
                    }

                    var newHttpResponse = httpReqData!.CreateResponse(errorResult.StatusCode);

                    await newHttpResponse.WriteAsJsonAsync(errorResult, errorResult.StatusCode);

                    context.GetInvocationResult().Value = newHttpResponse;
                }
            }
        }

        public abstract ErrorResult GetErrorResult(Exception exception);
    }
}
