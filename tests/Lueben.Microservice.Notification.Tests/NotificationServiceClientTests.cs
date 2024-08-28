using AutoFixture;
using AutoFixture.AutoMoq;
using System.Web;
using Lueben.Microservice.Notification.Constants;
using Lueben.Microservice.Notification.Models;
using Lueben.Microservice.Notification.Options;
using Lueben.Microservice.RestSharpClient.Abstractions;
using Moq;
using RestSharp;
using Xunit;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.Notification.Tests
{
    public class NotificationServiceClientTests
    {
        private const string BaseUrl = "https://test.com/notification";
        private const string LuebenRequestConsumer = "testConsumer";
        private readonly IFixture _fixture;
        private readonly Mock<IRestSharpClient> _restSharpClientMock;
        private readonly NotificationServiceClient _client;

        public NotificationServiceClientTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var options = new NotificationClientOptions()
            {
                BaseUrl = NotificationServiceClientTests.BaseUrl,
                LuebenRequestConsumer = NotificationServiceClientTests.LuebenRequestConsumer
            };
            var mockOptions = _fixture.Create<Mock<IOptionsSnapshot<NotificationClientOptions>>>();
            mockOptions.Setup(s => s.Value).Returns(options);
            _fixture.Inject(mockOptions.Object);

            _restSharpClientMock = _fixture.Freeze<Mock<IRestSharpClient>>();
            var restSharpClientFactoryMock = _fixture.Freeze<Mock<IRestSharpClientFactory>>();
            restSharpClientFactoryMock.Setup(m => m.Create(It.IsAny<object>()))
                .Returns(_restSharpClientMock.Object);

            _client = _fixture.Create<NotificationServiceClient>();
        }

        [Fact]
        public void GivenSendNotificationAsync_WhenRequestDataIsNull_ThenArgumentNullExceptionIsThrown()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _client.SendNotificationAsync(null));
        }

        [Fact]
        public async Task GivenSendNotificationAsync_WhenExecuted_ThenTheResponseIsSuccessful()
        {
            var notificationRequestMock = _fixture.Create<NotificationRequest>();
            var createNotificationResponseMock = _fixture.Create<CreateNotificationResponse>();

            _restSharpClientMock.Setup(m => m.ExecuteRequestAsync<CreateNotificationResponse>(It.IsAny<RestRequest>()))
                .Callback<RestRequest>(r =>
                {
                    Assert.NotNull(r);
                    Assert.Equal(Method.Post, r.Method);
                    Assert.Equal($"{NotificationServiceClientTests.BaseUrl}/notificationRequest", HttpUtility.UrlDecode(r.Resource));
                    var body = r.Parameters.FirstOrDefault(x => x.Type == ParameterType.RequestBody);
                    Assert.NotNull(body);
                    Assert.IsType<NotificationRequest>(body.Value);
                    var consumerHeader = r.Parameters.FirstOrDefault(x => x.Type == ParameterType.HttpHeader && x.Name == Headers.LuebenRequestConsumer);
                    Assert.NotNull(consumerHeader);
                    Assert.Equal(NotificationServiceClientTests.LuebenRequestConsumer, consumerHeader.Value);
                    Assert.NotNull(r.Parameters.FirstOrDefault(x => x.Type == ParameterType.HttpHeader && x.Name == Headers.IdempotencyKey));
                })
                .ReturnsAsync(createNotificationResponseMock);

            var result = await _client.SendNotificationAsync(notificationRequestMock);

            Assert.NotNull(result);
            Assert.Equal(result.Id, createNotificationResponseMock.Id);
            _restSharpClientMock.Verify(m => m.ExecuteRequestAsync<CreateNotificationResponse>(It.IsAny<RestRequest>()), Times.Once);
        }

        [Fact]
        public void GivenGetChannelStatusAsync_WhenRequestDataIsNull_ThenArgumentNullExceptionIsThrown()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _client.GetChannelStatusAsync(null));
        }

        [Fact]
        public async Task GivenGetChannelStatusAsync_WhenExecuted_ThenTheResponseIsSuccessful()
        {
            var requestMock = _fixture.Create<NotificationChannelStatusRequest>();
            var responseMock = _fixture.Create<NotificationChannelStatusResponse>();

            _restSharpClientMock.Setup(m => m.ExecuteRequestAsync<NotificationChannelStatusResponse>(It.IsAny<RestRequest>()))
                .Callback<RestRequest>(r =>
                {
                    Assert.NotNull(r);
                    Assert.Equal(Method.Get, r.Method);
                    Assert.Equal($"{NotificationServiceClientTests.BaseUrl}/notificationRequest/{{notificationId}}/status/{{channelType}}", HttpUtility.UrlDecode(r.Resource));
                    var consumerHeader = r.Parameters.FirstOrDefault(x => x.Type == ParameterType.HttpHeader && x.Name == Headers.LuebenRequestConsumer);
                    Assert.NotNull(consumerHeader);
                    Assert.Equal(NotificationServiceClientTests.LuebenRequestConsumer, consumerHeader.Value);
                    var notificationIdParameter = r.Parameters.FirstOrDefault(x => x.Type == ParameterType.UrlSegment && x.Name == "notificationId");
                    Assert.NotNull(notificationIdParameter);
                    Assert.Equal(requestMock.NotificationId, notificationIdParameter.Value);
                    var channelTypeParameter = r.Parameters.FirstOrDefault(x => x.Type == ParameterType.UrlSegment && x.Name == "channelType");
                    Assert.NotNull(channelTypeParameter);
                    Assert.Equal(requestMock.ChannelType, channelTypeParameter.Value);
                })
                .ReturnsAsync(responseMock);

            var result = await _client.GetChannelStatusAsync(requestMock);

            Assert.NotNull(result);
            Assert.Equal(result.Id, responseMock.Id);
            _restSharpClientMock.Verify(m => m.ExecuteRequestAsync<NotificationChannelStatusResponse>(It.IsAny<RestRequest>()), Times.Once);
        }

        [Fact]
        public async Task GivenGetChannelStatusesByIdAsync_WhenExecuted_ThenTheResponseIsSuccessful()
        {
            var notificationId = _fixture.Create<Guid>();
            var responseMock = _fixture.CreateMany<NotificationChannelStatusResponse>(3).ToList();

            _restSharpClientMock.Setup(m => m.ExecuteRequestAsync<List<NotificationChannelStatusResponse>>(It.IsAny<RestRequest>()))
                .Callback<RestRequest>(r =>
                {
                    Assert.NotNull(r);
                    Assert.Equal(Method.Get, r.Method);
                    Assert.Equal($"{NotificationServiceClientTests.BaseUrl}/notificationRequest/{{notificationId}}/status", HttpUtility.UrlDecode(r.Resource));
                    var consumerHeader = r.Parameters.FirstOrDefault(x => x.Type == ParameterType.HttpHeader && x.Name == Headers.LuebenRequestConsumer);
                    Assert.NotNull(consumerHeader);
                    Assert.Equal(NotificationServiceClientTests.LuebenRequestConsumer, consumerHeader.Value);
                    var notificationIdParameter = r.Parameters.FirstOrDefault(x => x.Type == ParameterType.UrlSegment && x.Name == "notificationId");
                    Assert.NotNull(notificationIdParameter);
                    Assert.Equal(notificationId.ToString(), notificationIdParameter.Value);
                })
                .ReturnsAsync(responseMock);

            var result = await _client.GetChannelStatusesByIdAsync(notificationId);

            Assert.NotNull(result);
            _restSharpClientMock.Verify(m => m.ExecuteRequestAsync<List<NotificationChannelStatusResponse>>(It.IsAny<RestRequest>()), Times.Once);
        }

        [Fact]
        public async Task GivenGetNotificationByIdAsync_WhenExecuted_ThenTheResponseIsSuccessful()
        {
            var notificationId = _fixture.Create<Guid>();
            var responseMock = _fixture.Create<NotificationResponse>();

            _restSharpClientMock.Setup(m => m.ExecuteRequestAsync<NotificationResponse>(It.IsAny<RestRequest>()))
                .Callback<RestRequest>(r =>
                {
                    Assert.NotNull(r);
                    Assert.Equal(Method.Get, r.Method);
                    Assert.Equal($"{NotificationServiceClientTests.BaseUrl}/notificationRequest/{{notificationId}}", HttpUtility.UrlDecode(r.Resource));
                    var consumerHeader = r.Parameters.FirstOrDefault(x => x.Type == ParameterType.HttpHeader && x.Name == Headers.LuebenRequestConsumer);
                    Assert.NotNull(consumerHeader);
                    Assert.Equal(NotificationServiceClientTests.LuebenRequestConsumer, consumerHeader.Value);
                    var notificationIdParameter = r.Parameters.FirstOrDefault(x => x.Type == ParameterType.UrlSegment && x.Name == "notificationId");
                    Assert.NotNull(notificationIdParameter);
                    Assert.Equal(notificationId.ToString(), notificationIdParameter.Value);
                })
                .ReturnsAsync(responseMock);

            var result = await _client.GetNotificationByIdAsync(notificationId);

            Assert.NotNull(result);
            Assert.Equal(result.Id, responseMock.Id);
            _restSharpClientMock.Verify(m => m.ExecuteRequestAsync<NotificationResponse>(It.IsAny<RestRequest>()), Times.Once);
        }
    }
}
