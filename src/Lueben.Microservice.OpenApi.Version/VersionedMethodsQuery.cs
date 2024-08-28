using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lueben.Microservice.OpenApi.Version.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;

namespace Lueben.Microservice.OpenApi.Version
{
    public class VersionedMethodsQuery : IVersionedMethodsQuery
    {
        private readonly IOpenApiOperationFilter _openApiOperationFilter;
        private readonly IDocumentHelper _documentHelper;

        public VersionedMethodsQuery(IHttpContextAccessor httpContextAccessor, IDocumentHelper documentHelper)
        {
            var req = httpContextAccessor.HttpContext.Request;
            string apiVersion = req.Query["apiversion"];
            if (!string.IsNullOrEmpty(apiVersion))
            {
                _openApiOperationFilter = new OpenApiVersionOperationFilter(apiVersion);
            }

            _documentHelper = documentHelper;
        }

        public virtual List<MethodInfo> Get(Assembly assembly)
        {
            var methods = _documentHelper.GetHttpTriggerMethods(assembly, Enumerable.Empty<string>());
            if (_openApiOperationFilter != null)
            {
                methods = _openApiOperationFilter.Filter(methods).ToList();
            }

            return methods;
        }
    }
}
