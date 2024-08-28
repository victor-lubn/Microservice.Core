using System.Collections.Generic;
using System.Linq;
using System.Net;
using Lueben.Microservice.OpenApi.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi.DocumentFilters
{
    public class CommonApiResponsesFilter : IDocumentFilter
    {
        private static readonly List<OperationType> NoContentOperations = new List<OperationType>
        {
            OperationType.Put,
            OperationType.Patch,
            OperationType.Delete,
        };

        private readonly NamingStrategy namingStrategy;

        public CommonApiResponsesFilter(NamingStrategy namingStrategy)
        {
            this.namingStrategy = namingStrategy;
        }

        public void Apply(IHttpRequestDataObject req, OpenApiDocument document)
        {
            foreach (var path in document.Paths.Values)
            {
                foreach (var (operationType, value) in path.Operations)
                {
                    AddCommonResponses(operationType, value.Responses);
                }
            }
        }

        protected virtual void AddCommonResponses(OperationType operationType, OpenApiResponses responses)
        {
            var commonResponseWithBodyAttributes = new List<OpenApiResponseWithBodyAttribute> { new OpenApiInternalServerErrorResponseAttribute() };

            if (!responses.ContainsKey(((int)HttpStatusCode.BadRequest).ToString()))
            {
                commonResponseWithBodyAttributes.Add(new OpenApiBadRequestResponseAttribute());
            }

            var commonResponseWithoutBodyAttributes = new List<OpenApiResponseWithoutBodyAttribute>();
            if (NoContentOperations.Contains(operationType))
            {
                commonResponseWithoutBodyAttributes.Add(new OpenApiNoContentResponseAttribute());
            }

            var commonResponsesWithoutBody = commonResponseWithoutBodyAttributes.Select(p => new
            {
                StatusCode = p.StatusCode,
                Response = p.ToOpenApiResponse(namingStrategy)
            });

            var commonResponsesWithBody = commonResponseWithBodyAttributes.Select(p => new
            {
                p.StatusCode,
                Response = p.ToOpenApiResponse(namingStrategy)
            });

            var additionalResponses = commonResponsesWithBody.Concat(commonResponsesWithoutBody)
                .ToDictionary(p => ((int)p.StatusCode).ToString(), p => p.Response)
                .ToOpenApiResponses();

            responses.AddRange(additionalResponses);
        }
    }
}
