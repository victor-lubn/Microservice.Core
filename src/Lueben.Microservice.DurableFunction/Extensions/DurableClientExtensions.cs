using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Lueben.Microservice.DurableFunction.Extensions
{
    public static class DurableClientExtensions
    {
        public static async Task<string> StartNewAsyncWithRetry<T>(this IDurableOrchestrationClient client, string orchestratorFunctionName, T input)
            where T : class
        {
            return await client.StartNewAsync(orchestratorFunctionName, new RetryData<T>(input));
        }
    }
}