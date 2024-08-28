using Lueben.Microservice.Mediator.Tests.Models;
using Moq;

namespace Lueben.Microservice.Mediator.Tests
{
    public class MediatorTests
    {
        private readonly Mock<IHandlerProvider> _handlerProviderMock;
        private readonly IMediator _mediator;

        public MediatorTests()
        {
            _handlerProviderMock = new Mock<IHandlerProvider>();
            _mediator = new Mediator(_handlerProviderMock.Object);
        }

        [Fact]
        public async Task GivenMediator_WhenSendIsCalled_ThenExpectedHandlerIsExecuted()
        {
            var request = new TestRequest();
            var expectedResult = new Unit();
            var handlerMock = new Mock<IRequestHandler<TestRequest, Unit>>();
            handlerMock.Setup(x => x.Handle(request)).ReturnsAsync(expectedResult);

            _handlerProviderMock.Setup(x => x.ProvideHandler<TestRequest, Unit>()).Returns(handlerMock.Object);

            var result = await _mediator.Send<TestRequest, Unit>(request);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GivenMediator_WhenPublishIsCalled_ThenExpectedHandlerIsExecuted()
        {
            var request = new TestNotification();
            var handlerMock = new Mock<INotificationHandler<TestNotification>>();
            _handlerProviderMock.Setup(x => x.GetAllNotificationHandlers<TestNotification>())
                .Returns(new List<INotificationHandler<TestNotification>> { handlerMock.Object });

            await _mediator.Publish(request);

            handlerMock.Verify(x => x.Handle(request), Times.Once);
        }
    }
}