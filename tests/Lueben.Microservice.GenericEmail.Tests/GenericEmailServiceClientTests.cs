using System;
using Lueben.Microservice.RestSharpClient.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Lueben.Microservice.GenericEmail.Tests
{
    public class GenericEmailServiceClientTests
    {
        private readonly Mock<IOptionsSnapshot<GenericEmailServiceOptions>> _genericEmailServiceOptionsMock = new Mock<IOptionsSnapshot<GenericEmailServiceOptions>>();
        private readonly Mock<IRestSharpClientFactory> _restClientFactoryMock = new Mock<IRestSharpClientFactory>();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void WhenGenericEmailServiceApiBaseUrlIsInvalid_ThenArgumentExceptionIsThrown(string baseUrl)
        {
            _genericEmailServiceOptionsMock.Setup(x => x.Value).Returns(new GenericEmailServiceOptions()
            {
                GenericEmailServiceApiBaseUrl = baseUrl,
            });
            Assert.Throws<ArgumentException>(() =>
                new GenericEmailServiceClient(_restClientFactoryMock.Object, _genericEmailServiceOptionsMock.Object));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void WhenApiUriIsInvalid_ThenArgumentExceptionIsThrown(string apiUri)
        {
            _genericEmailServiceOptionsMock.Setup(x => x.Value).Returns(new GenericEmailServiceOptions()
            {
                GenericEmailServiceApiBaseUrl = "baseUrl",
            });
            var client = new GenericEmailServiceClient(_restClientFactoryMock.Object, _genericEmailServiceOptionsMock.Object);

            Assert.Throws<ArgumentException>(() =>
                client.GetApiUrl(apiUri));
        }

        [Fact]
        public void WhenRequestDataIsInvalid_AndSendDynamicEmailIsCalled_ThenArgumentNullExceptionIsThrown()
        {
            _genericEmailServiceOptionsMock.Setup(x => x.Value).Returns(new GenericEmailServiceOptions()
            {
                GenericEmailServiceApiBaseUrl = "baseUrl",
            });
            var client = new GenericEmailServiceClient(_restClientFactoryMock.Object, _genericEmailServiceOptionsMock.Object);

            Assert.ThrowsAsync<ArgumentNullException>(() => client.SendDynamicEmail(null));
        }        
        
        [Fact]
        public void WhenRequestDataIsInvalid_AndSendEmailIsCalled_ThenArgumentNullExceptionIsThrown()
        {
            _genericEmailServiceOptionsMock.Setup(x => x.Value).Returns(new GenericEmailServiceOptions()
            {
                GenericEmailServiceApiBaseUrl = "baseUrl",
            });
            var client = new GenericEmailServiceClient(_restClientFactoryMock.Object, _genericEmailServiceOptionsMock.Object);

            Assert.ThrowsAsync<ArgumentNullException>(() => client.SendEmail(null));
        }        
        
        [Fact]
        public void WhenRequestDataIsInvalid_AndSendOnBehalfOfServiceEmailIsCalled_ThenArgumentNullExceptionIsThrown()
        {
            _genericEmailServiceOptionsMock.Setup(x => x.Value).Returns(new GenericEmailServiceOptions()
            {
                GenericEmailServiceApiBaseUrl = "baseUrl",
            });
            var client = new GenericEmailServiceClient(_restClientFactoryMock.Object, _genericEmailServiceOptionsMock.Object);

            Assert.ThrowsAsync<ArgumentNullException>(() => client.SendOnBehalfOfServiceEmail(null));
        }

        [Fact]
        public void WhenRestSharpFactoryIsNull_ThenArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GenericEmailServiceClient(null, _genericEmailServiceOptionsMock.Object));
        }
    }
}