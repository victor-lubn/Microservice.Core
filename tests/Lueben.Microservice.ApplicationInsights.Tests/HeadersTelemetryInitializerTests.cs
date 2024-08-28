using System.Collections.Generic;
using Lueben.ApplicationInsights.Headers;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using Xunit;

namespace Lueben.Microservice.ApplicationInsights.Tests
{
    public class HeadersTelemetryInitializerTests
    {
        private readonly Mock<IHttpContextAccessor> _contextAccessorMock;
        private readonly HeaderDictionary _requestHeaders = new HeaderDictionary();
        private readonly HeaderDictionary _responseHeaders = new HeaderDictionary();

        public HeadersTelemetryInitializerTests()
        {
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            IHttpRequestFeature requestFeature = new HttpRequestFeature
            {
                Headers = _requestHeaders
            };
            IHttpResponseFeature responseFeature = new HttpResponseFeature
            {
                Headers = _responseHeaders
            };
            var featureCollection = new FeatureCollection();
            featureCollection.Set(requestFeature);
            featureCollection.Set(responseFeature);
            _contextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext(featureCollection));
        }

        [Fact]
        public void GivenHeadersTelemetryInitializer_WhenRequestContainsLoggedHeader_ThenRequestHeaderTelemetryPropertyAdded()
        {
            const string mockHeader = "header";
            const string mockHeaderValue = "value";
            var trace = new RequestTelemetry();

            _requestHeaders.Add(mockHeader, mockHeaderValue);
            var initializer = new HeadersTelemetryInitializer(_contextAccessorMock.Object, new List<string> { mockHeader });

            initializer.Initialize(trace);

            Assert.Contains(trace.Properties, p => p.Key == $"Request-{mockHeader}" && p.Value == mockHeaderValue);
        }

        [Fact]
        public void GivenHeadersTelemetryInitializer_WhenResponseContainsLoggedHeader_ThenResponseHeaderTelemetryPropertyAdded()
        {
            const string mockHeader = "header";
            const string mockHeaderValue = "value";
            var trace = new RequestTelemetry();
            _responseHeaders.Add(mockHeader, mockHeaderValue);
            var initializer = new HeadersTelemetryInitializer(_contextAccessorMock.Object, new List<string> { mockHeader });

            initializer.Initialize(trace);

            Assert.Contains(trace.Properties, p => p.Key == $"Response-{mockHeader}" && p.Value == mockHeaderValue);
        }

        [Fact]
        public void GivenHeadersTelemetryInitializer_WhenRequestOrResponseContainsNotLoggedHeaders_ThenTelemetryPropertiesAreNotAdded()
        {
            const string mockHeader = "header";
            const string mockHeaderValue = "value";
            var trace = new RequestTelemetry();
            _responseHeaders.Add("notLoggedHeader", mockHeaderValue);
            _requestHeaders.Add("notLoggedHeader", mockHeaderValue);
            var initializer = new HeadersTelemetryInitializer(_contextAccessorMock.Object, new List<string> { mockHeader });

            initializer.Initialize(trace);

            Assert.Empty(trace.Properties);
        }
    }
}