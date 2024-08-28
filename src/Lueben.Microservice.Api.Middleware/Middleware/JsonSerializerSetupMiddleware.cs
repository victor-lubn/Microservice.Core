using System.Threading.Tasks;
using Lueben.Microservice.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Newtonsoft.Json;

namespace Lueben.Microservice.Api.Middleware.Middleware
{
    public class JsonSerializerSetupMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            JsonConvert.DefaultSettings = FunctionJsonSerializerSettingsProvider.CreateSerializerSettings;

            await next(context);
        }
    }
}
