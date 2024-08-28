using System.Net;
using System.Net.Mime;
using Lueben.Microservice.Api.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Lueben.Microservice.OpenApi.Attributes
{
    public sealed class OpenApiInternalServerErrorResponseAttribute : OpenApiResponseWithBodyAttribute
    {
        public OpenApiInternalServerErrorResponseAttribute() : base(HttpStatusCode.InternalServerError, MediaTypeNames.Application.Json, typeof(ErrorResult))
        {
            Summary = "Internal Server Error.";
            Description = "Something went wrong while processing the request.";
        }
    }
}