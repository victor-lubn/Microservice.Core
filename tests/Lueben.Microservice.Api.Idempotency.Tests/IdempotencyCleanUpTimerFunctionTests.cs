using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Lueben.Microservice.Api.Idempotency.Functions;
using Lueben.Microservice.Api.Idempotency.IdempotencyDataProviders;
using Lueben.Microservice.Api.Idempotency.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lueben.Microservice.Api.Idempotency.Tests
{
    public class IdempotencyCleanUpTimerFunctionTests
    {
        private readonly IFixture _fixture;
        private Mock<IIdempotencyDataProvider<IdempotencyEntity>> _dataProviderMock;
        private IdempotencyCleanUpTimerFunction _function;

        public IdempotencyCleanUpTimerFunctionTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _dataProviderMock = new Mock<IIdempotencyDataProvider<IdempotencyEntity>>();

            _function = new IdempotencyCleanUpTimerFunction(_dataProviderMock.Object);
        }

        [Fact]
        public async Task GivenTriggerIdempotencyCleanUpTimer_WhenCalled_ThenCleanUpIsExecuted()
        {
            var loggerMock = new Mock<ILogger>();
            var timerInfo = _fixture.Create<TimerInfo>();

            await _function.TriggerIdempotencyCleanUpTimer(timerInfo, loggerMock.Object);

            _dataProviderMock.Verify(x => x.CleanUp(), Times.Once);
        }

        [Fact]
        public async Task GivenTriggerIdempotencyCleanUpTimer_WhenCalledAndPastDue_ThenShouldCheckIt()
        {
            var loggerMock = new Mock<ILogger>();
            var timerInfo = new TimerInfo
            {
                ScheduleStatus = new ScheduleStatus(),
                IsPastDue = true
            };

            await _function.TriggerIdempotencyCleanUpTimer(timerInfo, loggerMock.Object);

           _dataProviderMock.Verify(x => x.CleanUp(), Times.Once);
        }
    }
}
