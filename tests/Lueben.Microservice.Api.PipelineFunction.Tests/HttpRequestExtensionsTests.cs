using System.Collections.Generic;
using System.Linq;
using Lueben.Microservice.Api.PipelineFunction.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Lueben.Microservice.Api.PipelineFunction.Tests
{
    public class HttpRequestExtensionsTests
    {
        private Mock<HttpRequest> _mockRequest;

        public HttpRequestExtensionsTests()
        {
            _mockRequest = new Mock<HttpRequest>();
        }

        [Fact]
        public void GivenGetHeaderValueOrDefault_WhenIsCalled_ThenReturnHeaderValue()
        {
            var headerName = "test";
            var expectedHeaderValue = "test value";
            var headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                {headerName, expectedHeaderValue}
            });

            _mockRequest.Setup(x => x.Headers).Returns(headers);

            var actualHeaderValue = _mockRequest.Object.GetHeaderValueOrDefault(headerName);

            Assert.Equal(expectedHeaderValue, actualHeaderValue);
        }


        [Fact]
        public void GivenGetHeaderValueOrDefault_WhenIsCalledAndNoHeader_ThenNullIsReturned()
        {
            var headers = new Mock<IHeaderDictionary>();
            _mockRequest.Setup(x => x.Headers).Returns(headers.Object);

            var actualHeaderValue = _mockRequest.Object.GetHeaderValueOrDefault("test");

            Assert.Null(actualHeaderValue);
        }
    }
}
