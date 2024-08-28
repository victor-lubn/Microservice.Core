using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Lueben.Microservice.Api.Middleware.Middleware
{
    public class HttpContextSetupMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IHttpContextAccessor _httpAccessor;

        public HttpContextSetupMiddleware(IHttpContextAccessor httpAccessor)
        {
            _httpAccessor = httpAccessor;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            _httpAccessor.HttpContext = context.GetHttpContext();

            await next(context);
        }
    }
}
