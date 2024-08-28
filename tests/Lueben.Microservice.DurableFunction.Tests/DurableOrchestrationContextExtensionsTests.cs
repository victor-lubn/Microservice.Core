using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using System.Threading.Tasks;
using Lueben.Microservice.DurableFunction.Extensions;
using Lueben.Microservice.DurableFunction.Tests.Models;
using Xunit;

namespace Lueben.Microservice.DurableFunction.Tests
{
    public class DurableOrchestrationContextExtensionsTests
    {
        [Fact]
        public async Task GivenCallDurableActivity_WhenIsCalled_ThenActivityWithRetryIsRun()
        {
            var contextMock = new Mock<IDurableOrchestrationContext>();
            var functionName = "testFunction";
            var data = new TestClass();
            var options = new WorkflowOptions
            {
                MaxEventRetryCount = 1,
                ActivityRetryInterval = "PT5S",
                ActivityMaxRetryInterval = "PT24H"
            };

            await contextMock.Object.CallDurableActivity(functionName, data, options);

            contextMock.Verify(x => x.CallActivityWithRetryAsync(functionName,
                It.Is<RetryOptions>(o =>
                    o.MaxNumberOfAttempts == options.MaxEventRetryCount && o.MaxRetryInterval.Milliseconds == options.ActivityMaxRetryIntervalTime.Milliseconds), data),
                Times.Once());
        }

        [Fact]
        public async Task GivenCallDurableActivity_WhenIsCalledWithoutOptions_ThenDefaultOptionsAreUsedToRetryActivity()
        {
            var contextMock = new Mock<IDurableOrchestrationContext>();
            var functionName = "testFunction";
            var data = new TestClass();

            await contextMock.Object.CallDurableActivity(functionName, data);

            contextMock.Verify(x => x.CallActivityWithRetryAsync(functionName,
                    It.Is<RetryOptions>(o => o.MaxNumberOfAttempts == int.MaxValue), data),
                Times.Once());
        }

        [Fact]
        public async Task GivenCallDurableActivityWithResult_WhenIsCalled_ThenActivityWithRetryIsRun()
        {
            var contextMock = new Mock<IDurableOrchestrationContext>();
            var functionName = "testFunction";
            var data = new TestClass();
            var options = new WorkflowOptions
            {
                MaxEventRetryCount = 1,
                ActivityRetryInterval = "PT5S",
                ActivityMaxRetryInterval = "PT2H"
            };

            await contextMock.Object.CallDurableActivity<TestClass>(functionName, data, options);

            contextMock.Verify(x => x.CallActivityWithRetryAsync<TestClass>(functionName,
                    It.Is<RetryOptions>(o =>
                        o.MaxNumberOfAttempts == options.MaxEventRetryCount && o.MaxRetryInterval.Milliseconds == options.ActivityMaxRetryIntervalTime.Milliseconds && o.FirstRetryInterval.Milliseconds == options.ActivityRetryIntervalTime.Milliseconds), data),
                Times.Once());
        }

        [Fact]
        public async Task GivenCallDurableActivityWithResult_WhenIsCalledWithoutOptions_ThenDefaultOptionsAreUsedToRetryActivity()
        {
            var contextMock = new Mock<IDurableOrchestrationContext>();
            var functionName = "testFunction";
            var data = new TestClass();

            await contextMock.Object.CallDurableActivity<TestClass>(functionName, data);

            contextMock.Verify(x => x.CallActivityWithRetryAsync<TestClass>(functionName,
                    It.Is<RetryOptions>(o => o.MaxNumberOfAttempts == int.MaxValue), data),
                Times.Once());
        }
    }
}
