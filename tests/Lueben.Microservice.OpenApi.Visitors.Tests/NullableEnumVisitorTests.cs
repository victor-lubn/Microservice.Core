using Lueben.Microservice.OpenApi.Visitors.Tests.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi.Visitors.Tests
{
    public class NullableEnumVisitorTests
    {
        [Fact]
        public void GivenNullableEnumVisitor_WhenIsVisitableCalled_ThenTrueIsReturned()
        {
            var visitor = new NullableEnumVisitor(new VisitorCollection(new List<IVisitor>()));

            var result = visitor.IsVisitable(typeof(TestClass));

            Assert.True(result);
        }

        [Fact]
        public void GivenNullableEnumVisitor_WhenVisitCalledAndNoExpectedAcceptor_ThenSchemaDefaultIsNotSet()
        {
            var key = "key";
            var visitor = new NullableEnumVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new TestAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.Null(schema.Default);
        }

        [Fact]
        public void GivenNullableEnumVisitor_WhenVisitCalledAndNoExpectedSchemaInAcceptor_ThenSchemaDefaultIsNotSet()
        {
            var key = "unexpectedKey";
            var visitor = new NullableEnumVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add("key", schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());
            
            Assert.Null(schema.Default);
        }

        [Fact]
        public void GivenNullableEnumVisitor_WhenVisitCalledAndPropertiesListIsEmptyInAcceptorSchema_ThenSchemaDefaultIsNotSet()
        {
            var key = "key";
            var visitor = new NullableEnumVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.Null(schema.Default);
        }

        [Fact]
        public void GivenNullableEnumVisitor_WhenVisitCalledForNullableEnum_ThenSchemaDefaultIsSet()
        {
            var key = "key";
            var visitor = new NullableEnumVisitor(new VisitorCollection(new List<IVisitor>()));

            var acceptor = new OpenApiSchemaAcceptor();
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.EnumProperty));
            acceptor.Properties.Add(key, propertyInfo);

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.NotNull(schema.Default);
        }

        [Fact]
        public void GivenNullableEnumVisitor_WhenVisitCalledNotForNullableEnum_ThenSchemaDefaultIsNotSet()
        {
            var key = "key";
            var visitor = new NullableEnumVisitor(new VisitorCollection(new List<IVisitor>()));

            var acceptor = new OpenApiSchemaAcceptor();
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.Created));
            acceptor.Properties.Add(key, propertyInfo);

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.Null(schema.Default);
        }
    }
}
