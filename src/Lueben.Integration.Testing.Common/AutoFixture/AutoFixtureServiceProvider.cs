using System;
using AutoFixture;
using AutoFixture.Kernel;

namespace Lueben.Integration.Testing.Common.AutoFixture
{
    internal class AutoFixtureServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFixture _fixture;

        public AutoFixtureServiceProvider(IServiceProvider serviceProvider, IFixture fixture)
        {
            _serviceProvider = serviceProvider;
            _fixture = fixture;
        }

        public object GetService(Type serviceType)
        {
            try
            {
                if (serviceType == typeof(IServiceProvider))
                {
                    return this;
                }

                var service = _serviceProvider.GetService(serviceType);

                return service ?? ResolveFromFixture(serviceType);
            }
            catch (InvalidOperationException)
            {
                return ResolveFromFixture(serviceType);
            }
        }

        private object ResolveFromFixture(Type serviceType)
        {
            var instance = _fixture.Create(serviceType, new SpecimenContext(_fixture));
            return instance;
        }
    }
}
