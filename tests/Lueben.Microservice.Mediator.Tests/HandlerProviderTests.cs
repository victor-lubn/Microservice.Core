using Lueben.Microservice.Mediator.Tests.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Lueben.Microservice.Mediator.Tests
{
    public class HandlerProviderTests
    {
        [Fact]
        public void GivenHandlerProvider_WhenProvideHandlerIsCalled_ThenExpectedServiceIsReturned()
        {
            var handlerMock = new Mock<IRequestHandler<TestRequest, Unit>>();
            var services = new ServiceCollection();
            services.AddSingleton(handlerMock.Object);
            var serviceProvider = services.BuildServiceProvider();
            var handlerProvider = new HandlerProvider(serviceProvider);

            var result = handlerProvider.ProvideHandler<TestRequest, Unit>();

            Assert.NotNull(result);
            Assert.Equal(handlerMock.Object, result);
        }

        [Fact]
        public void GivenHandlerProvider_WhenGetAllNotificationHandlersIsCalled_ThenExpectedServiceIsReturned()
        {
            var handlerMock = new Mock<INotificationHandler<TestNotification>>();
            var services = new ServiceCollection();
            services.AddSingleton(handlerMock.Object);
            var serviceProvider = services.BuildServiceProvider();
            var handlerProvider = new HandlerProvider(serviceProvider);

            var result = handlerProvider.GetAllNotificationHandlers<TestNotification>();
            
            Assert.Single(result);
            Assert.Equal(handlerMock.Object, result.First());
        }
    }
}
