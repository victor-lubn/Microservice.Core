using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Lueben.Microservice.OpenApi.Tests.v1
{
    public class DeleteOperation1HttpTrigger
    {
        [Function(nameof(GetOperation1HttpTrigger))]
        [OpenApiOperation(operationId: nameof(DeleteOperation1))]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        public static async Task<IActionResult> DeleteOperation1([HttpTrigger(AuthorizationLevel.Anonymous, "DELETE", Route = "v1/operation1")] HttpRequest req)
        {
            var result = new OkResult();

            return await Task.FromResult(result).ConfigureAwait(false);
        }
    }
}