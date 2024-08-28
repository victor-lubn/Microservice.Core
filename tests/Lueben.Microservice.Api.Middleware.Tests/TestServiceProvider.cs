namespace Lueben.Microservice.Api.Middleware.Tests
{
    public class TestServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider? _serviceProvider;
        private readonly Dictionary<Type, object> _services = new();

        public TestServiceProvider() : this(null)
        {
        }

        public TestServiceProvider(IServiceProvider? serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void AddService(Type type, object instance)
        {
            _services.Add(type, instance);
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider?.GetService(serviceType)
                   ?? _services[serviceType];
        }
    }
}
