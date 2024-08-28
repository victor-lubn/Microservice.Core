using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Lueben.Integration.Testing.WireMock.Providers;
using Lueben.Integration.Testing.WireMock.Utils;
using Newtonsoft.Json;
using WireMock.Admin.Mappings;
using WireMock.Logging;
using WireMock.Matchers.Request;
using WireMock.Pact.Models.V2;
using WireMock.Server;
using WireMock.Settings;

namespace Lueben.Integration.Testing.WireMock
{
    public class LuebenWireMockServer : WireMockServer
    {
        private static object _lock = new object();
        private readonly Dictionary<string, List<MappingModel>> _providerMappings;
        private readonly WireMockServerSettings _settings;
        private readonly IWireMockLogger _logger;

        protected LuebenWireMockServer(WireMockServerSettings settings) : base(settings)
        {
            _providerMappings = new Dictionary<string, List<MappingModel>>();
            _settings = settings;
            _logger = settings.Logger;
        }

        public IReadOnlyDictionary<string, List<MappingModel>> ProviderMappings => _providerMappings;

        public static new LuebenWireMockServer Start(WireMockServerSettings settings)
        {
            var server = new LuebenWireMockServer(settings);
            return server;
        }

        public virtual new IRespondWithAProvider Given(IRequestMatcher requestMatcher, bool saveToFile = false)
        {
            var provider = base.Given(requestMatcher, saveToFile);

            return new LuebenRespondWithAProvider(provider, this.RegisterMappingForProvider);
        }

        public virtual new LuebenWireMockServer WithConsumer(string consumer)
        {
            base.WithConsumer(consumer);
            return this;
        }

        public virtual new LuebenWireMockServer WithProvider(string provider)
        {
            base.WithProvider(provider);
            return this;
        }

        public virtual new void SavePact(string folder, string filename = null)
        {
            Directory.CreateDirectory(folder);

            foreach (var providerMappingsCollection in _providerMappings)
            {
                var (filenameUpdated, pact) = LuebenPactMapper.ToPactObject(Consumer, providerMappingsCollection, filename);

                var pactFilePath = Path.Combine(folder, filenameUpdated);
                SavePact(pactFilePath, pact);
            }
        }

        protected virtual void RegisterMappingForProvider()
        {
            if (string.IsNullOrEmpty(Provider))
            {
                // ignore mapping, it won't be exported to pact file
                return;
            }

            var mappingModel = MappingModels.OrderByDescending(x => x.UpdatedAt).First();

            _logger.Info($"Registering mapping with '{mappingModel.Title}' title for {Provider} provider");

            if (_providerMappings.ContainsKey(Provider))
            {
                _providerMappings[Provider].Add(mappingModel);
                return;
            }

            _providerMappings.Add(Provider, new List<MappingModel> { mappingModel });
        }

        private void SavePact(string pactFilePath, Pact pact)
        {
            lock (_lock)
            {
                if (File.Exists(pactFilePath))
                {
                    MergePact(pactFilePath, pact);
                    return;
                }

                WritePactFile(pactFilePath, pact);
            }
        }

        private void MergePact(string filePath, Pact pact)
        {
            var json = File.ReadAllText(filePath);

            var existingPact = JsonConvert.DeserializeObject<Pact>(json);
            existingPact.Interactions.AddRange(pact.Interactions);

            WritePactFile(filePath, existingPact);
        }

        private void WritePactFile(string filePath, Pact pact)
        {
            try
            {
                File.WriteAllBytes(filePath, LuebenPactMapper.SerializeAsPactFile(pact));
            }
            catch (IOException)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                SavePact(filePath, pact);
            }
        }
    }
}
