using Polly.Registry;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.Options;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Polly;

namespace Lueben.Microservice.CircuitBreaker.Tests
{
    public class DurableCircuitBreakerClientTests
    {
        private const string CircuitBreakerId = "CBTestId";
        private readonly DurableCircuitBreakerClient _client;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IDurableOrchestrationContext> _contextMock;
        private readonly Mock<IDurableCircuitBreaker> _circuitBreakerMock;
        private readonly Mock<IDurableClient> _durableClientMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<OptionsManager<CircuitBreakerSettings>> _optionsManagerMock;
        private readonly Mock<IPolicyRegistry<string>> _policyRegistryMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;

        public DurableCircuitBreakerClientTests()
        {
            _policyRegistryMock = new Mock<IPolicyRegistry<string>>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            var optionsFactoryMock = new Mock<IOptionsFactory<CircuitBreakerSettings>>();

            _optionsManagerMock = new Mock<OptionsManager<CircuitBreakerSettings>>(optionsFactoryMock.Object);

            _client = new DurableCircuitBreakerClient(_optionsManagerMock.Object, _policyRegistryMock.Object, _serviceProviderMock.Object);
            _loggerMock = new Mock<ILogger>();
            _contextMock = new Mock<IDurableOrchestrationContext>();
            _circuitBreakerMock = new Mock<IDurableCircuitBreaker>();
            _contextMock.Setup(x => x.GetInput<string>()).Returns(CircuitBreakerId);
            _contextMock.Setup(x => x.CreateEntityProxy<IDurableCircuitBreaker>(CircuitBreakerId))
                .Returns(_circuitBreakerMock.Object);
            _durableClientMock = new Mock<IDurableClient>();
            _configurationMock = new Mock<IConfiguration>();
        }

        [Fact]
        public void GivenDurableCircuitBreakerClient_WhenOptionsAreNotPassed_ThenArgumentNullExceptionIsThrown()
        {
            var policyRegistryMock = new Mock<IPolicyRegistry<string>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
           
            Assert.Throws<ArgumentNullException>(() => new DurableCircuitBreakerClient(null, policyRegistryMock.Object, serviceProviderMock.Object));
        }

        [Fact]
        public async Task GivenIsExecutionPermitted_WhenCircuitBreakerIdIsNotPassed_ThenArgumentNullExceptionIsThrown()
        {
            var loggerMock = new Mock<ILogger>();
            var contextMock = new Mock<IDurableOrchestrationContext>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _client.IsExecutionPermitted(string.Empty, loggerMock.Object, contextMock.Object));
        }

        [Fact]
        public async Task GivenIsExecutionPermitted_WhenCircuitBreakerIdIsPassed_ThenIsExecutionPermittedReturned()
        {
            var isExecutionPermitted = true;
            _circuitBreakerMock.Setup(x => x.IsExecutionPermitted()).Returns(Task.FromResult(isExecutionPermitted));

            var result = await _client.IsExecutionPermitted(CircuitBreakerId, _loggerMock.Object, _contextMock.Object);

            Assert.Equal(isExecutionPermitted, result);
            _contextMock.Verify(x => x.CreateEntityProxy<IDurableCircuitBreaker>(CircuitBreakerId), Times.Once);
        }
        
