using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lueben.Microservice.CircuitBreaker.Tests
{
    public class DurableCircuitBreakerTests
    {
        private const string CircuitBreakerId = "CBTestId";

        private readonly DurableCircuitBreaker _breaker;

        public DurableCircuitBreakerTests()
        {
            var loggerMock = new Mock<ILogger>();
            _breaker = new DurableCircuitBreaker(loggerMock.Object);
            var contextMock = new Mock<IDurableEntityContext>();
            contextMock.Setup(x => x.EntityKey).Returns(CircuitBreakerId);
            Entity.SetMockContext(contextMock.Object);
        }

        [Fact]
        public void GivenGetEntityId_WhenCalled_ThenEntityIsCreated()
        {
            var result = DurableCircuitBreaker.GetEntityId(CircuitBreakerId);

            Assert.Equal(CircuitBreakerId, result.EntityKey);
            Assert.Equal(nameof(DurableCircuitBreaker).ToLowerInvariant(), result.EntityName);
        }

        [Fact]

        public async Task GivenIsExecutionPermitted_WhenCalledAndBreakerClosed_ThenTrueIsReturned()
        {
            _breaker.CircuitState = CircuitState.Closed;

            var result = await _breaker.IsExecutionPermitted();

            Assert.True(result);
        }

        [Fact]

        public async Task GivenIsExecutionPermitted_WhenCalledAndBreakerOpenAndBrokerUntilPassed_ThenTrueIsReturned()
        {
            _breaker.CircuitState = CircuitState.Open;
            _breaker.BrokenUntil = DateTime.UtcNow.AddHours(-2);

            var result = await _breaker.IsExecutionPermitted();

            Assert.True(result);
        }

        [Fact]

        public async Task GivenIsExecutionPermitted_WhenCalledAndBreakerHalfOpenAndBrokerUntilNotPassed_ThenFalseIsReturned()
        {
            _breaker.CircuitState = CircuitState.HalfOpen;
            _breaker.BrokenUntil = DateTime.UtcNow.AddHours(2);

            var result = await _breaker.IsExecutionPermitted();

            Assert.False(result);
        }

        [Fact]

        public async Task GivenRecordSuccess_WhenCalledAndBreakerIsHalfOpen_ThenBreakerIsClosed()
        {
            _breaker.CircuitState = CircuitState.HalfOpen;
            _breaker.BrokenUntil = DateTime.UtcNow.AddHours(2);

            var result = await _breaker.RecordSuccess();

            Assert.Equal(CircuitState.Closed, _breaker.CircuitState);
        }

        [Fact]

        public async Task GivenRecordSuccess_WhenCalledAndBreakerIsOpenAndBrokerUntilIsPassed_ThenBreakerIsClosed()
        {
            _breaker.CircuitState = CircuitState.Open;
            _breaker.BrokenUntil = DateTime.UtcNow.AddHours(-2);

            await _breaker.RecordSuccess();

            Assert.Equal(CircuitState.Closed, _breaker.CircuitState);
        }

        [Fact]

        public async Task GivenRecordSuccess_WhenCalledAndBreakerIsClosed_ThenStatusIsNotChanged()
        {
            _breaker.CircuitState = CircuitState.Closed;

            await _breaker.RecordSuccess();

            Assert.Equal(CircuitState.Closed, _breaker.CircuitState);
        }

        [Fact]

        public async Task GivenRecordFailure_WhenCalledAndBreakerIsClosedAndFailuresNumberExceeds_ThenBreakerSetAsOpen()
        {
            _breaker.CircuitState = CircuitState.Closed;
            _breaker.MaxConsecutiveFailures = 2;
            _breaker.ConsecutiveFailureCount = 3;

            await _breaker.RecordFailure();

            Assert.Equal(CircuitState.Open, _breaker.CircuitState);
        }

        [Fact]

        public async Task GivenRecordFailure_WhenCalledAndBreakerIsHalfOpen_ThenBreakerSetAsOpen()
        {
            _breaker.CircuitState = CircuitState.HalfOpen;

            await _breaker.RecordFailure();

            Assert.Equal(CircuitState.Open, _breaker.CircuitState);
        }


        [Fact]

        public async Task GivenRecordFailure_WhenCalledAndBreakerIsClosedAndFailuresNumberNotExceeds_ThenStatusIsNotChanged()
        {
            _breaker.CircuitState = CircuitState.Closed;
            _breaker.MaxConsecutiveFailures = 5;
            _breaker.ConsecutiveFailureCount = 1;

            await _breaker.RecordFailure();

            Assert.Equal(CircuitState.Closed, _breaker.CircuitState);
        }

        [Fact]

        public async Task GivenRecordFailure_WhenCalledAndBreakerIsOpen_ThenStatusIsNotChanged()
        {
            _breaker.CircuitState = CircuitState.Open;

            await _breaker.RecordFailure();

            Assert.Equal(CircuitState.Open, _breaker.CircuitState);
        }

        [Fact]

        public async Task GivenGetCircuitState_WhenCalled_ThenCurrentStatusIsReturned()
        {
            _breaker.CircuitState = CircuitState.Open;

            var result = await _breaker.GetCircuitState();

            Assert.Equal(CircuitState.Open, result);
        }

        [Fact]

        public async Task GivenGetBreakerState_WhenCalled_ThenBreakerIsReturned()
        {
            var result = await _breaker.GetBreakerState();

            Assert.Equal(_breaker, result);
        }
    }
}
