using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using RestSharp;
using Xunit;

namespace Lueben.Microservice.RestSharpClient.Tests
{
    public class RestSharpClientTests : IDisposable
    {
        private readonly IFixture _fixture;
        private readonly Mock<ILogger<RestSharpClient>> _loggerMock;
        private readonly RestSharpClient _restSharpClient;
        private bool _disposed;

        public RestSharpClientTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            _loggerMock = new Mock<ILogger<RestSharpClient>>();

            
            var client = new HttpClient(new HttpClientHandler());

            _restSharpClient = new RestSharpClient(client, _loggerMock.Object);
        }

        [Fact]
        public async Task GivenRestSharpClient_WhenExecuteRequestAsyncIsCalled_ButResourceDoesNotExist_ThenShouldLogErrorWithRequestUrl()
        {
            var url = $"http://{_fixture.Create<string>()}.com";
            var request = new RestRequest(url);
            request.AddHeader(KnownHeaders.Authorization, "Bearer test-token");

            await Assert.ThrowsAsync<RestClientApiException>(async () => await _restSharpClient.ExecuteRequestAsync(request));

            _loggerMock.Verify(
                m => m.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().StartsWith($"Request to {url} failed") && !v.ToString().Contains($"{KnownHeaders.Authorization}=")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GivenRestSharpClient_WhenExecuteRequestAsyncGenericIsCalled_ButResourceDoesNotExist_ThenShouldLogErrorWithRequestUrl()
        {
            var url = $"http://{_fixture.Create<string>()}.com";
            var request = new RestRequest(url);
            request.AddHeader(KnownHeaders.Authorization, "Bearer test-token");

            await Assert.ThrowsAsync<RestClientApiException>(async () => await _restSharpClient.ExecuteRequestAsync<object>(request));

            _loggerMock.Verify(
                m => m.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().StartsWith($"Request to {url} failed") && !v.ToString().Contains($"{KnownHeaders.Authorization}=")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GivenRestSharpClient_WhenRequestIsSuccessful_ThenExceptionIsNotThrown()
        {
            var url = $"https://google.com";
            var request = new RestRequest(url);

            await _restSharpClient.ExecuteRequestAsync(request);

            _loggerMock.Verify(
                m => m.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().StartsWith($"Request to {url} failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task GivenRestSharpClient_WhenActivityExists_ThenRequestIdHeaderIsSet()
        {
            using (new Activity("sample").Start())
            {
                var url = $"https://google.com";
                var request = new RestRequest(url);

                await _restSharpClient.ExecuteRequestAsync(request);
                Assert.NotNull(request.Parameters.First(x => x.Type == ParameterType.HttpHeader && x.Name == "Request-Id"));
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _restSharpClient.Dispose();
            }

            _disposed = true;
        }
    }
}
