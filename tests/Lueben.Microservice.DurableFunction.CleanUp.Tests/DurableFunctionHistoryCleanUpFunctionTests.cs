using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using DurableTask.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Lueben.Microservice.DurableFunction.CleanUp.Tests
{
    public class DurableFunctionHistoryCleanUpFunctionTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IDurableOrchestrationClient> _durableClientMock;
        private readonly Mock<IDurableOrchestrationContext> _durableOrchestrationContextMock;

        public DurableFunctionHistoryCleanUpFunctionTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            CorrelationTraceContext.Current = new W3CTraceContext();
            
            _durableClientMock = new Mock<IDurableOrchestrationClient>();
            _durableOrchestrationContextMock = _fixture.Freeze<Mock<IDurableOrchestrationContext>>();
            _durableOrchestrationContextMock.Setup(x => x.CurrentUtcDateTime).Returns(DateTime.UtcNow);
            var optionsMock = _fixture.Freeze<Mock<IOptionsSnapshot<DurableFunctionHistoryCleanUpOptions>>>();
            optionsMock.Setup(x => x.Value).Returns(new DurableFunctionHistoryCleanUpOptions
                {HistoryExpirationDays = 10, MaxHistoryAgeMonths = 1, PurgeHistoryBatchTimeFrameHours = 400});
        }

        [Fact]
        public async Task GivenCleanUp_WhenExecuted_ThenExpectedOrchestratorIsStarted()
        {
            var timerInfo = _fixture.Create<TimerInfo>();
            var function = _fixture.Create<DurableFunctionHistoryCleanUpFunction>();

            await function.PurgeInstanceHistory(timerInfo, _durableClientMock.Object);

            _durableClientMock.Verify(x => x.StartNewAsync(nameof(DurableFunctionHistoryCleanUpFunction.HistoryCleanUpOrchestrator), It.IsAny<string>(), It.IsAny<(DateTime, DateTime)>()), Times.Once);
        }

        [Fact]
        public async Task GivenHistoryCleanUpOrchestrator_WhenExecuted_ThenExpectedActivityIsRun()
        {
            var function = _fixture.Create<DurableFunctionHistoryCleanUpFunction>();
            _durableOrchestrationContextMock.Setup(x =>
                x.CallActivityAsync<PurgeHistoryResult>(nameof(DurableFunctionHistoryCleanUpFunction.HistoryCleanUpActivity),
                    It.IsAny<(DateTime, DateTime)>())).Returns(Task.FromResult(new PurgeHistoryResult(1)));

            await function.HistoryCleanUpOrchestrator(_durableOrchestrationContextMock.Object);

            _durableOrchestrationContextMock.Verify(x =>
                x.CallActivityAsync<PurgeHistoryResult>(nameof(DurableFunctionHistoryCleanUpFunction.HistoryCleanUpActivity),
                    It.IsAny<(DateTime, DateTime)>()), Times.Once);
        }

        [Fact]
        public async Task GivenHistoryCleanUpOrchestrator_WhenNotAllRecordsAreScanned_ThenExecutionShouldBeContinued()
        {
            var function = _fixture.Create<DurableFunctionHistoryCleanUpFunction>();

            var startScanningDate = DateTime.UtcNow.AddMonths(-1);
            var endScanningDate = startScanningDate.AddHours(400);
            _durableOrchestrationContextMock.Setup(x => x.GetInput<(DateTime, DateTime)>())
                .Returns((startScanningDate, endScanningDate));

            _durableOrchestrationContextMock.Setup(x =>
                x.CallActivityAsync<PurgeHistoryResult>(nameof(DurableFunctionHistoryCleanUpFunction.HistoryCleanUpActivity),
                    It.IsAny<(DateTime, DateTime)>())).Returns(Task.FromResult(new PurgeHistoryResult(1)));

            await function.HistoryCleanUpOrchestrator(_durableOrchestrationContextMock.Object);

            _durableOrchestrationContextMock.Verify(x =>
                x.CallActivityAsync<PurgeHistoryResult>(nameof(DurableFunctionHistoryCleanUpFunction.HistoryCleanUpActivity),
                    It.IsAny<(DateTime, DateTime)>()), Times.Once);
            _durableOrchestrationContextMock.Verify(x => x.ContinueAsNew(It.IsAny<(DateTime, DateTime)>(), default), Times.Once);
        }

        [Fact]
        public async Task GivenHistoryCleanUpActivity_WhenExecuted_ThenHistoryPurgeIsPerformed()
        {
            var function = _fixture.Create<DurableFunctionHistoryCleanUpFunction>();
            var result = new PurgeHistoryResult(10);
            var durableClientMock = _fixture.Create<Mock<IDurableOrchestrationClient>>();
            durableClientMock.Setup(x => x.PurgeInstanceHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                    It.Is<IEnumerable<OrchestrationStatus>>(
                        l => l.Count() == 1 && l.Contains(OrchestrationStatus.Completed))))
                .Returns(Task.FromResult(result));

            var actualResult = await function.HistoryCleanUpActivity((DateTime.MinValue, DateTime.MaxValue), durableClientMock.Object);

            Assert.Equal(result, actualResult);
        }

        [Fact]
        public async Task GivenHistoryCleanUpActivity_WhenException_ThenNullIsReturned()
        {
            var function = _fixture.Create<DurableFunctionHistoryCleanUpFunction>();
            var result = new PurgeHistoryResult(10);
            var durableClientMock = _fixture.Create<Mock<IDurableOrchestrationClient>>();
            durableClientMock.Setup(x => x.PurgeInstanceHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                    It.Is<IEnumerable<OrchestrationStatus>>(
                        l => l.Count() == 1 && l.Contains(OrchestrationStatus.Completed))))
                .Throws(new Exception());

            var actualResult = await function.HistoryCleanUpActivity((DateTime.MinValue, DateTime.MaxValue), durableClientMock.Object);

            Assert.Null(actualResult);
        }

        [Fact]
        public async Task GivenHistoryCleanUpOrchestrator_WhenActivityTimeOut_ThenExecutionShouldBeContinuedForNextTimeRange()
        {
            var function = _fixture.Create<DurableFunctionHistoryCleanUpFunction>();

            var startScanningDate = DateTime.UtcNow.AddMonths(-1);
            var endScanningDate = startScanningDate.AddHours(400);
            _durableOrchestrationContextMock.Setup(x => x.GetInput<(DateTime, DateTime)>())
                .Returns((startScanningDate, endScanningDate));

            var exception = new FunctionFailedException("test",
                new FunctionTimeoutException("Test activity timeout exception"));
            _durableOrchestrationContextMock.Setup(x =>
                x.CallActivityAsync<PurgeHistoryResult>(nameof(DurableFunctionHistoryCleanUpFunction.HistoryCleanUpActivity),
                    It.IsAny<(DateTime, DateTime)>())).Throws(exception);

            await function.HistoryCleanUpOrchestrator(_durableOrchestrationContextMock.Object);

            _durableOrchestrationContextMock.Verify(x =>
                x.CallActivityAsync<PurgeHistoryResult>(nameof(DurableFunctionHistoryCleanUpFunction.HistoryCleanUpActivity),
                    It.IsAny<(DateTime, DateTime)>()), Times.Once);
            _durableOrchestrationContextMock.Verify(x => x.ContinueAsNew(It.IsAny<(DateTime, DateTime)>(), default), Times.Once);
        }

        [Fact]
        public async Task GivenHistoryCleanUpOrchestrator_WhenUnexpectedError_ThenShouldThrowsException()
        {
            var function = _fixture.Create<DurableFunctionHistoryCleanUpFunction>();

            var startScanningDate = DateTime.UtcNow.AddMonths(-1);
            var endScanningDate = startScanningDate.AddHours(400);
            
            _durableOrchestrationContextMock.Setup(x => x.GetInput<(DateTime, DateTime)>())
                .Returns((startScanningDate, endScanningDate));

           _durableOrchestrationContextMock.Setup(x =>
                x.CallActivityAsync<PurgeHistoryResult>(nameof(DurableFunctionHistoryCleanUpFunction.HistoryCleanUpActivity),
                    It.IsAny<(DateTime, DateTime)>())).Throws(new Exception("test message"));
           
           await Assert.ThrowsAsync<Exception>(async () => await function.HistoryCleanUpOrchestrator(_durableOrchestrationContextMock.Object));

            _durableOrchestrationContextMock.Verify(x =>
                x.CallActivityAsync<PurgeHistoryResult>(nameof(DurableFunctionHistoryCleanUpFunction.HistoryCleanUpActivity),
                    It.IsAny<(DateTime, DateTime)>()), Times.Once);
            _durableOrchestrationContextMock.Verify(x => x.ContinueAsNew(It.IsAny<(DateTime, DateTime)>(), default), Times.Never);
        }
    }
}
