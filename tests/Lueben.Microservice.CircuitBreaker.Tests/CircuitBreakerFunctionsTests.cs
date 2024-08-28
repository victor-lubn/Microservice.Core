using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.CircuitBreaker.Tests
{
    public class CircuitBreakerFunctionsTests
    {
        private const string CircuitBreakerId = "CBTestId";

        private readonly Mock<IDurableOrchestrationContext> _orchContextMock;
        private readonly Mock<IDurableEntityContext> _entityContextMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<OptionsManager<CircuitBreakerSettings>> _optionsManagerMock;
        private readonly CircuitBreakerFunctions _function;

        public CircuitBreakerFunctionsTests()
        {
            _orchContextMock = new Mock<IDurableOrchestrationContext>();
            _entityContextMock = new Mock<IDurableEntityContext>();
            _loggerMock = new Mock<ILogger>();

            var optionsFactoryMock = new Mock<IOptionsFactory<CircuitBreakerSettings>>();
            _optionsManagerMock = new Mock<OptionsManager<CircuitBreakerSettings>>(optionsFactoryMock.Object);

            _entityContextMock.Setup(x => x.EntityKey).Returns(CircuitBreakerId);
            Entity.SetMockContext(_entityContextMock.Object);


            _function = new CircuitBreakerFunctions(_optionsManagerMock.Object);
        }

        [Fact]
        public void GivenCircuitBreakerFunctions_WhenOptionsAreNotPassed_ThenArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new CircuitBreakerFunctions(null));
        }

        [Fact]
        public async Task GivenCircuitBreakerFunctions_WhenRunAndStateIsSet_ThenExpectedMethodIsExecuted()
        {
            _entityContextMock.Setup(x => x.HasState).Returns(true);

            await _function.Run(_entityContextMock.Object, _loggerMock.Object);

            _entityContextMock.Verify(x => x.SetState(It.IsAny<object>()), Times.Never);
            _entityContextMock.Verify(x => x.DispatchAsync<DurableCircuitBreaker>(_loggerMock.Object), Times.Once);
        }

        [Fact]
        public async Task GivenCircuitBreakerFunctions_WhenRunAndStateIsNotSetAndSettingsAreCorrect_ThenStateIsSet()
        {
            var settings = new CircuitBreakerSettings
            {
                ConsistencyPriorityCheckCircuitRetryInterval = "PT3S",
                ConsistencyPriorityCheckCircuitTimeout = "PT2S",
                BreakDuration = "PT2S",
                MaxConsecutiveFailures = 3
            };
            _optionsManagerMock.Setup(x => x.Get(CircuitBreakerId)).Returns(settings);

            _entityContextMock.Setup(x => x.HasState).Returns(false);

            await _function.Run(_entityContextMock.Object, _loggerMock.Object);

            _entityContextMock.Verify(x => x.SetState(It.Is<object>(o=> ((DurableCircuitBreaker)o).CircuitState == CircuitState.Closed)), Times.Once);
        }

        [Fact]
        public async Task GivenCircuitBreakerFunctions_WhenRunAndStateIsNotSetAndBreakDurationIsNotSpecified_ThenInvalidOperationExceptionIsThrown()
        {
            var settings = new CircuitBreakerSettings
            {
                ConsistencyPriorityCheckCircuitRetryInterval = "PT3S",
                ConsistencyPriorityCheckCircuitTimeout = "PT2S",
                BreakDuration = "PT0S",
                MaxConsecutiveFailures = 3
            };
            _optionsManagerMock.Setup(x => x.Get(CircuitBreakerId)).Returns(settings);

            _entityContextMock.Setup(x => x.HasState).Returns(false);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _function.Run(_entityContextMock.Object, _loggerMock.Object));
        }

        [Fact]
        public async Task GivenCircuitBreakerFunctions_WhenRunAndStateIsNotSetAndFailuresNumberIsNotSpecified_ThenInvalidOperationExceptionIsThrown()
        {
            var settings = new CircuitBreakerSettings
            {
                ConsistencyPriorityCheckCircuitRetryInterval = "PT3S",
                ConsistencyPriorityCheckCircuitTimeout = "PT2S",
                BreakDuration = "PT10S",
                MaxConsecutiveFailures = 0
            };
            _optionsManagerMock.Setup(x => x.Get(CircuitBreakerId)).Returns(settings);

            _entityContextMock.Setup(x => x.HasState).Returns(false);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _function.Run(_entityContextMock.Object, _loggerMock.Object));
        }

        [Fact]
        public async Task GivenIsExecutionPermittedInternalOrchestrator_WhenCalledAndNoBreakerIdPassed_ThenInvalidOperationExceptionIsThrown()
        {
            _orchContextMock.Setup(x => x.GetInput<string>()).Returns(string.Empty);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _function.IsExecutionPermittedInternalOrchestrator(_orchContextMock.Object, _loggerMock.Object));
        }

        [Fact]
        public async Task GivenIsExecutionPermittedInternalOrchestrator_WhenCalledAndBreakerIdPassed_ThenProxyIsCreated()
        {
            var breakerId = "testId";
            var isExecutionPermitted = true;
   
            var cbMock = new Mock<IDurableCircuitBreaker>();
            cbMock.Setup(x => x.IsExecutionPermitted()).Returns(Task.FromResult(isExecutionPermitted));
          
            _orchContextMock.Setup(x => x.GetInput<string>()).Returns(breakerId);
            _orchContextMock.Setup(x => x.CreateEntityProxy<IDurableCircuitBreaker>(breakerId))
                .Returns(cbMock.Object);

            var result = await _function.IsExecutionPermittedInternalOrchestrator(_orchContextMock.Object, _loggerMock.Object);

            Assert.Equal(isExecutionPermitted, result);
            _orchContextMock.Verify(x => x.CreateEntityProxy<IDurableCircuitBreaker>(breakerId), Times.Once);
        }

        [Fact]
        public async Task GivenIsExecutionPermitted_WhenCalledAndBreakerIdIsNotPassed_ThenArgumentNullExceptionIsThrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _function.IsExecutionPermitted(null, _loggerMock.Object, _orchContextMock.Object));
        }

        [Fact]
        public void GivenCircuitBreakerSettings_WhenInstantiated_ThenExpectedPropertiesValues()
        {
            var settings = new CircuitBreakerSettings
            {
                ConsistencyPriorityCheckCircuitTimeout = "PT1S",
                BreakDuration = "PT10S",
                ConsistencyPriorityCheckCircuitRetryInterval = "PT3S"
            };

            Assert.Equal(10, settings.BreakDurationTime.Seconds);
            Assert.Equal(1, settings.ConsistencyPriorityCheckCircuitTimeoutTime.Seconds);
            Assert.Equal(2, settings.PerformancePriorityCheckCircuitIntervalTime.Seconds);
            Assert.Equal(3, settings.ConsistencyPriorityCheckCircuitRetryIntervalTime.Seconds);
        }

        [Fact]
        public void GivenLogCircuitBreakerMessage_WhenCalled_ThenInfoMessageIsLogged()
        {
            var logger = _loggerMock.Object;
            var message = "message";
            logger.LogCircuitBreakerMessage("test", message);

            _loggerMock.Verify(l => l.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Debug),
                    It.Is<EventId>(eventId => eventId.Id == 0),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == message && @type.Name == "FormattedLogValues"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
