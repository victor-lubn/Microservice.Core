using System;
using AutoFixture.Kernel;

namespace Lueben.Integration.Testing.Common.AutoFixture
{
    internal class ServiceProviderBasedSpecimenBuilder : ISpecimenBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderBasedSpecimenBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object Create(object request, ISpecimenContext context)
        {
            if (request is Type type)
            {
                var service = _serviceProvider.GetService(type);
                if (service is OmitSpecimen)
                {
                    return new NoSpecimen();
                }

                return service ?? context.Resolve(request);
            }

            return new NoSpecimen();
        }
    }
}
