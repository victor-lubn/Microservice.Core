using AutoFixture;
using AutoFixture.AutoMoq;
using Castle.DynamicProxy;
using Lueben.Microservice.CircuitBreaker;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using IInvocation = Castle.DynamicProxy.IInvocation;

namespace Lueben.Microservice.Interceptors.Tests
{
    public class InterceptorsTests
    {
        private readonly IFixture _fixture;
        private const string CbId = "cbId";

        public InterceptorsTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
        }

        [Fact]
        public async void GivenClientWithCB_WhenCBIsClosed_ThenClientIsExecutedWithRetryInterceptor()
        {
            const int retMock = 1;
            var durableCircuitBreakerClientMock = _fixture.Freeze<Mock<IDurableCircuitBreakerClient>>();
            durableCircuitBreakerClientMock.Setup(x => 
                x.IsExecutionPermitted(CbId, It.IsAny<ILogger>(), It.IsAny<IDurableClient>(), It.IsAny<IConfiguration>()))
                .Returns(Task.FromResult(true));

            var clientMock = new Mock<ITestClient>();
            clientMock.Setup(x => x.TestMethod()).Returns(Task.FromResult(retMock));
            var retryInterceptor = new Mock<IAsyncInterceptor>();
            retryInterceptor.Setup(x => x.InterceptAsynchronous<int>(It.IsAny<IInvocation>())).Callback((IInvocation inv) => inv.Proceed()).Verifiable();

            var circuitBreakerClient = _fixture.Create<CircuitBreakerClient>();
            var clientWithRetry = clientMock.Object.AddInterceptor(retryInterceptor.Object);
            var clientWithCb = clientWithRetry.AddCircuitBreaker(circuitBreakerClient, CbId);

            var ret = await clientWithCb.TestMethod();

            Assert.Equal(retMock, ret);

            durableCircuitBreakerClientMock.Verify(x => x.IsExecutionPermitted(CbId, It.IsAny<ILogger>(), It.IsAny<IDurableClient>(), It.IsAny<IConfiguration>()), Times.Once);
            clientMock.Verify(x => x.TestMethod(), Times.Once);
            retryInterceptor.Verify(x => x.InterceptAsynchronous<int>(It.IsAny<IInvocation>()), Times.Once);
        }

        [Fact]
        public async void GivenClientWithCB_WhenCBIsOpen_ThenClientIsNotExecuted()
        {
            var durableCircuitBreakerClientMock = _fixture.Freeze<Mock<IDurableCircuitBreakerClient>>();
            durableCircuitBreakerClientMock.Setup(x =>
                    x.IsExecutionPermitted(CbId, It.IsAny<ILogger>(), It.IsAny<IDurableClient>(), It.IsAny<IConfiguration>()))
                .Returns(Task.FromResult(false));

            var clientMock = new Mock<ITestClient>();
            clientMock.Setup(x => x.TestMethod()).Returns(Task.FromResult(1));
            var retryInterceptor = new Mock<IAsyncInterceptor>();
            retryInterceptor.Setup(x => x.InterceptAsynchronous<int>(It.IsAny<IInvocation>())).Callback((IInvocation inv) => inv.Proceed()).Verifiable();

            var circuitBreakerClient = _fixture.Create<CircuitBreakerClient>();
            var clientWithRetry = clientMock.Object.AddInterceptor(retryInterceptor.Object);
            var clientWithCb = clientWithRetry.AddCircuitBreaker(circuitBreakerClient, CbId);

            await Assert.ThrowsAsync<CircuitBreakerOpenStateException>(clientWithCb.TestMethod);

            durableCircuitBreakerClientMock.Verify(x => x.IsExecutionPermitted(CbId, It.IsAny<ILogger>(), It.IsAny<IDurableClient>(), It.IsAny<IConfiguration>()), Times.Once);
            retryInterceptor.Verify(x => x.InterceptAsynchronous<int>(It.IsAny<IInvocation>()), Times.Never);
            clientMock.Verify(x => x.TestMethod(), Times.Never);
        }
    }
}