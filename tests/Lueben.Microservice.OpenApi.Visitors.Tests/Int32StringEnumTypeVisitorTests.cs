using Lueben.Microservice.OpenApi.Visitors.Tests.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;

namespace Lueben.Microservice.OpenApi.Visitors.Tests
{
    public class Int32StringEnumTypeVisitorTests
    {

        [Fact]
        public void GivenInt32StringEnumTypeVisitor_WhenInstantiatedAndAppropriateVisitorInPassedCollection_ThenSetupVisitor()
        {
            var visitor = new Int32EnumTypeVisitor(new VisitorCollection(new List<IVisitor>()));

            var int32StringEnumTypeVisitor = new Int32StringEnumTypeVisitor(new VisitorCollection(new List<IVisitor> {visitor}));

            var result = int32StringEnumTypeVisitor.IsVisitable(typeof(TestClass));

            Assert.False(result);
        }

        [Fact]
        public void GivenInt32StringEnumTypeVisitor_WhenInstantiatedAndAppropriateNoVisitorInPassedCollection_ThenVisitorIsNull()
        {
            var int32StringEnumTypeVisitor = new Int32StringEnumTypeVisitor(new VisitorCollection(new List<IVisitor>()));

            Assert.Throws<NullReferenceException>(() => int32StringEnumTypeVisitor.IsVisitable(typeof(TestClass)));
        }
    }
}
