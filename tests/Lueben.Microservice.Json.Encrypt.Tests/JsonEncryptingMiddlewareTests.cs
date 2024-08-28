using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lueben.Microservice.Json.Encrypt.Configurations;
using Lueben.Microservice.Json.Encrypt.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Lueben.Microservice.Json.Encrypt.Tests
{
    public class JsonEncryptingMiddlewareTests
    {
        private readonly Mock<FunctionContext> _mockContext;
        private readonly Mock<FunctionExecutionDelegate> _mockNext;

        public JsonEncryptingMiddlewareTests()
        {
            _mockContext = new Mock<FunctionContext>();
            _mockNext = new Mock<FunctionExecutionDelegate>();
        }

        [Fact]
        public async Task GivenJsonEncryptingMiddleware_WhenRequiredHeaderNotExists_ThenLocaleCodeHeaderNotFoundExceptionIsThrown()
        {
            var middleware = new JsonEncryptingMiddleware(Options.Create(new EncryptionOptions
            {
                Secret = "63771811611376451693633762723226"
            }));

            await middleware.Invoke(_mockContext.Object, _mockNext.Object);

            var defaultSettings = JsonConvert.DefaultSettings.Invoke();
            Assert.IsType<EncryptedStringPropertyResolver>(defaultSettings.ContractResolver);

            _mockNext.Verify(n => n.Invoke(It.IsAny<FunctionContext>()), Times.Once);
        }
    }
}
