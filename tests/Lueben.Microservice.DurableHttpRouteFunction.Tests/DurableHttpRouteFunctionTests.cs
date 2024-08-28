using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DurableTask.Core;
using Lueben.Microservice.DurableFunction;
using Lueben.Microservice.Serialization;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Lueben.Microservice.DurableHttpRouteFunction.Tests
{
    public class DurableHttpRouteFunctionTests
    {
        private readonly Mock<IOptions<DurableTaskOptions>> _durableTaskOptionsMock;
        private readonly Mock<ILogger<DurableHttpRouteFunction>> _mockLogger;
        private readonly Mock<IDurableOrchestrationContext> _durableOrchestrationContextMock;
        private readonly Mock<IOptionsSnapshot<WorkflowOptions>> _workFlowOptionsSnapshot;
        private readonly IConfiguration _configuration;

        public DurableHttpRouteFunctionTests()
        {
            JsonConvert.DefaultSettings = FunctionJsonSerializerSettingsProvider.CreateSerializerSettings;
            _mockLogger = new Mock<ILogger<DurableHttpRouteFunction>>();
            _durableOrchestrationContextMock = new Mock<IDurableOrchestrationContext>();
            _durableOrchestrationContextMock.Setup(x => x.GetInput<RetryData<HttpRouteInput>>())
                .Returns(new RetryData<HttpRouteInput>(new HttpRouteInput
                {
                    HandlerOptions = new HttpEventHandlerOptions
                    {
                        ServiceUrl = "https://mock.com",
                        FunctionKey = "mock"
                    },
                    Payload = "eventdata"
                }));
            _workFlowOptionsSnapshot = new Mock<IOptionsSnapshot<WorkflowOptions>>();
            _workFlowOptionsSnapshot.Setup(x => x.Value).Returns(new WorkflowOptions
            {
                MaxEventRetryCount = 2
            });

            
            _durableTaskOptionsMock = new Mock<IOptions<DurableTaskOptions>>();
            _durableTaskOptionsMock.Setup(x => x.Value).Returns(new DurableTaskOptions
            {
                Tracing = new TraceOptions
                {
                    DistributedTracingEnabled = true,
                    Version = DurableDistributedTracingVersion.V1
                }
            });


            var config = new Dictionary<string, string>()
            {
                ["HttpEventHandlerOptions:Foo:ServiceUrl"] = "https://mock.com",
                ["HttpEventHandlerOptions:Foo:FunctionKey"] = "mock_function_key",
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            CorrelationTraceContext.Current = new W3CTraceContext();
        }

        [Fact]
        public async Task GivenEventOrchestrator_WhenMessageIsRouted_ThenOrchestrationIsFinished()
        {
            _durableOrchestrationContextMock.Setup(x => x.CallHttpAsync(It.IsAny<DurableHttpRequest>()))
                .Returns(Task.FromResult(new DurableHttpResponse(HttpStatusCode.OK, null, string.Empty)));
            var function = new DurableHttpRouteFunction(
                TelemetryConfiguration.CreateDefault(),
                _mockLogger.Object,
                _workFlowOptionsSnapshot.Object,
                _durableTaskOptionsMock.Object,
                _configuration);

            await function.Orchestrator(_durableOrchestrationContextMock.Object);

            _durableOrchestrationContextMock.Verify(x => x.CallHttpAsync(It.IsAny<DurableHttpRequest>()), Times.Once);
            _durableOrchestrationContextMock.Verify(x => x.ContinueAsNew(It.IsAny<object>(), false), Times.Never);
        }

        [Fact]
        public async Task GivenEventOrchestrator_WhenServiceReturnsBadRequest_ThenEventIncorrectDataLogEventIsCreated()
        {
            _durableOrchestrationContextMock.Setup(x => x.CallHttpAsync(It.IsAny<DurableHttpRequest>()))
                .Returns(Task.FromResult(new DurableHttpResponse(HttpStatusCode.BadRequest, null, "BadRequest.")));
            var function = new DurableHttpRouteFunction(
                TelemetryConfiguration.CreateDefault(),
                _mockLogger.Object,
                _workFlowOptionsSnapshot.Object,
                _durableTaskOptionsMock.Object,
                _configuration);

            await function.Orchestrator(_durableOrchestrationContextMock.Object);

            _durableOrchestrationContextMock.Verify(x => x.CallHttpAsync(It.IsAny<DurableHttpRequest>()), Times.Once);
            _durableOrchestrationContextMock.Verify(x => x.ContinueAsNew(It.IsAny<object>(), false), Times.Never);
        }

        [Fact]
        public async Task GivenEventOrchestrator_WhenServiceReturnsNotBadRequestError_ThenRetryIsExecutedAndEventReturnedEventIsCreated()
        {
            _durableOrchestrationContextMock.Setup(x => x.CallHttpAsync(It.IsAny<DurableHttpRequest>()))
                .Returns(Task.FromResult(new DurableHttpResponse(HttpStatusCode.ServiceUnavailable, null, "ServiceUnavailable")));
            var function = new DurableHttpRouteFunction(
                TelemetryConfiguration.CreateDefault(),
                _mockLogger.Object,
                _workFlowOptionsSnapshot.Object,
                _durableTaskOptionsMock.Object,
                _configuration);

            await function.Orchestrator(_durableOrchestrationContextMock.Object);

            _durableOrchestrationContextMock.Verify(x => x.CallHttpAsync(It.IsAny<DurableHttpRequest>()), Times.Once);
            _durableOrchestrationContextMock.Verify(x => x.CreateTimer(It.IsAny<DateTime>(), CancellationToken.None), Times.Once);
            _durableOrchestrationContextMock.Verify(x => x.ContinueAsNew(It.IsAny<object>(), false), Times.Once);
        }

        [Fact]
        public async Task GivenEventOrchestrator_WhenServiceReturnsNotBadRequestErrorAndRetryLimitExceeded_ThenNoRetryIsExecutedAndEventRetryLimitExceededEventIsCreated()
        {
            _durableOrchestrationContextMock.Setup(x => x.GetInput<RetryData<HttpRouteInput>>())
                .Returns(new RetryData<HttpRouteInput>(new HttpRouteInput
                {
                    HandlerOptions = new HttpEventHandlerOptions
                    {
                        ServiceUrl = "https://mock.com",
                        FunctionKey = "mock"
                    },
                    Payload = "eventData"
                })
                {
                    Retry = _workFlowOptionsSnapshot.Object.Value.MaxEventRetryCount
                });

            _durableOrchestrationContextMock.Setup(x => x.CallHttpAsync(It.IsAny<DurableHttpRequest>()))
                .Returns(Task.FromResult(new DurableHttpResponse(HttpStatusCode.ServiceUnavailable, null, "ServiceUnavailable")));
            var function = new DurableHttpRouteFunction(
                TelemetryConfiguration.CreateDefault(),
                _mockLogger.Object,
                _workFlowOptionsSnapshot.Object,
                _durableTaskOptionsMock.Object,
                _configuration);

            await function.Orchestrator(_durableOrchestrationContextMock.Object);

            _durableOrchestrationContextMock.Verify(x => x.CallHttpAsync(It.IsAny<DurableHttpRequest>()), Times.Once);
            _durableOrchestrationContextMock.Verify(x => x.CreateTimer(It.IsAny<DateTime>(), CancellationToken.None), Times.Never);
            _durableOrchestrationContextMock.Verify(x => x.ContinueAsNew(It.IsAny<object>(), false), Times.Never);
        }

        [Fact]
        public async Task GivenEventOrchestrator_WhenRequestTimeOut_ThenRetryIsExecuted()
        {
            _durableOrchestrationContextMock.Setup(x => x.GetInput<RetryData<HttpRouteInput>>())
                .Returns(new RetryData<HttpRouteInput>(new HttpRouteInput
                {
                    HandlerOptions = new HttpEventHandlerOptions
                    {
                        ServiceUrl = "https://mock.com",
                        FunctionKey = "mock"
                    },
                    Payload = "eventData"
                })
                {
                    Retry = _workFlowOptionsSnapshot.Object.Value.MaxEventRetryCount
                });
            _durableOrchestrationContextMock.Setup(x => x.CallHttpAsync(It.IsAny<DurableHttpRequest>()))
                .Throws<TimeoutException>();
            var function = new DurableHttpRouteFunction(
                TelemetryConfiguration.CreateDefault(),
                _mockLogger.Object,
                _workFlowOptionsSnapshot.Object,
                _durableTaskOptionsMock.Object,
                _configuration);

            await function.Orchestrator(_durableOrchestrationContextMock.Object);

            _durableOrchestrationContextMock.Verify(x => x.CallHttpAsync(It.IsAny<DurableHttpRequest>()), Times.Once);
            _durableOrchestrationContextMock.Verify(x => x.CreateTimer(It.IsAny<DateTime>(), CancellationToken.None), Times.Once);
            _durableOrchestrationContextMock.Verify(x => x.ContinueAsNew(It.IsAny<object>(), false), Times.Once);
        }

        [Fact]
        public async Task GivenEventOrchestrator_WhenConfigurationOptionIsPassedInsteadOfFunctionKey_ThenShouldResolveFunctionKeyFromConfiguration()
        {
            _durableOrchestrationContextMock.Setup(x => x.GetInput<RetryData<HttpRouteInput>>())
                .Returns(new RetryData<HttpRouteInput>(new HttpRouteInput
                {
                    HandlerOptions = new HttpEventHandlerOptions
                    {
                        ServiceUrl = "https://mock.com",
                        FunctionKey = "HttpEventHandlerOptions:Foo:FunctionKey",
                    },
                    Payload = "eventData",
                })
                {
                    Retry = _workFlowOptionsSnapshot.Object.Value.MaxEventRetryCount,
                });

            _durableOrchestrationContextMock.Setup(x => x.CallHttpAsync(It.IsAny<DurableHttpRequest>()))
                .Throws<TimeoutException>();

            var function = new DurableHttpRouteFunction(
                TelemetryConfiguration.CreateDefault(),
                _mockLogger.Object,
                _workFlowOptionsSnapshot.Object,
                _durableTaskOptionsMock.Object,
                _configuration);

            await function.Orchestrator(_durableOrchestrationContextMock.Object);

            _durableOrchestrationContextMock.Verify(
                x => x.CallHttpAsync(
                    It.Is<DurableHttpRequest>(r => r.Headers["x-functions-key"] == _configuration["HttpEventHandlerOptions:Foo:FunctionKey"])),
                Times.Once);
        }
    }
}