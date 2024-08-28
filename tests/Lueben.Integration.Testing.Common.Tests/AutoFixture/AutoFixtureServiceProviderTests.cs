using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using Lueben.Integration.Testing.Common.AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Lueben.Integration.Testing.Common.Tests.AutoFixture
{
    public class AutoFixtureServiceProviderTests
    {
        private readonly IFixture _fixture;

        public AutoFixtureServiceProviderTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public void GivenAutoFixtureServiceProvider_WhenServiceIsNotRegistered_ThenFixtureShouldCreateIt()
        {
            var fixtureMock = new Mock<IFixture>();

            var serviceProvider = new AutoFixtureServiceProvider(new ServiceCollection().BuildServiceProvider(), fixtureMock.Object);

            serviceProvider.GetService(typeof(TestService));

            fixtureMock.Verify(x => x.Create(typeof(TestService), It.IsAny<ISpecimenContext>()), Times.Once);
        }

        [Fact]
        public void GivenAutoFixtureServiceProvider_WhenServiceRegistered_ThenShouldBeResolvedFromServiceProvider()
        {
            var fixtureMock = new Mock<IFixture>();

            var sp = new ServiceCollection().AddSingleton<TestService>().BuildServiceProvider();
            var serviceProvider = new AutoFixtureServiceProvider(sp, fixtureMock.Object);

            serviceProvider.GetService(typeof(TestService));

            fixtureMock.Verify(x => x.Create(It.IsAny<Type>(), It.IsAny<ISpecimenContext>()), Times.Never);
        }
    }

    public class TestService
    {
    }
}
