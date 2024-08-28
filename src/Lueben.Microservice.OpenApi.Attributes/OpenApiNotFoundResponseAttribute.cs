using System.Net;
using System.Net.Mime;
using Lueben.Microservice.Api.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Lueben.Microservice.OpenApi.Attributes
{
    public sealed class OpenApiNotFoundResponseAttribute : OpenApiResponseWithBodyAttribute
    {
        public OpenApiNotFoundResponseAttribute() : this("Entity not found.", "Entity was not found.")
        {
        }

        public OpenApiNotFoundResponseAttribute(string summary, string description) : base(HttpStatusCode.NotFound, MediaTypeNames.Application.Json, typeof(ErrorResult))
        {
            Summary = summary;
            Description = description;
        }
    }
}