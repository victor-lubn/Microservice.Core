using Lueben.Microservice.OpenApi.Visitors.Tests.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi.Visitors.Tests
{
    public class AuditDateTimeFieldsVisitorTests
    {
        [Fact]
        public void GivenAuditDateTimeFieldsVisitor_WhenVisitCalled_ThenAuditFieldsAreAdded()
        {
            var key = "created";
            var visitor = new AuditDateTimeFieldsVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.True(schema.ReadOnly);
            Assert.NotNull(schema.Description);
        }

        [Fact]
        public void GivenAuditDateTimeFieldsVisitor_WhenVisitCalledAndNoExpectedSchemaInAcceptor_ThenAuditFieldsAreNotAdded()
        {
            var key = "unexpectedKey";
            var visitor = new AuditDateTimeFieldsVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.False(schema.ReadOnly);
            Assert.Null(schema.Description);
        }

        [Fact]
        public void GivenAuditDateTimeFieldsVisitor_WhenVisitCalledAndNoExpectedAcceptor_ThenAuditFieldsAreNotAdded()
        {
            var key = "created";
            var visitor = new AuditDateTimeFieldsVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new TestAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.False(schema.ReadOnly);
            Assert.Null(schema.Description);
        }
    }
}