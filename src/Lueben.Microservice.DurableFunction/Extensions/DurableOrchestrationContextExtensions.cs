using System.Threading.Tasks;
using Lueben.Microservice.ApplicationInsights;
using Lueben.Microservice.DurableFunction.Exceptions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Lueben.Microservice.DurableFunction.Extensions
{
    public static class DurableOrchestrationContextExtensions
    {
        public static async Task CallDurableActivity(this IDurableOrchestrationContext context, string activityFunctionName, object data, WorkflowOptions options = null)
        {
            var retryOptions = GetRetryOptions(options ?? new WorkflowOptions());
            await context.CallActivityWithRetryAsync(activityFunctionName, retryOptions, data);
        }

        public static async Task<TResult> CallDurableActivity<TResult>(this IDurableOrchestrationContext context, string activityFunctionName, object data, WorkflowOptions options = null)
        {
            var retryOptions = GetRetryOptions(options ?? new WorkflowOptions());
            return await context.CallActivityWithRetryAsync<TResult>(activityFunctionName, retryOptions, data);
        }

        public static RetryOptions GetRetryOptions(WorkflowOptions options)
        {
            var retryCount = options.MaxEventRetryCount == 0 ? int.MaxValue : options.MaxEventRetryCount;
            var retryOptions = new RetryOptions(options.ActivityRetryIntervalTime, retryCount)
            {
                BackoffCoefficient = options.BackoffCoefficient,
                MaxRetryInterval = options.ActivityMaxRetryIntervalTime,
                RetryTimeout = options.ActivityRetryTimeout,
                Handle = e =>
                {
                    switch (e?.InnerException)
                    {
                        case EventDataProcessFailureException _:
                            return true;
                        default: return false;
                    }
                }
            };
            return retryOptions;
        }

        public static ILoggerService CreateReplaySafeLoggerService(this IDurableOrchestrationContext context, ILoggerService loggerService)
        {
            return new ReplaySafeLoggerService(loggerService, context);
        }
    }
}