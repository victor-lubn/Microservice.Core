using Lueben.Microservice.OpenApi.Visitors.Tests.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi.Visitors.Tests
{
    public class ComplexObjectVisitorTests
    {
        [Fact]
        public void GivenComplexObjectVisitor_WhenVisitCalled_ThenListOfOpenApiSchemaIsExtended()
        {
            var key = "key";
            var visitor = new ComplexObjectVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            schema.Properties.Add(key, schema);
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.NotEmpty(schema.OneOf);
        }

        [Fact]
        public void GivenComplexObjectVisitor_WhenVisitCalledAndPropertiesListIsNullInAcceptorSchema_ThenListOfOpenApiSchemaIsNotExtended()
        {
            var key = "key";
            var visitor = new ComplexObjectVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema
            {
                Properties = null
            };
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.Empty(schema.OneOf);
            Assert.Null(schema.Reference);
        }

        [Fact]
        public void GivenComplexObjectVisitor_WhenVisitCalledAndNoPropertiesInAcceptorSchema_ThenListOfOpenApiSchemaIsNotExtended()
        {
            var key = "key";
            var visitor = new ComplexObjectVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.Empty(schema.OneOf);
            Assert.Null(schema.Reference);
        }

        [Fact]
        public void GivenComplexObjectVisitor_WhenVisitCalledAndNoExpectedSchemaInAcceptor_ThenListOfOpenApiSchemaIsNotExtended()
        {
            var key = "key";
            var visitor = new ComplexObjectVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>("unexpectedKey", typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.Empty(schema.OneOf);
            Assert.Null(schema.Reference);
        }

        [Fact]
        public void GivenComplexObjectVisitor_WhenVisitCalledAndNoExpectedAcceptor_ThenListOfOpenApiSchemaIsNotExtended()
        {
            var key = "key";
            var visitor = new ComplexObjectVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new TestAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.Empty(schema.OneOf);
            Assert.Null(schema.Reference);
        }
    }
}