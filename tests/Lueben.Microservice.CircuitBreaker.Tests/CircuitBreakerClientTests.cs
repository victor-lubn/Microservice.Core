using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Lueben.Microservice.CircuitBreaker.Tests.Extensions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lueben.Microservice.CircuitBreaker.Tests
{
    public class CircuitBreakerClientTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IDurableCircuitBreakerClient> _durableCircuitBreakerClientMock;
        private readonly Mock<ILogger<CircuitBreakerClient>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;

        public CircuitBreakerClientTests()
        {
            _fixture = new Fixture()
                .Customize(new AutoMoqCustomization { ConfigureMembers = true });

            _durableCircuitBreakerClientMock = _fixture.Freeze<Mock<IDurableCircuitBreakerClient>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<CircuitBreakerClient>>>();
            _configurationMock = _fixture.Freeze<Mock<IConfiguration>>();
        }

        [Fact]
        public void GivenCircuitBreakerClient_WhenCircuitBreakerClientIsNotPassed_ThenArgumentNullExceptionIsThrown()
        {
            var durableClientFactory = _fixture.Freeze<Mock<IDurableClientFactory>>();
            Assert.Throws<ArgumentNullException>(() =>
                new CircuitBreakerClient(durableClientFactory.Object,null,_loggerMock.Object, _configurationMock.Object, null));
        }

        [Fact]
        public void GivenCircuitBreakerClient_WhenLoggerIsNotPassed_ThenArgumentNullExceptionIsThrown()
        {
            var durableClientFactory = _fixture.Freeze<Mock<IDurableClientFactory>>();
            Assert.Throws<ArgumentNullException>(() =>
                new CircuitBreakerClient(durableClientFactory.Object, _durableCircuitBreakerClientMock.Object, null, _configurationMock.Object, null));
        }

        [Fact]
        public void GivenCircuitBreakerClient_WhenConfigurationIsNotPassed_ThenArgumentNullExceptionIsThrown()
        {
            var durableClientFactory = _fixture.Freeze<Mock<IDurableClientFactory>>();
            Assert.Throws<ArgumentNullException>(() =>
                new CircuitBreakerClient(durableClientFactory.Object, _durableCircuitBreakerClientMock.Object, _loggerMock.Object, null, null));
        }

        [Fact]
        public async Task ExecuteGeneric_ActionWithResult_SuccessReported()
        {
            var circuitBreakerId = _fixture.Create<string>();

            _durableCircuitBreakerClientMock.SetupIsExecutionPermitted(circuitBreakerId, true);

            var resultObject = _fixture.Create<object>();

            var client = _fixture.Create<CircuitBreakerClient>();
            var actionMock = _fixture.Create<Mock<Func<Task<object>>>>();
            actionMock.Setup(x => x.Invoke()).Returns(Task.FromResult(resultObject));

            var result = await client.Execute(circuitBreakerId, actionMock.Object);
            Assert.Equal(resultObject, result);

            _durableCircuitBreakerClientMock.VerifyOnlyOneSuccess(circuitBreakerId);
        }

        [Fact]
        public async Task ExecuteGeneric_ActionWithException_SuccessReported()
        {
            var circuitBreakerId = _fixture.Create<string>();

            _durableCircuitBreakerClientMock.SetupIsExecutionPermitted(circuitBreakerId, true);

            var actionMock = _fixture.Create<Mock<Func<Task<object>>>>();
            actionMock.Setup(x => x.Invoke()).Throws<InvalidDataException>();

            var client = _fixture.Create<CircuitBreakerClient>();
            await Assert.ThrowsAsync<InvalidDataException>(async () =>
                await client.Execute(circuitBreakerId, actionMock.Object));

            actionMock.Verify(x => x.Invoke(), Times.Once);

            _durableCircuitBreakerClientMock.VerifyOnlyOneSuccess(circuitBreakerId);
        }

        [Fact]
        public async Task ExecuteGeneric_ActionWithRepeatableException_FailureReported()
        {
            var circuitBreakerId = _fixture.Create<string>();

            _durableCircuitBreakerClientMock.SetupIsExecutionPermitted(circuitBreakerId, true);

            var actionMock = _fixture.Create<Mock<Func<Task<object>>>>();
            actionMock.Setup(x => x.Invoke()).Throws<RetriableOperationFailedException>();

            var client = _fixture.Create<CircuitBreakerClient>();
            await Assert.ThrowsAsync<RetriableOperationFailedException>(async () =>
                await client.Execute(circuitBreakerId, actionMock.Object));

            actionMock.Verify(x => x.Invoke(), Times.Once);

            _durableCircuitBreakerClientMock.VerifyOnlyOneFailure(circuitBreakerId);
        }

        [Fact]
        public async Task GivenExecuteWithResult_WhenExecutionIsNotPermittedAndCallbackIsNull_ThenActionIsExecuted()
        {
            var circuitBreakerId = _fixture.Create<string>();

            _durableCircuitBreakerClientMock.SetupIsExecutionPermitted(circuitBreakerId, false);

            var resultObject = _fixture.Create<object>();

            var client = _fixture.Create<CircuitBreakerClient>();
            var actionMock = _fixture.Create<Mock<Func<Task<object>>>>();
            actionMock.Setup(x => x.Invoke()).Returns(Task.FromResult(resultObject));

            var result = await client.Execute(circuitBreakerId, actionMock.Object);

            Assert.Equal(resultObject, result);
        }

        [Fact]
        public async Task GivenExecuteWithResult_WhenExecutionIsNotPermittedAndCallbackIsNotNull_ThenCallbackIsExecuted()
        {
            var circuitBreakerId = _fixture.Create<string>();

            _durableCircuitBreakerClientMock.SetupIsExecutionPermitted(circuitBreakerId, false);

            var resultObject = _fixture.Create<object>();

            var client = _fixture.Create<CircuitBreakerClient>();
            var callbackMock = _fixture.Create<Mock<Func<Task<object>>>>();
            callbackMock.Setup(x => x.Invoke()).Returns(Task.FromResult(resultObject));

            var result = await client.Execute(circuitBreakerId, null, callbackMock.Object);

            Assert.Equal(resultObject, result);
        }

        [Fact]
        public async Task Execute_ActionWithResult_SuccessReported()
        {
            var circuitBreakerId = _fixture.Create<string>();

            _durableCircuitBreakerClientMock.SetupIsExecutionPermitted(circuitBreakerId, true);

            var client = _fixture.Create<CircuitBreakerClient>();
            var actionMock = _fixture.Create<Mock<Func<Task>>>();

            await client.Execute(circuitBreakerId, actionMock.Object);

            actionMock.Verify(x => x.Invoke(), Times.Once);

            _durableCircuitBreakerClientMock.VerifyOnlyOneSuccess(circuitBreakerId);
        }

        [Fact]
        public async Task Execute_ActionWithException_SuccessReported()
        {
            var circuitBreakerId = _fixture.Create<string>();

            _durableCircuitBreakerClientMock.SetupIsExecutionPermitted(circuitBreakerId, true);

            var actionMock = _fixture.Create<Mock<Func<Task>>>();
            actionMock.Setup(x => x.Invoke()).Throws<InvalidDataException>();

            var client = _fixture.Create<CircuitBreakerClient>();
            await Assert.ThrowsAsync<InvalidDataException>(async () =>
                await client.Execute(circuitBreakerId, actionMock.Object));

            actionMock.Verify(x => x.Invoke(), Times.Once);

            _durableCircuitBreakerClientMock.VerifyOnlyOneSuccess(circuitBreakerId);
        }

        [Fact]
        public async Task Execute_ActionWithRepeatableException_FailureReported()
        {
            var circuitBreakerId = _fixture.Create<string>();

            _durableCircuitBreakerClientMock.SetupIsExecutionPermitted(circuitBreakerId, true);

            var actionMock = _fixture.Create<Mock<Func<Task>>>();
            actionMock.Setup(x => x.Invoke()).Throws<RetriableOperationFailedException>();

            var client = _fixture.Create<CircuitBreakerClient>();
            await Assert.ThrowsAsync<RetriableOperationFailedException>(async () =>
                await client.Execute(circuitBreakerId, actionMock.Object));

            actionMock.Verify(x => x.Invoke(), Times.Once);

            _durableCircuitBreakerClientMock.VerifyOnlyOneFailure(circuitBreakerId);
        }

        [Fact]
        public async Task GivenExecute_WhenExecutionIsNotPermittedAndCallbackIsNull_ThenActionIsExecuted()
        {
            var circuitBreakerId = _fixture.Create<string>();

            _durableCircuitBreakerClientMock.SetupIsExecutionPermitted(circuitBreakerId, false);

            var client = _fixture.Create<CircuitBreakerClient>();
            var actionMock = _fixture.Create<Mock<Func<Task>>>();

            await client.Execute(circuitBreakerId, actionMock.Object);

            actionMock.Verify(x => x.Invoke(), Times.Once);

            _durableCircuitBreakerClientMock.VerifyOnlyOneSuccess(circuitBreakerId);
        }

        [Fact]
        public async Task GivenExecute_WhenExecutionIsNotPermittedAndCallbackIsNotNull_ThenCallbackIsExecuted()
        {
            var circuitBreakerId = _fixture.Create<string>();

            _durableCircuitBreakerClientMock.SetupIsExecutionPermitted(circuitBreakerId, false);

            var client = _fixture.Create<CircuitBreakerClient>();
            var callbackMock = _fixture.Create<Mock<Func<Task>>>();
            var actionMock = _fixture.Create<Mock<Func<Task>>>();

            await client.Execute(circuitBreakerId, actionMock.Object, callbackMock.Object);

            actionMock.Verify(x => x.Invoke(), Times.Once);
            callbackMock.Verify(x => x.Invoke(), Times.Once);
        }
    }
}
