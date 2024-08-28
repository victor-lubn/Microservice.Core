using Lueben.Microservice.OpenApi.DocumentFilters;
using Lueben.Microservice.OpenApi.Extensions;
using Lueben.Microservice.OpenApi.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Filters;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Lueben.Microservice.OpenApi.Tests
{

    public class CommonApiParametersFilterTests
    {
        [Fact]
        public async Task GivenOpenApiDocument_WhenCommonApiParameterAdded_ThenItExistInAllMethods()
        {
            const string identityReferenceHeader = "Lueben-Request-Identity";
            const string sourceConsumerHeader = "Lueben-Request-Consumer";

            var options = new LuebenOpenApiHttpTiggerContextOptions();
            options.AddCommonHeader(identityReferenceHeader, "Api consumer identity name");
            options.AddCommonHeader(sourceConsumerHeader, "Api consumer application name", true);

            var filter = new CommonApiParametersFilter(options.CommonOpenApiParameters);
            var filters = new DocumentFilterCollection(new List<IDocumentFilter> { filter });

            var doc = Helpers.InitDocument(new QueryCollection());
            var filteredDocument = await doc.GetFilteredDocument(filters);

            var paths = filteredDocument["paths"] as JObject;
            Assert.NotNull(paths);

            foreach (var path in paths)
            {
                var pathOperations = path.Value as JObject;
                Assert.NotNull(pathOperations);
                foreach (var op in pathOperations)
                {
                    var opProps = op.Value as JObject;
                    Assert.NotNull(opProps);
                    var h1 = opProps["parameters"]?.First(p => p["name"]?.ToString() == identityReferenceHeader);
                    Assert.NotNull(h1);
                    var h2 = opProps["parameters"]?.First(p => p["name"]?.ToString() == sourceConsumerHeader);
                    Assert.NotNull(h2);
                }
            }
        }
    }
}