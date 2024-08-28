using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WireMock.Admin.Mappings;
using WireMock.Extensions;
using WireMock.Pact.Models.V2;

namespace Lueben.Integration.Testing.WireMock.Utils
{
    public static class LuebenPactMapper
    {
        private const string DefaultConsumer = "Default Consumer";
        private const string DefaultProvider = "Default Provider";

        private static Type pactMapperType = GetPactMapperType();

        public static (string FileName, Pact Pact) ToPactObject(string consumer, KeyValuePair<string, List<MappingModel>> providerMappings, string filename = null)
        {
            consumer = consumer ?? DefaultConsumer;
            var provider = providerMappings.Key ?? DefaultProvider;

            filename ??= $"pact-{consumer}-{provider}.json";

            var pact = new Pact
            {
                Consumer = new Pacticipant { Name = consumer },
                Provider = new Pacticipant { Name = provider },
            };

            foreach (var mapping in providerMappings.Value.OrderBy(m => m.Guid))
            {
                var path = mapping.Request.GetPathAsString();
                if (path == null)
                {
                    continue;
                }

                var interaction = new Interaction
                {
                    Description = mapping.Description,
                    ProviderState = mapping.Title,
                    Request = MapRequest(mapping.Request, path),
                    Response = MapResponse(mapping.Response),
                };

                pact.Interactions.Add(interaction);
            }

            return (filename, pact);
        }

        public static byte[] SerializeAsPactFile(object value)
        {
            var json = JsonConvert.SerializeObject(value, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                },
            });

            return Encoding.UTF8.GetBytes(json);
        }

        private static Type GetPactMapperType()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "WireMock.Net");
            var pactMapperType = assembly?.GetTypes().FirstOrDefault(x => x.Name == "PactMapper");

            return pactMapperType;
        }

        private static PactRequest MapRequest(RequestModel request, string path)
        {
            var result = (PactRequest)pactMapperType.GetMethod("MapRequest", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                .Invoke(null, new object[] { request, path });

            return result;
        }

        private static PactResponse MapResponse(ResponseModel response)
        {
            var result = (PactResponse)pactMapperType.GetMethod("MapResponse", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                .Invoke(null, new object[] { response });

            return result;
        }
    }
}
