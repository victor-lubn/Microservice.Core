using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lueben.Microservice.EventHub
{
    [ExcludeFromCodeCoverage]
    public class Event<T>
    {
        /// <summary>
        /// Human readable event type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Event data.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Event sender.
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Event version.
        /// </summary>
        public int? Version { get; set; }

        /// <summary>
        /// List of additional event properties to be added.
        /// </summary>
        public IDictionary<string, object> AdditionalProperties { get; set; }

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}; {Data}";
        }
    }
}