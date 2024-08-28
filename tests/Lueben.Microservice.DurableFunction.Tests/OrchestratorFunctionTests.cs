using System;
using System.Net;
using System.Threading.Tasks;
using DurableTask.Core;
using Grpc.Core;
using Lueben.Microservice.ApplicationInsights;
using Lueben.Microservice.DurableFunction.Exceptions;
using Lueben.Microservice.DurableFunction.Tests.Models;
using Lueben.Microservice.RestSharpClient;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lueben.Microservice.DurableFunction.Tests
{
    public class OrchestratorFunctionTests
    {
        protected readonly TelemetryConfiguration _telemetryConfiguration;
        protected readonly Mock<ILogger<OrchestratorFunction<TestClass>>> _loggerMock;
        protected readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly Mock<IDurableOrchestrationContext> _contextMock;

        public OrchestratorFunctionTests()
        {
            _loggerMock = new Mock<ILogger<OrchestratorFunction<TestClass>>>();
            _loggerServiceMock = new Mock<ILoggerService>();
            _telemetryConfiguration = new TelemetryConfiguration();
            _contextMock = new Mock<IDurableOrchestrationContext>();
            CorrelationTraceContext.Current = new W3CTraceContext();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void GivenOrchestratorFunction_WhenInstantiatedAndNotAllParamsArePassed_ThenArgumentNullExceptionIsThrown(int nullableParamOrder)
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TestOrchestratorFunction(nullableParamOrder == 1 ? null : _telemetryConfiguration,
                    nullableParamOrder == 2 ? null : _loggerMock.Object,
                    nullableParamOrder == 3 ? null : _loggerServiceMock.Object));
        }

        [Fact]
        public async Task GivenHandleErrors_WhenIsCalled_ThenProcessActivitiesIsExecuted()
        {
            var function = new TestOrchestratorFunction(_telemetryConfiguration, _loggerMock.Object, _loggerServiceMock.Object);
            _contextMock.Setup(x => x.GetInput<TestClass>()).Returns(new TestClass());

            await function.HandleErrors(_contextMock.Object);

            Assert.NotEmpty(function.TestProperty);
        }

        [Fact]
        public async Task GivenHandleErrors_WhenIsCalledAndExceptionOccurred_ThenIncorrectEventDataExceptionIsNotReThrown()
        {
            var function = new TestOrchestratorFunction(_telemetryConfiguration, _loggerMock.Object, _loggerServiceMock.Object);
            _contextMock.Setup(x => x.GetInput<TestClass>()).Throws(
                new FunctionFailedException("test", new IncorrectEventDataException("test", new Exception())));

            await function.HandleErrors(_contextMock.Object);

            Assert.Null(function.TestProperty);
        }

        [Fact]
        public async Task GivenHandleErrors_WhenIsCalledAndExceptionOccurred_ThenEventDataProcessFailureExceptionIsReThrown()
        {
            var function = new TestOrchestratorFunction(_telemetryConfiguration, _loggerMock.Object, _loggerServiceMock.Object);
            _contextMock.Setup(x => x.GetInput<TestClass>()).Throws(
                new FunctionFailedException("test", new EventDataProcessFailureException("test", new Exception())));

            ;
            await Assert.ThrowsAsync<FunctionFailedException>(async () => await function.HandleErrors(_contextMock.Object));

            Assert.Null(function.TestProperty);
        }

        [Fact]
        public async Task GivenHandleErrors_WhenIsCalledAndExceptionOccurred_ThenExceptionIsReThrown()
        {
            var function = new TestOrchestratorFunction(_telemetryConfiguration, _loggerMock.Object, _loggerServiceMock.Object);
            _contextMock.Setup(x => x.GetInput<TestClass>()).Throws(new EventDataProcessFailureException("test"));

            ;
            await Assert.ThrowsAsync<EventDataProcessFailureException>(async () => await function.HandleErrors(_contextMock.Object));

            Assert.Null(function.TestProperty);
        }

        [Fact]
        public async Task GivenHandleRestClientExceptions_WhenIsCalledAndExceptionOccurred_ThenExceptionIsHandledAsExpected()
        {
            var function = new TestOrchestratorFunction(_telemetryConfiguration, _loggerMock.Object, _loggerServiceMock.Object);
            var data = new TestClass();

            await Assert.ThrowsAsync<EventDataProcessFailureException>(async () =>
                await function.HandleRestClientExceptions<ErrorResponse>(
                    (input) => throw new IncorrectEventDataException("test"), data));
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.UnprocessableEntity)]
        public async Task GivenHandleRestClientExceptions_WhenIsCalledAndRestClientApiExceptionOccurred_ThenExceptionIsHandledAsExpected(HttpStatusCode statusCode)
        {
            var function = new TestOrchestratorFunction(_telemetryConfiguration, _loggerMock.Object, _loggerServiceMock.Object);
            var data = new TestClass();

            await Assert.ThrowsAsync<IncorrectEventDataException>(async () =>
                await function.HandleRestClientExceptions<ErrorResponse>(
                    (input) => throw new RestClientApiException("test", "test error", statusCode, new Exception()), data));
        }

        [Fact]
        public async Task GivenHandleRestClientExceptions_WhenIsCalledAndTransientRestClientApiExceptionOccurred_ThenExceptionIsHandledAsExpected()
        {
            var function = new TestOrchestratorFunction(_telemetryConfiguration, _loggerMock.Object, _loggerServiceMock.Object);
            var data = new TestClass();

            await Assert.ThrowsAsync<EventDataProcessFailureException>(async () =>
                await function.HandleRestClientExceptions<ErrorResponse, TestClass>(
                    (input) => throw new RestClientApiException("test", "test error", HttpStatusCode.InternalServerError, new Exception()), data));
        }
    }

    public class TestOrchestratorFunction: OrchestratorFunction<TestClass>
    {
        public TestOrchestratorFunction(TelemetryConfiguration telemetryConfiguration, ILogger<OrchestratorFunction<TestClass>> logger, ILoggerService loggerService) : base(telemetryConfiguration, logger, loggerService)
        {
        }

        public string TestProperty { get; set; }

        public override Task ProcessActivities(IDurableOrchestrationContext context, TestClass eventData)
        {
            TestProperty = "test";

            return Task.CompletedTask;
        }
    }
}