        [Fact]
        public async Task GivenIsExecutionPermittedStrongConsistency_WhenRetryConfigurationIsIncorrect_ThenArgumentExceptionIsThrown()
        {
            var settings = new CircuitBreakerSettings
            {
                ConsistencyPriorityCheckCircuitRetryInterval = "PT3S",
                ConsistencyPriorityCheckCircuitTimeout = "PT2S"
            };
            _optionsManagerMock.Setup(x => x.Get(CircuitBreakerId)).Returns(settings);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _client.IsExecutionPermitted_StrongConsistency(CircuitBreakerId, _loggerMock.Object,
                    _durableClientMock.Object, _configurationMock.Object));

        }

        [Fact]
        public async Task GivenIsExecutionPermittedStrongConsistency_WhenOrchestrationCompleted_ThenOrchestrationOutputAsExecutionPermittedIsReturned()
        {
            var settings = new CircuitBreakerSettings
            {
                ConsistencyPriorityCheckCircuitRetryInterval = "PT0S",
                ConsistencyPriorityCheckCircuitTimeout = "PT0S"
            };
            var executionPermittedInstanceId = "testId";
            var isExecutionPermitted = true;
            _optionsManagerMock.Setup(x => x.Get(CircuitBreakerId)).Returns(settings);
            _durableClientMock.Setup(x => x.StartNewAsync(It.IsAny<string>(), CircuitBreakerId))
                .ReturnsAsync(executionPermittedInstanceId);
            var status = OrchestrationRuntimeStatus.Completed;
            _durableClientMock.Setup(x => x.GetStatusAsync(executionPermittedInstanceId, default, default, default))
                .ReturnsAsync(new DurableOrchestrationStatus{ RuntimeStatus = status, Output = isExecutionPermitted});

            var result = await _client.IsExecutionPermitted_StrongConsistency(CircuitBreakerId, _loggerMock.Object,
                _durableClientMock.Object, _configurationMock.Object);

            Assert.Equal(isExecutionPermitted, result);
        }

        [Fact]
        public async Task GivenIsExecutionPermittedStrongConsistency_WhenOrchestrationOutputIsIncorrect_ThenExecutionPermittedIsReturned()
        {
            var settings = new CircuitBreakerSettings
            {
                ConsistencyPriorityCheckCircuitRetryInterval = "PT0S",
                ConsistencyPriorityCheckCircuitTimeout = "PT0S"
            };
            var executionPermittedInstanceId = "testId";
            _optionsManagerMock.Setup(x => x.Get(CircuitBreakerId)).Returns(settings);
            _durableClientMock.Setup(x => x.StartNewAsync(It.IsAny<string>(), CircuitBreakerId))
                .ReturnsAsync(executionPermittedInstanceId);
            var status = OrchestrationRuntimeStatus.Completed;
            _durableClientMock.Setup(x => x.GetStatusAsync(executionPermittedInstanceId, default, default, default))
                .ReturnsAsync(new DurableOrchestrationStatus { RuntimeStatus = status, Output = "incorrect" });

            var result = await _client.IsExecutionPermitted_StrongConsistency(CircuitBreakerId, _loggerMock.Object,
                _durableClientMock.Object, _configurationMock.Object);

            Assert.True(result);
        }

        [Theory]
        [InlineData(OrchestrationRuntimeStatus.Canceled)]
        [InlineData(OrchestrationRuntimeStatus.Failed)]
        [InlineData(OrchestrationRuntimeStatus.Terminated)]
        public async Task GivenIsExecutionPermittedStrongConsistency_WhenOrchestrationStatusIsNotSuccess_ThenExecutionPermittedIsReturned(OrchestrationRuntimeStatus status)
        {
            var settings = new CircuitBreakerSettings
            {
                ConsistencyPriorityCheckCircuitRetryInterval = "PT0S",
                ConsistencyPriorityCheckCircuitTimeout = "PT0S"
            };
            var executionPermittedInstanceId = "testId";
            _optionsManagerMock.Setup(x => x.Get(CircuitBreakerId)).Returns(settings);
            _durableClientMock.Setup(x => x.StartNewAsync(It.IsAny<string>(), CircuitBreakerId))
                .ReturnsAsync(executionPermittedInstanceId);
            _durableClientMock.Setup(x => x.GetStatusAsync(executionPermittedInstanceId, default, default, default))
                .ReturnsAsync(new DurableOrchestrationStatus { RuntimeStatus = status, Output = "incorrect" });

            var result = await _client.IsExecutionPermitted_StrongConsistency(CircuitBreakerId, _loggerMock.Object,
                _durableClientMock.Object, _configurationMock.Object);

            Assert.True(result);
        }

        [Fact]
        public async Task GivenIsExecutionPermittedStrongConsistency_WhenOrchestrationStatusIsPending_ThenExecutionPermittedReturned()
        {
            var settings = new CircuitBreakerSettings
            {
                ConsistencyPriorityCheckCircuitRetryInterval = "PT0S",
                ConsistencyPriorityCheckCircuitTimeout = "PT0S"
            };
            var executionPermittedInstanceId = "testId";
            _optionsManagerMock.Setup(x => x.Get(CircuitBreakerId)).Returns(settings);
            _durableClientMock.Setup(x => x.StartNewAsync(It.IsAny<string>(), CircuitBreakerId))
                .ReturnsAsync(executionPermittedInstanceId);
            var status = OrchestrationRuntimeStatus.Pending;
            _durableClientMock.Setup(x => x.GetStatusAsync(executionPermittedInstanceId, default, default, default))
                .ReturnsAsync(new DurableOrchestrationStatus { RuntimeStatus = status, Output = true });

            var result = await _client.IsExecutionPermitted_StrongConsistency(CircuitBreakerId, _loggerMock.Object,
                _durableClientMock.Object, _configurationMock.Object);

            Assert.True(result);
        }

        [Fact]
        public async Task GivenRecordSuccess_WhenIsCalled_ThenExpectedMethodIsExecuted()
        {
            await _client.RecordSuccess(CircuitBreakerId, _loggerMock.Object, _contextMock.Object);

            _circuitBreakerMock.Verify(x => x.RecordSuccess(), Times.Once);
            _contextMock.Verify(x => x.CreateEntityProxy<IDurableCircuitBreaker>(CircuitBreakerId), Times.Once);
        }

        [Fact]
        public async Task GivenRecordFailure_WhenIsCalled_ThenExpectedMethodIsExecuted()
        {
            await _client.RecordFailure(CircuitBreakerId, _loggerMock.Object, _contextMock.Object);

            _circuitBreakerMock.Verify(x => x.RecordFailure(), Times.Once);
            _contextMock.Verify(x => x.CreateEntityProxy<IDurableCircuitBreaker>(CircuitBreakerId), Times.Once);
        }

        [Fact]
        public async Task GivenGetCircuitState_WhenIsCalled_ThenStateIsReturned()
        {
            var expectedState = CircuitState.Closed;
            _circuitBreakerMock.Setup(x => x.GetCircuitState()).ReturnsAsync(expectedState);

            var  result = await _client.GetCircuitState(CircuitBreakerId, _loggerMock.Object, _contextMock.Object);

            Assert.Equal(expectedState, result);

            _circuitBreakerMock.Verify(x => x.GetCircuitState(), Times.Once);
            _contextMock.Verify(x => x.CreateEntityProxy<IDurableCircuitBreaker>(CircuitBreakerId), Times.Once);
        }

        [Fact]
        public async Task GivenGetBreakerState_WhenIsCalled_ThenStateIsReturned()
        {
            var expectedValue = new DurableCircuitBreaker(_loggerMock.Object);
            _circuitBreakerMock.Setup(x => x.GetBreakerState()).ReturnsAsync(expectedValue);

            var result = await _client.GetBreakerState(CircuitBreakerId, _loggerMock.Object, _contextMock.Object);

            Assert.Equal(expectedValue, result);

            _circuitBreakerMock.Verify(x => x.GetBreakerState(), Times.Once);
            _contextMock.Verify(x => x.CreateEntityProxy<IDurableCircuitBreaker>(CircuitBreakerId), Times.Once);
        }

        [Fact]
        public async Task GivenIsExecutionPermitted_WhenCircuitBreakerEntityNotExists_ThenExecutionPermittedReturned()
        {
            var settings = new CircuitBreakerSettings
            {
                PerformancePriorityCheckCircuitInterval = "PT3S"
            };
            _optionsManagerMock.Setup(x => x.Get(CircuitBreakerId)).Returns(settings);

            var breakerState = new EntityStateResponse<DurableCircuitBreaker>
            {
                EntityExists = false
            };

            _durableClientMock.Setup(x => x.ReadEntityStateAsync<DurableCircuitBreaker>(It.Is<EntityId>(e => e.EntityKey == CircuitBreakerId), default, default))
                .ReturnsAsync(breakerState);
            var cachePolicyMock = new Mock<IAsyncPolicy<DurableCircuitBreaker>>();
            var cachePolicy = cachePolicyMock.Object;
            _policyRegistryMock.Setup(x => x.TryGet(It.IsAny<string>(), out cachePolicy)).Returns(true);
            var breaker = new DurableCircuitBreaker(_loggerMock.Object)
            {
                CircuitState = CircuitState.Open
            };

            var result = await _client.IsExecutionPermitted(CircuitBreakerId, _loggerMock.Object, _durableClientMock.Object, _configurationMock.Object);

            Assert.True(result);
        }

        [Theory]
        [InlineData(CircuitState.HalfOpen, -2, true)]
        [InlineData(CircuitState.HalfOpen, 2, false)]
        [InlineData(CircuitState.Open, -2, true)]
        [InlineData(CircuitState.Open, 2, false)]
        public async Task GivenIsExecutionPermitted_WhenCircuitBreakerIsOpen_ThenExpectedExecutionPermittedReturned(CircuitState state, int brokerUntilDiff, bool expectedResult)
        {
            var settings = new CircuitBreakerSettings
            {
                PerformancePriorityCheckCircuitInterval = "PT3S"
            };
            _optionsManagerMock.Setup(x => x.Get(CircuitBreakerId)).Returns(settings);

            var breakerState = new EntityStateResponse<DurableCircuitBreaker>
            {
                EntityExists = true
            };

            _durableClientMock.Setup(x => x.ReadEntityStateAsync<DurableCircuitBreaker>(It.Is<EntityId>(e => e.EntityKey == CircuitBreakerId), default, default))
                .ReturnsAsync(breakerState);

            var cachePolicyMock = new Mock<IAsyncPolicy<DurableCircuitBreaker>>();
            var cachePolicy = cachePolicyMock.Object;
            _policyRegistryMock.Setup(x => x.TryGet(It.IsAny<string>(), out cachePolicy)).Returns(true);
            var breaker = new DurableCircuitBreaker(_loggerMock.Object)
            {
                CircuitState = state,
                BrokenUntil = DateTime.UtcNow.AddHours(brokerUntilDiff)
            };

            cachePolicyMock
                .Setup(x => x.ExecuteAsync(It.IsAny<Func<Context, Task<DurableCircuitBreaker>>>(), It.IsAny<Polly.Context>()))
                .ReturnsAsync(breaker);

            var result = await _client.IsExecutionPermitted(CircuitBreakerId, _loggerMock.Object, _durableClientMock.Object, _configurationMock.Object);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GivenIsExecutionPermitted_WhenCircuitBreakerIsClosed_ThenExecutionPermittedReturned()
        {
            var settings = new CircuitBreakerSettings
            {
                PerformancePriorityCheckCircuitInterval = "PT3S"
            };
            _optionsManagerMock.Setup(x => x.Get(CircuitBreakerId)).Returns(settings);

            var breakerState = new EntityStateResponse<DurableCircuitBreaker>
            {
                EntityExists = true
            };

            _durableClientMock.Setup(x => x.ReadEntityStateAsync<DurableCircuitBreaker>(It.Is<EntityId>(e => e.EntityKey == CircuitBreakerId), default, default))
                .ReturnsAsync(breakerState);

            var cachePolicyMock = new Mock<IAsyncPolicy<DurableCircuitBreaker>>();
            var cachePolicy = cachePolicyMock.Object;
            _policyRegistryMock.Setup(x => x.TryGet(It.IsAny<string>(), out cachePolicy)).Returns(true);
            var breaker = new DurableCircuitBreaker(_loggerMock.Object)
            {
                CircuitState = CircuitState.Closed,
            };

            cachePolicyMock
                .Setup(x => x.ExecuteAsync(It.IsAny<Func<Context, Task<DurableCircuitBreaker>>>(), It.IsAny<Polly.Context>()))
                .ReturnsAsync(breaker);

            var result = await _client.IsExecutionPermitted(CircuitBreakerId, _loggerMock.Object, _durableClientMock.Object, _configurationMock.Object);

            Assert.True(result);
        }

        [Fact]
        public async Task GivenRecordFailure_WhenIsCalledForDurableClient_ThenExpectedMethodIsExecuted()
        {
            await _client.RecordFailure(CircuitBreakerId, _loggerMock.Object, _durableClientMock.Object);

            _durableClientMock.Verify(
                x => x.SignalEntityAsync(CircuitBreakerId,
                    It.IsAny<Action<IDurableCircuitBreaker>>()), Times.Once);
        }

        [Theory]
        [InlineData(true, false, CircuitState.Open, CircuitState.Closed)]
        [InlineData(false, false, CircuitState.Open, CircuitState.Closed)]
        [InlineData(true, true, CircuitState.Open, CircuitState.Open)]
        public async Task GivenGetCircuitState_WhenIsCalledForDurableClient_ThenExpectedEntityStateIsReturned(bool entityExists, bool isEntityStateDefined, CircuitState entityState, CircuitState expectedResult)
        {
            var breakerState = new EntityStateResponse<DurableCircuitBreaker>
            {
                EntityExists = entityExists,
                EntityState = isEntityStateDefined
                    ? new DurableCircuitBreaker(_loggerMock.Object)
                    {
                        CircuitState = entityState
                    }
                    : null
            };
            _durableClientMock
                .Setup(x => x.ReadEntityStateAsync<DurableCircuitBreaker>(It.IsAny<EntityId>(), default, default))
                .ReturnsAsync(breakerState);

            var result = await _client.GetCircuitState(CircuitBreakerId, _loggerMock.Object, _durableClientMock.Object);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, false, true)]
        [InlineData(true, true, false)]
        public async Task GivenGetBreakerState_WhenIsCalledForDurableClient_ThenDurableClientStateIsReturned(bool entityExists, bool isEntityStateDefined, bool isExpectedResultNull)
        {
            var breakerState = new EntityStateResponse<DurableCircuitBreaker>
            {
                EntityExists = entityExists,
                EntityState = isEntityStateDefined ? new DurableCircuitBreaker(_loggerMock.Object) : null
            };
            _durableClientMock
                .Setup(x => x.ReadEntityStateAsync<DurableCircuitBreaker>(It.IsAny<EntityId>(), default, default))
                .ReturnsAsync(breakerState);

            var result = await _client.GetBreakerState(CircuitBreakerId, _loggerMock.Object, _durableClientMock.Object);

            if (isExpectedResultNull)
            {
                Assert.Null(result);
            }
            else
            {
                Assert.NotNull(result);
            }
        }
    }
}
