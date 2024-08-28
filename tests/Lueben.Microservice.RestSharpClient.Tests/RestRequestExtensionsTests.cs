using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;
using Xunit;

namespace Lueben.Microservice.RestSharpClient.Tests
{
    public class RestRequestExtensionsTests
    {
        private RestRequest _request;

        public RestRequestExtensionsTests()
        {
            _request = new RestRequest("http://test.com/order/{orderId}/orderLine/{orderLineId}");
        }

        [Fact]
        public void GivenPopulateQueryStringParameters_WhenQueryParametersParameterIsNull_ThenNoParametersAreAdded()
        {
            _request.PopulateQueryStringParameters(null);
            Assert.Empty(_request.Parameters.Where(p => p.Type == ParameterType.QueryString));
        }

        [Fact]
        public void GivenPopulateQueryStringParameters_WhenQueryParametersParameterIsAnEmptyObject_ThenNoParametersAreAdded()
        {
            _request.PopulateQueryStringParameters(new { });
            Assert.Empty(_request.Parameters.Where(p => p.Type == ParameterType.QueryString));
        }

        [Fact]
        public void GivenPopulateQueryStringParameters_ThenShouldAddQueryParametersToRequest()
        {
            _request.PopulateQueryStringParameters(new { id = "1", name = "john_doe" });

            Assert.Equal(2, _request.Parameters.Where(p => p.Type == ParameterType.QueryString).Count());
            Assert.Equal("1", _request.Parameters.Where(p => p.Type == ParameterType.QueryString).First(p => p.Name == "id").Value);
            Assert.Equal("john_doe", _request.Parameters.Where(p => p.Type == ParameterType.QueryString).First(p => p.Name == "name").Value);
        }

        [Fact]
        public void GivenPopulateUrlSegmentParameters_WhenUrlSegmentsCollectionIsNull_ThenShouldNotAddParametersToRequest()
        {
            _request.PopulateUrlSegmentParameters(_request.Resource, null);
            Assert.Empty(_request.Parameters.Where(p => p.Type == ParameterType.UrlSegment));
        }

        [Fact]
        public void GivenPopulateUrlSegmentParameters_WhenUrlSegmentsCollectionIsEmpty_ThenShouldNotAddParametersToRequest()
        {
            _request.PopulateUrlSegmentParameters(_request.Resource, new List<object>().AsReadOnly());
            Assert.Empty(_request.Parameters.Where(p => p.Type == ParameterType.UrlSegment));
        }

        [Fact]
        public void GivenPopulateUrlSegmentParameters_WhenParametersCountDontMatch_ThenShouldThrowException()
        {
            Assert.Throws<Exception>(() => _request.PopulateUrlSegmentParameters(_request.Resource, new List<object>() { new() }.AsReadOnly()));
        }

        [Fact]
        public void GivenPopulateUrlSegmentParameters_WhenValuesArePassed_ThenShouldAddUrlSegmentParameters()
        {
            var urlSegments = new List<object>()
            {
                "123",
                "456",
            };

            _request.PopulateUrlSegmentParameters(_request.Resource, urlSegments.AsReadOnly());
            Assert.Equal(2, _request.Parameters.Where(p => p.Type == ParameterType.UrlSegment).Count());

            Assert.Equal("123", _request.Parameters.Where(p => p.Type == ParameterType.UrlSegment && p.Name == "orderId").First().Value);
            Assert.Equal("456", _request.Parameters.Where(p => p.Type == ParameterType.UrlSegment && p.Name == "orderLineId").First().Value);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GivenPopulateBody_WhenBodyIsPassed_ThenShouldAddBodyAsExpected(bool isBodyNull)
        {
            var body = isBodyNull ? null : new { Prop = "Bar" };
            var expectedCount = isBodyNull ? 0 : 1;

            _request.PopulateBody(body);

            Assert.Equal(expectedCount, _request.Parameters.Where(p => p.Type == ParameterType.RequestBody).Count());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GivenAddHeaders_WhenHeadersCollectionIsNullOrEmpty_ThenShouldNotAttemptAdding(bool isHeaderCollectionNull)
        {
            var headers = isHeaderCollectionNull ? null : new Dictionary<string, string>();

            _request.PopulateHeaders(headers);
            Assert.Empty(_request.Parameters.Where(p => p.Type == ParameterType.HttpHeader));
        }

        [Fact]
        public void GivenAddHeaders_WhenHeadersArePassed_ThenShouldAddHeadersToRequest()
        {
            var headers = new Dictionary<string, string>()
            {
                ["foo"] = "bar",
            };
            _request.PopulateHeaders(headers);
            Assert.Single(_request.Parameters.Where(p => p.Type == ParameterType.HttpHeader));
        }
    }
}
