using System;
using System.Linq;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.OpenApi.Models;

namespace Lueben.Microservice.OpenApi.DocumentFilters
{
    public class EndpointsByOperationIdsDocumentFilter : IDocumentFilter
    {
        public const string OperationsQueryParameterName = "operations";

        public const string ExcludeOperationsQueryParameterName = "excludeoperations";

        public void Apply(IHttpRequestDataObject req, OpenApiDocument document)
        {
            var operations = req.Query[OperationsQueryParameterName];
            var excludeOperations = req.Query[ExcludeOperationsQueryParameterName];

            if (string.IsNullOrEmpty(operations) && string.IsNullOrEmpty(excludeOperations))
            {
                return;
            }

            var operationsList = operations.ToArray();
            var excludeOperationsList = excludeOperations.ToArray();

            if (operationsList.Length > 0 && excludeOperationsList.Length > 0)
            {
                throw new Exception("Include and Exclude operations query parameters cannot be used together.");
            }

            foreach (var path in document.Paths)
            {
                foreach (var operation in path.Value.Operations)
                {
                    var operationId = operation.Value.OperationId;

                    if (operationsList.Length > 0 && !operationsList.Contains(operationId))
                    {
                        path.Value.Operations.Remove(operation.Key);
                    }

                    if (excludeOperationsList.Length > 0 && excludeOperationsList.Contains(operationId))
                    {
                        path.Value.Operations.Remove(operation.Key);
                    }
                }

                if (path.Value.Operations.Count == 0)
                {
                    document.Paths.Remove(path.Key);
                }
            }
        }
    }
}
