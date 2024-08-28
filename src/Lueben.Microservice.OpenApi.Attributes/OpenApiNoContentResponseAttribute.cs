using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Lueben.Microservice.OpenApi.Attributes
{
    public sealed class OpenApiNoContentResponseAttribute : OpenApiResponseWithoutBodyAttribute
    {
        public OpenApiNoContentResponseAttribute() : this("No content response.", "This operation returns no content.")
        {
        }

        public OpenApiNoContentResponseAttribute(string summary, string description) : base(HttpStatusCode.NoContent)
        {
            Summary = summary;
            Description = description;
        }
    }
}