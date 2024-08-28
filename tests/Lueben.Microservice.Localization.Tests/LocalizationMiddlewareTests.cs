using System.Collections.Generic;
using System.Threading.Tasks;
using Lueben.Microservice.Localization.Configurations;
using Lueben.Microservice.Localization.Constants;
using Lueben.Microservice.Localization.Exceptions;
using Lueben.Microservice.Localization.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Lueben.Microservice.Localization.Tests
{
    public class LocalizationMiddlewareTests
    {
        private readonly Mock<FunctionContext> _mockContext;
        private readonly Mock<FunctionExecutionDelegate> _mockNext;
        public LocalizationMiddlewareTests()
        {
            _mockContext = new Mock<FunctionContext>();
            _mockNext = new Mock<FunctionExecutionDelegate>();
        }

        [Fact]
        public async Task GivenLocalizationMiddleware_WhenRequiredHeaderNotExists_ThenLocaleCodeHeaderNotFoundExceptionIsThrown()
        {
            var options = Options.Create(new LocalizationOptions
            {
                AllowedLocales = "en-GB,en-IE,fr-FR,fr-BE"
            });

            LocalizationOptions.Value = options.Value;
            var context = new DefaultHttpContext();
            _mockContext.Setup(x => x.Items).Returns(new Dictionary<object, object>
            {
                { "HttpRequestContext", context }
            });

            var middleware = new LocalizationMiddleware();

            await Assert.ThrowsAsync<LocaleCodeHeaderNotFoundException>(() => middleware.Invoke(_mockContext.Object, _mockNext.Object));
        }

        [Fact]
        public async Task GivenLocalizationMiddleware_WhenRequiredHeaderEmpty_ThenLocaleCodeHeaderNotFoundExceptionIsThrown()
        {
            var options = Options.Create(new LocalizationOptions
            {
                AllowedLocales = "en-GB,en-IE,fr-FR,fr-BE"
            });

            LocalizationOptions.Value = options.Value;

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(new(CommonConstants.LocaleCodeHeaderKey, string.Empty));
            _mockContext.Setup(x => x.Items).Returns(new Dictionary<object, object>
            {
                { "HttpRequestContext", context }
            });

            var middleware = new LocalizationMiddleware();

            await Assert.ThrowsAsync<LocaleCodeHeaderNotFoundException>(() => middleware.Invoke(_mockContext.Object, _mockNext.Object));
        }

        [Fact]
        public async Task GivenLocalizationMiddleware_WhenRequiredHeaderNotExistsAndDefaultLocaleCodeExists_ThenTaskCompleted()
        {
            var options = Options.Create(new LocalizationOptions
            {
                AllowedLocales = "en-GB,en-IE,fr-FR,fr-BE",
                DefaultLocale = "en-GB"
            });

            LocalizationOptions.Value = options.Value;

            var context = new DefaultHttpContext();
            _mockContext.Setup(x => x.Items).Returns(new Dictionary<object, object>
            {
                { "HttpRequestContext", context }
            });

            var middleware = new LocalizationMiddleware();

            await middleware.Invoke(_mockContext.Object, _mockNext.Object);

            _mockNext.Verify(n => n.Invoke(It.IsAny<FunctionContext>()), Times.Once);
        }

        [Theory]
        [InlineData("en", "en-GB,en-IE,fr-FR,fr-BE")]
        [InlineData("fr-BE", "en-GB,fr-FR")]
        public async Task GivenLocalizationMiddleware_WhenRequiredHeaderInvalid_ThenLocaleCodeNotSupportedExceptionIsThrown(string header, string allowedLocales)
        {
            var options = Options.Create(new LocalizationOptions
            {
                AllowedLocales = allowedLocales,
                DefaultLocale = "en-GB"
            });

            LocalizationOptions.Value = options.Value;

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(new(CommonConstants.LocaleCodeHeaderKey, header));
            _mockContext.Setup(x => x.Items).Returns(new Dictionary<object, object>
            {
                { "HttpRequestContext", context }
            });

            var middleware = new LocalizationMiddleware();

            await Assert.ThrowsAsync<LocaleCodeNotSupportedException>(() => middleware.Invoke(_mockContext.Object, _mockNext.Object));

            _mockNext.Verify(n => n.Invoke(It.IsAny<FunctionContext>()), Times.Never);
        }

        [Theory]
        [InlineData("en-GB", "en-GB,en-IE,fr-FR,fr-BE")]
        [InlineData("fr-be", "en-GB,en-IE,fr-FR,fr-BE")]
        [InlineData("en-IE", "en-GB, en-IE, fr-FR, fr-BE")]
        public async Task GivenLocalizationMiddleware_WhenRequiredHeaderValid_ThenNextDelegateIsExecuted(string header, string allowedLocales)
        {
            var options = Options.Create(new LocalizationOptions
            {
                AllowedLocales = allowedLocales
            });
            LocalizationOptions.Value = options.Value;

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(new(CommonConstants.LocaleCodeHeaderKey, header));
            _mockContext.Setup(x => x.Items).Returns(new Dictionary<object, object>
            {
                { "HttpRequestContext", context }
            });

            var middleware = new LocalizationMiddleware();
            await middleware.Invoke(_mockContext.Object, _mockNext.Object);

            _mockNext.Verify(n => n.Invoke(It.IsAny<FunctionContext>()), Times.Once);
        }
    }
}
