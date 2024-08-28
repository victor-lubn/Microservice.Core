using System;
using System.Linq;
using System.Net;
using System.Reflection;
using AutoFixture;
using AutoFixture.AutoMoq;
using Castle.DynamicProxy;
using Lueben.Microservice.Interceptors;
using Lueben.Microservice.RestSharpClient.Authentication;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Lueben.Microservice.RestSharpClient.Factory.Tests
{
    public class MicroservicesRestSharpClientFactoryTests
    {
        private class TestClient
        {
        }

        private readonly IFixture _fixture;

        public MicroservicesRestSharpClientFactoryTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
        }

        [Fact]
        public void GivenMicroservicesRestSharpClientFactory_WhenClientUseFunctionKey_ThenRestSharpClientWillUseFunctionKeyAuthenticator()
        {
            const string functionKey = "functionKey";
            var options = _fixture.Build<RestSharpClientOptions>()
                .OmitAutoProperties()
                .With(o => o.FunctionKey, functionKey)
                .Create();

            var optionsMock = _fixture.Freeze<Mock<IOptionsSnapshot<RestSharpClientOptions>>>();
            optionsMock.Setup(x => x.Get(nameof(TestClient)))
                .Returns(options);

            var factory = _fixture.Create<MicroserviceRestSharpClientFactory>();

            var client = factory.Create(new TestClient()) as RestSharpClient;

            Assert.NotNull(client);
            Assert.IsType<FunctionKeyAuthenticator>(client.Authenticator);
        }

        [Fact]
        public void GivenMicroservicesRestSharpClientFactory_WhenClientUseApim_ThenRestSharpClientWillOAuthToken()
        {
            const string scope = "scope";
            var options = _fixture.Build<RestSharpClientOptions>()
                .OmitAutoProperties()
                .With(o => o.Scope, scope)
                .Create();

            var optionsMock = _fixture.Freeze<Mock<IOptionsSnapshot<RestSharpClientOptions>>>();
            optionsMock.Setup(x => x.Get(nameof(TestClient)))
                .Returns(options);

            var factory = _fixture.Create<MicroserviceRestSharpClientFactory>();

            var client = factory.Create(new TestClient()) as RestSharpClient;

            Assert.NotNull(client);
            Assert.IsType<ClientCredentialsAuthenticator>(client.Authenticator);
        }

        [Fact]
        public void GivenMicroservicesRestSharpClientFactory_WhenCreatingClientBySectionName_ThenShouldCreateRestClient()
        {
            var options = _fixture.Create<RestSharpClientOptions>();
            var optionsMock = _fixture.Freeze<Mock<IOptionsSnapshot<RestSharpClientOptions>>>();
            optionsMock.Setup(x => x.Get(nameof(TestClient)))
                .Returns(options);

            var factory = _fixture.Create<MicroserviceRestSharpClientFactory>();

            var client = factory.Create(nameof(TestClient));
            Assert.NotNull(client);
        }

        [Fact]
        public void GivenMicroservicesRestSharpClientFactory_WhenOptionsAreNotSetUp_ThenShouldThrowException()
        {
            var optionsMock = _fixture.Freeze<Mock<IOptionsSnapshot<RestSharpClientOptions>>>();
            optionsMock.Setup(x => x.Get(nameof(TestClient)))
                .Returns(default(RestSharpClientOptions));

            var factory = _fixture.Create<MicroserviceRestSharpClientFactory>();

            Assert.Throws<Exception>(() => factory.Create(nameof(TestClient)));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GivenMicroservicesRestSharpClientFactory_WhenCircuitBreakerIdIsNullOrEmpty_ThenOnlyRetryPolicyInterceptorIsAdded(string circuitBreakerId)
        {
            var options = _fixture.Build<RestSharpClientOptions>()
                .OmitAutoProperties()
                .With(x => x.EnableRetry, true)
                .With(x => x.CircuitBreakerId, circuitBreakerId)
                .Create();

            var optionsMock = _fixture.Freeze<Mock<IOptionsSnapshot<RestSharpClientOptions>>>();
            optionsMock.Setup(x => x.Get(nameof(TestClient)))
                .Returns(options);

            var factory = _fixture.Create<MicroserviceRestSharpClientFactory>();

            var client = factory.Create(nameof(TestClient));
            Assert.Contains("IRestSharpClientProxy", client.GetType().Name);

            var interceptorsField = client.GetType().GetField("__interceptors", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(interceptorsField);

            var interceptors = ((object[])interceptorsField.GetValue(client)).Select(x => x as AsyncDeterminationInterceptor).ToList();
            Assert.Single(interceptors);
            Assert.IsType<RetryPolicyInterceptor<RestClientApiException>>(interceptors.First().AsyncInterceptor);
        }

        [Fact]
        public void GivenMicroservicesRestSharpClientFactory_WhenEnableRetryIsFalse_ThenOnlyCircuitBreakerInterceptorIsAdded()
        {
            var options = _fixture.Build<RestSharpClientOptions>()
                .OmitAutoProperties()
                .With(x => x.EnableRetry, false)
                .With(x => x.CircuitBreakerId, "foo")
                .Create();

            var optionsMock = _fixture.Freeze<Mock<IOptionsSnapshot<RestSharpClientOptions>>>();
            optionsMock.Setup(x => x.Get(nameof(TestClient)))
                .Returns(options);

            var factory = _fixture.Create<MicroserviceRestSharpClientFactory>();

            var client = factory.Create(nameof(TestClient));
            Assert.Contains("IRestSharpClientProxy", client.GetType().Name);

            var interceptorsField = client.GetType().GetField("__interceptors", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(interceptorsField);

            var interceptors = ((object[])interceptorsField.GetValue(client)).Select(x => x as AsyncDeterminationInterceptor).ToList();
            Assert.Single(interceptors);
            Assert.IsType<CircuitBreakerInterceptor>(interceptors.First().AsyncInterceptor);
        }

        [Theory]
        [InlineData(HttpStatusCode.InternalServerError, true)]
        [InlineData(HttpStatusCode.BadGateway, true)]
        [InlineData(HttpStatusCode.TooManyRequests, true)]
        [InlineData(HttpStatusCode.RequestTimeout, true)]
        [InlineData(HttpStatusCode.BadRequest, false)]
        public void Test(HttpStatusCode statusCode, bool expectedValue)
        {
            var methodInfo = typeof(MicroserviceRestSharpClientFactory).GetMethod("CheckStatusCode", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(methodInfo);

            Assert.Equal(expectedValue, (bool)methodInfo.Invoke(null, new object[] { statusCode }));
        }
    }
}