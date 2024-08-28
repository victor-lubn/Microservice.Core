using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi.Visitors.Tests.Models
{
    public class TestAcceptor : IAcceptor
    {
        public Dictionary<string, OpenApiSchema> Schemas { get; set; } = new Dictionary<string, OpenApiSchema>();
        public void Accept(VisitorCollection collection, NamingStrategy namingStrategy)
        {
            throw new NotImplementedException();
        }
    }
}
