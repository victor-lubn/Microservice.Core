using System.Threading.Tasks;
using Lueben.Microservice.Json.Encrypt.Configurations;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.Json.Encrypt.Middleware
{
    public class JsonEncryptingMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IOptions<EncryptionOptions> _options;

        public JsonEncryptingMiddleware(IOptions<EncryptionOptions> options)
        {
            _options = options;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var defaultSettings = JsonConvert.DefaultSettings?.Invoke() ?? new JsonSerializerSettings();
            defaultSettings.ContractResolver = new EncryptedStringPropertyResolver(_options)
            {
                NamingStrategy = (defaultSettings.ContractResolver as DefaultContractResolver)?.NamingStrategy,
            };
            JsonConvert.DefaultSettings = () => defaultSettings;

            await next(context);
        }
    }
}
