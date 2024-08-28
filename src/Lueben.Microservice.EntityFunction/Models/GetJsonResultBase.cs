using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Lueben.Microservice.EntityFunction.Models
{
    [ExcludeFromCodeCoverage]
    public abstract class GetJsonResultBase : ObjectResult
    {
        protected GetJsonResultBase(object value) : base(value)
        {
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var json = SerializeObject();
            var data = Encoding.UTF8.GetBytes(json);
            var response = context.HttpContext.Response;
            response.ContentType = MediaTypeNames.Application.Json;
            await response.Body.WriteAsync(data, 0, data.Length);
        }

        protected abstract string SerializeObject();
    }
}
