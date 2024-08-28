using System;
using System.Threading;
using Lueben.Microservice.Serialization;
using PactNet;
using PactNet.Mocks.MockHttpService;

namespace Lueben.Microservice.GenericEmail.Tests.Pact
{
    public class PactClassFixture : IDisposable
    {
        private bool _disposed;
        public IPactBuilder PactBuilder { get; }
        public IMockProviderService MockProviderService { get; }
        public int MockServerPort { get; }

        private static int _freePort = 9222;

        public string MockProviderServiceBaseUri => $"http://localhost:{MockServerPort}";

        public const string ServiceConsumer = "LuebenCore";

        public PactClassFixture(string service)
        {
            MockServerPort = Interlocked.Increment(ref _freePort);
            var pactConfig = new PactConfig
            {
                SpecificationVersion = "2.0.0",
                PactDir = @".\..\..\..\..\..\pacts",
                LogDir = @".\pact_logs"
            };

            PactBuilder = new PactBuilder(pactConfig);
            PactBuilder.ServiceConsumer(ServiceConsumer).HasPactWith(service);
            MockProviderService = PactBuilder.MockService(MockServerPort, FunctionJsonSerializerSettingsProvider.CreateSerializerSettings());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                PactBuilder.Build();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}