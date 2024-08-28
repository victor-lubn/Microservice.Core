using System;

namespace Lueben.Microservice.Api.Middleware.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException() : base("Entity not found.")
        {
        }
    }
}