using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Lueben.Microservice.OpenApi.Tests.v1
{
    public class GetOperation2HttpTrigger
    {
        [Function(nameof(GetOperation1HttpTrigger))]
        [OpenApiOperation(operationId: nameof(GetOperation2))]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        public static async Task<IActionResult> GetOperation2([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "v1/operation2")] HttpRequest req)
        {
            var result = new OkResult();

            return await Task.FromResult(result).ConfigureAwait(false);
        }
    }
}