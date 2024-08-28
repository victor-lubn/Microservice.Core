using System;
using System.Linq;
using AutoFixture;
using Lueben.Integration.Testing.Common.AutoFixture;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Integration.Testing.Common.Extensions
{
    public static class FixtureExtensions
    {
        public static IFixture ConfigureServices(this IFixture fixture, Action<IServiceCollection> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var services = new ServiceCollection();
            action(services);

            return fixture.RegisterFromServiceCollection(services);
        }

        public static IFixture RegisterFromServiceCollection(this IFixture fixture, IServiceCollection services)
        {
            var serviceProvider = new AutoFixtureServiceProvider(services.BuildServiceProvider(), fixture);
            fixture.Inject<IServiceProvider>(serviceProvider);

            if (!fixture.Behaviors.Any(x => x is OmitOnRecursionBehavior))
            {
                fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            }

            fixture.Customizations.Add(new ServiceProviderBasedSpecimenBuilder(serviceProvider));

            return fixture;
        } 
    }
}
