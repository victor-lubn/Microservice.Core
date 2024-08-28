using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lueben.Microservice.CircuitBreaker.Tests.Extensions
{
    internal static class DurableCircuitBreakerClientMockExtensions
    {
        public static void SetupIsExecutionPermitted(this Mock<IDurableCircuitBreakerClient> durableCircuitBreakerClientMock, string circuitBreakerId, bool value)
        {
            durableCircuitBreakerClientMock
                .Setup(x => x.IsExecutionPermitted(circuitBreakerId, It.IsAny<ILogger>(), It.IsAny<IDurableClient>(), It.IsAny<IConfiguration>()))
                .Returns(Task.FromResult(value));
        }

        public static void VerifyOnlyOneSuccess(this Mock<IDurableCircuitBreakerClient> durableCircuitBreakerClientMock, string circuitBreakerId)
        {
            durableCircuitBreakerClientMock
                .Verify(m => m.RecordSuccess(circuitBreakerId, It.IsAny<ILogger>(), It.IsAny<IDurableClient>()), Times.Once);
            durableCircuitBreakerClientMock
                .Verify(m => m.RecordFailure(circuitBreakerId, It.IsAny<ILogger>(), It.IsAny<IDurableClient>()), Times.Never);
        }

        public static void VerifyOnlyOneFailure(this Mock<IDurableCircuitBreakerClient> durableCircuitBreakerClientMock, string circuitBreakerId)
        {
            durableCircuitBreakerClientMock
                .Verify(m => m.RecordSuccess(circuitBreakerId, It.IsAny<ILogger>(), It.IsAny<IDurableClient>()), Times.Never);
            durableCircuitBreakerClientMock
                .Verify(m => m.RecordFailure(circuitBreakerId, It.IsAny<ILogger>(), It.IsAny<IDurableClient>()), Times.Once);
        }
    }
}