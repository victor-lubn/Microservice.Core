using Lueben.Microservice.OpenApi.Visitors.Tests.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi.Visitors.Tests
{
    public class ReadonlyAttributeVisitorTests
    {
        [Fact]
        public void GivenReadonlyAttributeVisitor_WhenIsVisitableCalled_ThenTrueIsReturned()
        {
            var visitor = new ReadonlyAttributeVisitor(new VisitorCollection(new List<IVisitor>()));

            var result = visitor.IsVisitable(typeof(TestClass));

            Assert.True(result);
        }

        [Fact]
        public void GivenReadonlyAttributeVisitor_WhenVisitCalledAndNoExpectedAcceptor_ThenSchemaReadOnlyIsNotSet()
        {
            var key = "key";
            var visitor = new ReadonlyAttributeVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new TestAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.False(schema.ReadOnly);
        }

        [Fact]
        public void GivenReadonlyAttributeVisitor_WhenVisitCalledAndNoExpectedSchemaInAcceptor_ThenSchemaReadOnlyIsNotSet()
        {
            var key = "unexpectedKey";
            var visitor = new ReadonlyAttributeVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add("key", schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());
            
            Assert.False(schema.ReadOnly);
        }

        [Fact]
        public void GivenReadonlyAttributeVisitor_WhenVisitCalledAndSchemaIsReadOnly_ThenSchemaReadOnlyIsNotChanged()
        {
            var key = "key";
            var visitor = new ReadonlyAttributeVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema
            {
                ReadOnly = true
            };
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.True(schema.ReadOnly);
        }

        [Fact]
        public void GivenReadonlyAttributeVisitor_WhenVisitCalledAndPropertiesListIsEmptyInAcceptor_ThenSchemaReadOnlyIsNotSet()
        {
            var key = "key";
            var visitor = new ReadonlyAttributeVisitor(new VisitorCollection(new List<IVisitor>()));
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.False(schema.ReadOnly);
        }

        [Fact]
        public void GivenReadonlyAttributeVisitor_WhenVisitCalledForReadOnlyProperty_ThenSchemaReadOnlyIsSet()
        {
            var key = "key";
            var visitor = new ReadonlyAttributeVisitor(new VisitorCollection(new List<IVisitor>()));

            var acceptor = new OpenApiSchemaAcceptor();
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.ReadOnlyProperty));
            acceptor.Properties.Add(key, propertyInfo);

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.True(schema.ReadOnly);
        }

        [Fact]
        public void GivenReadonlyAttributeVisitor_WhenVisitCalledNotForReadOnlyProperty_ThenSchemaReadOnlyIsNotSet()
        {
            var key = "key";
            var visitor = new ReadonlyAttributeVisitor(new VisitorCollection(new List<IVisitor>()));

            var acceptor = new OpenApiSchemaAcceptor();
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.Created));
            acceptor.Properties.Add(key, propertyInfo);

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.False(schema.ReadOnly);
        }
    }
}
