using System;
using System.Linq;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;

namespace Lueben.Microservice.OpenApi.Visitors
{
    public class Int32StringEnumTypeVisitor : StringEnumTypeVisitor
    {
        private readonly IVisitor _int32EnumTypeVisitor;

        public Int32StringEnumTypeVisitor(VisitorCollection collection) : base(collection)
        {
            var int32EnumTypeVisitor = collection.Visitors.FirstOrDefault(v => v.GetType() == typeof(Int32EnumTypeVisitor));
            if (int32EnumTypeVisitor != null)
            {
                collection.Visitors.Remove(int32EnumTypeVisitor);
            }

            _int32EnumTypeVisitor = int32EnumTypeVisitor;
        }

        public override bool IsVisitable(Type type)
        {
            var isVisitable = _int32EnumTypeVisitor.IsVisitable(type);
            return isVisitable;
        }
    }
}