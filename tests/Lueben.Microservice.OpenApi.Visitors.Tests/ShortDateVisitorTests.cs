using Lueben.Microservice.OpenApi.Visitors.Tests.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi.Visitors.Tests
{
    public class ShortDateVisitorTests
    {
        [Fact]
        public void GivenShortDateVisitor_WhenIsVisitableCalled_ThenTrueIsReturned()
        {
            var visitor = new ShortDateVisitor(new VisitorCollection(new List<IVisitor>()));

            var result = visitor.IsVisitable(typeof(TestClass));

            Assert.True(result);
        }

        [Fact]
        public void GivenShortDateVisitor_WhenVisitCalledAndNoExpectedAcceptor_ThenSchemaFormatAndExampleAreNotSet()
        {
            var key = "key";
            var visitor = new ShortDateVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new TestAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.Null(schema.Example);
            Assert.Null(schema.Format);
        }

        [Fact]
        public void GivenShortDateVisitor_WhenVisitCalledAndNoExpectedSchemaInAcceptor_ThenSchemaFormatAndExampleAreNotSet()
        {
            var key = "unexpectedKey";
            var visitor = new ShortDateVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add("key", schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.Null(schema.Example);
            Assert.Null(schema.Format);
        }

        [Fact]
        public void GivenShortDateVisitor_WhenVisitCalledAndPropertiesListIsEmptyInAcceptor_ThenSchemaFormatAndExampleAreNotSet()
        {
            var key = "key";
            var visitor = new ShortDateVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.Null(schema.Example);
            Assert.Null(schema.Format);
        }

        [Fact]
        public void GivenShortDateVisitor_WhenVisitCalledForShortDateProperty_ThenSchemaFormatAndExampleAreSet()
        {
            var key = "key";
            var visitor = new ShortDateVisitor(new VisitorCollection(new List<IVisitor>()));

            var acceptor = new OpenApiSchemaAcceptor();
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.ShortDateProperty));
            acceptor.Properties.Add(key, propertyInfo);

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.NotNull(schema.Example);
            Assert.Equal("date", schema.Format);
        }

        [Fact]
        public void GivenShortDateVisitor_WhenVisitCalledNotForShortDateProperty_ThenSchemaFormatAndExampleAreNotSet()
        {
            var key = "key";
            var visitor = new ShortDateVisitor(new VisitorCollection(new List<IVisitor>()));

            var acceptor = new OpenApiSchemaAcceptor();
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.Created));
            acceptor.Properties.Add(key, propertyInfo);

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.Null(schema.Example);
            Assert.Null(schema.Format);
        }
    }
}
