using RestSharp;
using Xunit;

namespace Lueben.Microservice.RestSharpClient.Factory.Tests
{
    public class RestRequestExtensionsTests
    {
        [Fact]
        public void PopulateUrlSegmentParameters_NotNullArgument_NoException()
        {
            var url = "http://Lueben.com/api/resource/{id}";
            var request = new RestRequest(url);
            request.PopulateUrlSegmentParameters(url, new []{ "ID" });

            var restClient = new RestClient();
            var resultUri = restClient.BuildUri(request);

            Assert.Equal("http://Lueben.com/api/resource/ID", resultUri.ToString());
        }
    }
}
