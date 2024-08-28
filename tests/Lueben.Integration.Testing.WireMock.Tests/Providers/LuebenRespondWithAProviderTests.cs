using System;
using Lueben.Integration.Testing.WireMock.Providers;
using Moq;
using WireMock.ResponseProviders;
using WireMock.Server;
using Xunit;

namespace Lueben.Integration.Testing.WireMock.Tests.Providers
{
    public class LuebenRespondWithAProviderTests
    {
        private readonly Mock<IRespondWithAProvider> _respondProviderMock;

        public LuebenRespondWithAProviderTests()
        {
            _respondProviderMock= new Mock<IRespondWithAProvider>();
        }

        [Fact]
        public void GivenLuebenRespondWithAProvider_WhenRespondWithIsExecuted_ThenShouldCallDecoratedProvider_AndonAfterMappingRegisteredAction()
        {
            var responseProviderMock = new Mock<IResponseProvider>();
            var onAfterMappingRegisteredMock = new Mock<Action>();
            var provider = new LuebenRespondWithAProvider(_respondProviderMock.Object, onAfterMappingRegisteredMock.Object);

            provider.RespondWith(responseProviderMock.Object);

            _respondProviderMock.Verify(x => x.RespondWith(responseProviderMock.Object), Times.Once);
            onAfterMappingRegisteredMock.Verify(x => x.Invoke(), Times.Once);
        }
    }
}
