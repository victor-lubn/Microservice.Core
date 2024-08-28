using System;
using System.Collections.Generic;
using System.Reflection;
using Lueben.Microservice.Diagnostics;
using Lueben.Microservice.Json.Encrypt.Configurations;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.Json.Encrypt
{
    public class EncryptedStringPropertyResolver : DefaultContractResolver
    {
        private readonly IOptions<EncryptionOptions> options;

        public EncryptedStringPropertyResolver(IOptions<EncryptionOptions> options)
        {
            Ensure.ArgumentNotNull(options, nameof(options));

            this.options = options;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = base.CreateProperties(type, memberSerialization);

            foreach (var prop in props)
            {
                var pi = type.GetProperty(prop.UnderlyingName);
                if (pi != null && pi.GetCustomAttribute(typeof(JsonEncryptAttribute), true) != null)
                {
                    prop.Converter = new EncryptingJsonConverter(this.options.Value.Secret);
                }
            }

            return props;
        }
    }
}
