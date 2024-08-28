using System.Net;
using System.Net.Mime;
using Lueben.Microservice.Api.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Lueben.Microservice.OpenApi.Attributes
{
    public sealed class OpenApiBadRequestResponseAttribute : OpenApiResponseWithBodyAttribute
    {
        public OpenApiBadRequestResponseAttribute() : this("Invalid request payload.", "Request payload is not valid.")
        {
        }

        public OpenApiBadRequestResponseAttribute(string summary, string description) : base(HttpStatusCode.BadRequest, MediaTypeNames.Application.Json, typeof(ErrorResult))
        {
            Summary = summary;
            Description = description;
        }
    }
}