using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.OpenApi.Models;

namespace Lueben.Microservice.OpenApi.DocumentFilters
{
    public class CommonApiParametersFilter : IDocumentFilter
    {
        private IList<OpenApiParameter> _commonApiParameters;

        public CommonApiParametersFilter(IList<OpenApiParameter> commonApiParameters)
        {
            _commonApiParameters = commonApiParameters;
        }

        public void Apply(IHttpRequestDataObject req, OpenApiDocument document)
        {
            foreach (var path in document.Paths.Values)
            {
                foreach (var operation in path.Operations.Values)
                {
                    AddCommonHeaders(operation.Parameters);
                }
            }
        }

        private void AddCommonHeaders(IList<OpenApiParameter> parameters)
        {
            if (_commonApiParameters is null)
            {
                return;
            }

            foreach (var apiParameter in _commonApiParameters)
            {
                parameters.Add(apiParameter);
            }
        }
    }
}
