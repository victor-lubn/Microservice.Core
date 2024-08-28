using System.Threading.Tasks;
using Lueben.Microservice.DurableFunction.Extensions;
using Lueben.Microservice.DurableFunction.Tests.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Xunit;

namespace Lueben.Microservice.DurableFunction.Tests
{
    public class DurableClientExtensionsTests
    {
        [Fact]
        public async Task GivenStartNewAsyncWithRetry_WhenCalled_ThenExpectedMethodIsExecuted()
        {
            var clientMock = new Mock<IDurableOrchestrationClient>();
            var input = new TestClass();
            var orchName = "test";

            await clientMock.Object.StartNewAsyncWithRetry(orchName, input);

            clientMock.Verify(x => x.StartNewAsync(orchName, It.Is<RetryData<TestClass>>(d => d.Input == input)));
        }
    }
}
