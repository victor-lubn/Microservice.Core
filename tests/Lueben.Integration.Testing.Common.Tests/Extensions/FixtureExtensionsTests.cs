using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using Lueben.Integration.Testing.Common.AutoFixture;
using Lueben.Integration.Testing.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lueben.Integration.Testing.Common.Tests.Extensions
{
    public class FixtureExtensionsTests
    {
        private readonly IFixture _fixture;

        public FixtureExtensionsTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public void GivenFixture_WhenServiceIsRegisteredInFixture_ThenShouldResolveWithExpectedDependencies()
        {
            _fixture.ConfigureServices(services => services.AddSingleton<IServiceCollectionRegisteredDependency, ServiceCollectionRegisteredDependency>());

            _fixture.Register<IFixtureRegisteredDependency>(() => new FixtureRegisteredDependency());

            var service = _fixture.Create<TestService>();

            Assert.NotNull(service);
            Assert.NotNull(service.ServiceCollectionDependency);
            Assert.NotNull(service.FixtureRegisteredDependency);
            Assert.NotNull(service.UnregisteredDependency);
            Assert.IsType<ServiceCollectionRegisteredDependency>(service.ServiceCollectionDependency);
            Assert.IsType<FixtureRegisteredDependency>(service.FixtureRegisteredDependency);
            Assert.StartsWith("Mock", service.UnregisteredDependency.ToString());
        }

        [Fact]
        public void GivenFixture_WhenServiceIsRegisteredInServiceCollection_ThenShouldResolveWithExpectedDependencies()
        {
            _fixture.ConfigureServices(services =>
                services.AddSingleton<IServiceCollectionRegisteredDependency, ServiceCollectionRegisteredDependency>()
                        .AddSingleton<TestService>());

            _fixture.Register<IFixtureRegisteredDependency>(() => new FixtureRegisteredDependency());

            var service = _fixture.Create<TestService>();
            Assert.NotNull(service);
            Assert.NotNull(service.ServiceCollectionDependency);
            Assert.NotNull(service.FixtureRegisteredDependency);
            Assert.NotNull(service.UnregisteredDependency);
            Assert.IsType<ServiceCollectionRegisteredDependency>(service.ServiceCollectionDependency);
            Assert.IsType<FixtureRegisteredDependency>(service.FixtureRegisteredDependency);
            Assert.StartsWith("Mock", service.UnregisteredDependency.ToString());
        }

        [Fact]
        public void GivenAutoFixtureServiceProvider_WhenResolvingIServiceProvider_ThenShouldReturnItself()
        {
            _fixture.ConfigureServices(services =>
                services.AddSingleton<IServiceCollectionRegisteredDependency, ServiceCollectionRegisteredDependency>()
                        .AddSingleton<TestService>());

            _fixture.Register<IFixtureRegisteredDependency>(() => new FixtureRegisteredDependency());

            var serviceProvider = _fixture.Create<IServiceProvider>();
            Assert.NotNull(serviceProvider);
            Assert.IsType<AutoFixtureServiceProvider>(serviceProvider);

            var sp = serviceProvider.GetService(typeof(IServiceProvider));
            Assert.Same(serviceProvider, sp);
        }
    }

    public interface IServiceCollectionRegisteredDependency
    {
    }

    public class ServiceCollectionRegisteredDependency : IServiceCollectionRegisteredDependency
    {
    }

    public interface IFixtureRegisteredDependency
    {
    }

    public class FixtureRegisteredDependency : IFixtureRegisteredDependency
    {
    }

    public interface IUnregisteredDependency { }

    public class TestService
    {
        public TestService(
            IServiceProvider serviceProvider,
            IServiceCollectionRegisteredDependency serviceCollectionDependency,
            IFixtureRegisteredDependency fixtureRegisteredDependency,
            IUnregisteredDependency unregisteredDependency,
            InnerService innerService)
        {
            ServiceProvider = serviceProvider;
            ServiceCollectionDependency = serviceCollectionDependency;
            FixtureRegisteredDependency = fixtureRegisteredDependency;
            UnregisteredDependency = unregisteredDependency;
            InnerService = innerService;
        }

        public IServiceProvider ServiceProvider { get; }

        public IServiceCollectionRegisteredDependency ServiceCollectionDependency { get; }

        public IFixtureRegisteredDependency FixtureRegisteredDependency { get; }

        public IUnregisteredDependency UnregisteredDependency { get; }

        public InnerService InnerService { get; }
    }

    public class InnerService
    {
        public InnerService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }
    }
}
