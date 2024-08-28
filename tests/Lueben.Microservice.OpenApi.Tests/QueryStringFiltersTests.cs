using Lueben.Microservice.OpenApi.DocumentFilters;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Filters;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Lueben.Microservice.OpenApi.Tests
{
    public class QueryStringFiltersTests
    {
        private readonly DocumentFilterCollection _filters;

        public QueryStringFiltersTests()
        {
            var documentFilter = new EndpointsByOperationIdsDocumentFilter();
            var versionFilter = new EndpointsByApiVersionDocumentFilter();
            _filters = new DocumentFilterCollection(new List<IDocumentFilter> { documentFilter, versionFilter });
        }

        [Theory]
        [InlineData("v1", null, 2, 1)]
        [InlineData("v1", new[] { "GetOperation1" }, 1, 0)]
        [InlineData("v1", new[] { "GetOperation2" }, 0, 1)]
        [InlineData("v1", new[] { "NotExistingOperation" }, 0, 0)]
        [InlineData("v1", new[] { "GetOperation1", "DeleteOperation1" }, 2, 0)]
        [InlineData("v1", new[] { "GetOperation1", "GetOperation2" }, 1, 1)]
        [InlineData("v2", null, 1, 1)]
        [InlineData("v2", new[] { "GetOperation1" }, 1, 0)]
        [InlineData("v2", new[] { "DeleteOperation1" }, 0, 0)]
        [InlineData("v2", new[] { "GetOperation1", "DeleteOperation1" }, 1, 0)]
        [InlineData("v2", new[] { "GetOperation1", "GetOperation2" }, 1, 1)]
        [InlineData("v2", new[] { "GetOperation1", "GetOperation1" }, 1, 0)]
        public async Task GivenFiltersAdded_WhenApplied_ThenGetExpectedListOfOperations(string version, string[] operations, int pathOperation1Count, int pathOperation2Count)
        {
            var queryParameters = new Dictionary<string, StringValues>
            {
                { EndpointsByApiVersionDocumentFilter.ApiVersionQueryParameterName, new StringValues(version) }
            };

            if (operations?.Length > 0)
            {
                queryParameters.Add(EndpointsByOperationIdsDocumentFilter.OperationsQueryParameterName, new StringValues(operations));
            }

            var openApiSpecification = await ApplyQueryStringFilters(queryParameters);

            Assert.Equal(pathOperation1Count, openApiSpecification["paths"]?[$"/{version}/operation1"]?.Count() ?? 0);
            Assert.Equal(pathOperation2Count, openApiSpecification["paths"]?[$"/{version}/operation2"]?.Count() ?? 0);
        }

        [Fact]
        public async Task GivenFiltersAdded_WhenApplied_ThenGetExpectedOperationsInPaths()
        {
            const string operation = "GetOperation1";
            var queryParameters = new Dictionary<string, StringValues>
            {
                { EndpointsByApiVersionDocumentFilter.ApiVersionQueryParameterName, new StringValues("v1") },
                { EndpointsByOperationIdsDocumentFilter.OperationsQueryParameterName, new StringValues(operation) },
            };

            var openApiSpecification = await ApplyQueryStringFilters(queryParameters);

            Assert.Equal(1, openApiSpecification["paths"]?.Count());
            Assert.Equal(operation, openApiSpecification["paths"]?["/v1/operation1"]?["get"]?["operationId"]?.ToString());
        }

        [Fact]
        public async Task GivenFiltersAdded_WhenExcludeOperationsApplied_ThenGetExpectedOperationsInPaths()
        {
            const string operation1 = "GetOperation1";
            const string operation2 = "GetOperation2";
            var queryParameters = new Dictionary<string, StringValues>
            {
                { EndpointsByApiVersionDocumentFilter.ApiVersionQueryParameterName, new StringValues("v1") },
                { EndpointsByOperationIdsDocumentFilter.ExcludeOperationsQueryParameterName, new StringValues(new [] {operation1, operation2}) }
            };

            var openApiSpecification = await ApplyQueryStringFilters(queryParameters);

            Assert.Equal(1, openApiSpecification["paths"]?.Count());
            Assert.NotNull(openApiSpecification["paths"]?["/v1/operation1"]?["delete"]);
            Assert.Null(openApiSpecification["paths"]?["/v1/operation1"]?["get"]);
            Assert.Null(openApiSpecification["paths"]?["/v1/operation2"]?["get"]);
        }

        [Fact]
        public async Task GivenFiltersAdded_WhenExcludeAndIncludeOperationsApplied_ThenExceptionIsRaised()
        {
            const string operation = "GetOperation1";
            var queryParameters = new Dictionary<string, StringValues>
            {
                { EndpointsByApiVersionDocumentFilter.ApiVersionQueryParameterName, new StringValues("v1") },
                { EndpointsByOperationIdsDocumentFilter.ExcludeOperationsQueryParameterName, new StringValues(operation) },
                { EndpointsByOperationIdsDocumentFilter.OperationsQueryParameterName, new StringValues(operation) }
            };

            await Assert.ThrowsAsync<Exception>(async () => await ApplyQueryStringFilters(queryParameters));
        }

        private async Task<JObject> ApplyQueryStringFilters(Dictionary<string, StringValues> dictionary)
        {
            var doc = Helpers.InitDocument(new QueryCollection(dictionary));
            return await doc.GetFilteredDocument(_filters);
        }
    }
}