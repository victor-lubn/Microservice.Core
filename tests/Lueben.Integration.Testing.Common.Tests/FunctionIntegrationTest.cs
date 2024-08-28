using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Xunit;
using Xunit.Abstractions;

namespace Lueben.Integration.Testing.Common.Tests
{
    public class Function
    {
    }

    public class Model
    {
    }

    public class HttpIntegrationFunctionTest : HttpFunctionIntegrationTest<Function>
    {
        public HttpIntegrationFunctionTest(
            LuebenWireMockClassFixture wireMockClassFixture,
            ITestOutputHelper testOutputHelper,
            string consumer) : base(wireMockClassFixture, testOutputHelper, consumer)
        {
        }

        protected override void SetupServices()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class FunctionIntegrationTest : HttpFunctionIntegrationTest<Function>
    {
        public FunctionIntegrationTest(LuebenWireMockClassFixture LuebenWireMockClassFixture, ITestOutputHelper testOutputHelper)
            : base(LuebenWireMockClassFixture, testOutputHelper, "Foo")
        {
        }

        [Fact]
        public void GivenBaseClass_ShouldSetupInfrastructure()
        {
            Assert.NotNull(this.WireMockServer);
            Assert.True(this.WireMockServer.IsStarted);
            Assert.Empty(this.WireMockServer.Mappings);
            Assert.Empty(this.WireMockServer.LogEntries);

            Assert.Equal("Foo", this.WireMockServer.Consumer);

            Assert.NotNull(this.Fixture);
            Assert.NotNull(this.Function);
            Assert.IsType<Function>(this.Function);
        }

        [Fact]
        public void GivenBaseClass_ShouldCreateRequest()
        {
            var queryParameters = new Dictionary<string, string>()
            {
                ["code"] = "1234",
                ["hello"] = "world",
            };

            var headers = new Dictionary<string, string>()
            {
                ["Authorization"] = "Basic FooBar",
            };

            var request = CreateHttpRequest<Model>(queryParameters, headers);
            Assert.NotNull(request);

            Assert.Equal("1234", request.Query["code"]);
            Assert.Equal("world", request.Query["hello"]);

            Assert.Equal("Basic FooBar", request.Headers["Authorization"]);

            Assert.NotNull(request.Body);
        }

        [Fact]
        public void GivenBaseClass_WhenCreatingRequest_ThenShouldSetupHttpContextAccessorMock()
        {
            var request = CreateHttpRequest<Model>();
            Assert.NotNull(request);

            var context = this.HttpContextAccessorMock.Object.HttpContext;
            Assert.IsType<DefaultHttpContext>(context);
            Assert.Same(request, context.Request);
        }
    }
}
