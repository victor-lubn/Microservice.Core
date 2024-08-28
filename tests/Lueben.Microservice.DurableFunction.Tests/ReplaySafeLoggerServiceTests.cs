using Lueben.Microservice.ApplicationInsights;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Xunit;

namespace Lueben.Microservice.DurableFunction.Tests
{
    public class ReplaySafeLoggerServiceTests
    {
        private readonly Mock<IDurableOrchestrationContext> _contextMock;
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly ReplaySafeLoggerService _replaySafeLoggerService;

        public ReplaySafeLoggerServiceTests()
        {
            _contextMock = new Mock<IDurableOrchestrationContext>();
            _loggerServiceMock = new Mock<ILoggerService>();

            _replaySafeLoggerService = new ReplaySafeLoggerService(_loggerServiceMock.Object, _contextMock.Object);
        }

        [Theory]
        [InlineData(true, 0)]
        [InlineData(false, 1)]
        public void GivenReplaySafeLoggerService_WhenParametersArePassed_ThenShouldLogEventAsExpected(bool isReplaying, int times)
        {
            _contextMock.Setup(m => m.IsReplaying).Returns(isReplaying);

            _replaySafeLoggerService.LogEvent("foo");

            _loggerServiceMock.Verify(m => m.LogEvent("foo", null, null), Times.Exactly(times));
        }
    }
}
