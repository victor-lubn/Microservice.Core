using System.Net;
using Lueben.Microservice.OpenApi.Tests.v1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Lueben.Microservice.OpenApi.Tests.v2
{
    public class GetOperation1HttpTriggerV2
    {
        [Function(nameof(GetOperation1HttpTrigger))]
        [OpenApiOperation(operationId: nameof(GetOperation1))]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        public static async Task<IActionResult> GetOperation1([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "v2/operation1")] HttpRequest req)
        {
            var result = new OkResult();

            return await Task.FromResult(result).ConfigureAwait(false);
        }
    }
}