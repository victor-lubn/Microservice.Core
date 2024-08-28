using Lueben.Microservice.OpenApi.Tests.v1;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Filters;
using Microsoft.OpenApi;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi;

namespace Lueben.Microservice.OpenApi.Tests
{
    public static class Helpers
    {
        public static IDocument InitDocument(QueryCollection queryCollection)
        {
            var request = new Mock<IHttpRequestDataObject>();
            request.Setup(x => x.Query).Returns(queryCollection);
            var doc = new Document(new DocumentHelper(new RouteConstraintFilter(), new OpenApiSchemaAcceptor()));
            doc.InitialiseDocument();
            doc.AddServer(request.Object, "");
            return doc;
        }

        public static async Task<JObject> GetFilteredDocument(this IDocument document, DocumentFilterCollection filters)
        {
            var assembly = Assembly.GetAssembly(typeof(GetOperation1HttpTrigger));
            document.Build(assembly);
            var filteredDocument = document.ApplyDocumentFilters(filters);
            var openApiJson = await filteredDocument.RenderAsync(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
            var openApiSpecification = JObject.Parse(openApiJson);
            return openApiSpecification;
        }
    }
}
