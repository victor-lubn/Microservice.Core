using System.Net;
using Lueben.Microservice.Localization.Constants;
using Lueben.Microservice.Localization.Exceptions;
using Lueben.Microservice.Localization.Middleware;
using Xunit;

namespace Lueben.Microservice.Localization.Tests
{
    public class LocaleCodeNotSupportedMiddlewareTests
    {
        [Fact]
        public void GivenGetErrorResult_WhenLocaleCodeNotSupportedException_ThenExpectedResultIsReturned()
        {
            var middleware = new LocaleCodeNotSupportedMiddleware();

            var exception = new LocaleCodeNotSupportedException("test message");

            var errorResult = middleware.GetErrorResult(exception);

            Assert.Equal(HttpStatusCode.NotAcceptable, errorResult.StatusCode);
            Assert.Equal(ErrorNames.NotAcceptable, errorResult.Name);
        }

        [Fact]
        public void GivenGetErrorResult_WhenLocaleCodeHeaderNotFoundException_ThenExpectedResultIsReturned()
        {
            var middleware = new LocaleCodeNotSupportedMiddleware();

            var exception = new LocaleCodeHeaderNotFoundException();

            var errorResult = middleware.GetErrorResult(exception);

            Assert.Equal(HttpStatusCode.BadRequest, errorResult.StatusCode);
            Assert.Equal(ErrorNames.BadRequest, errorResult.Name);
        }
    }
}
