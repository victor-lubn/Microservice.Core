using System.Threading.Tasks;
using Lueben.Microservice.Api.Idempotency.Exceptions;
using Lueben.Microservice.Api.Idempotency.Extensions;
using Lueben.Microservice.Api.Idempotency.Models;
using Lueben.Microservice.EntityFunction.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Lueben.Microservice.Api.Idempotency.Tests
{
    public class IdempotencyEntityExtensionsTests
    {
        [Fact]
        public void GivenGetVerifiedEntity_WhenEntityIsNull_ThenNullShouldBeReturned()
        {
            IdempotencyEntity entity = null;

            var actualResult = entity.GetVerifiedEntity("test");

            Assert.Null(actualResult);
        }

        [Fact]
        public void GivenGetVerifiedEntity_WhenEntityResponseIsNull_ThenIdempotencyConflictExceptionShouldBeThrown()
        {
            var entity = new IdempotencyEntity();

            var exception = Assert.Throws<IdempotencyConflictException>(() => entity.GetVerifiedEntity("test"));

            Assert.NotNull(exception);
        }

        [Fact]
        public void GivenGetVerifiedEntity_WhenEntityHashDiffers_ThenIdempotencyPayloadExceptionShouldBeThrown()
        {
            var entity = new IdempotencyEntity
            {
               PayloadHash = "test1",
               Response = "Test response"
            };

            var exception = Assert.Throws<IdempotencyPayloadException>(() => entity.GetVerifiedEntity("test"));

            Assert.NotNull(exception);
        }

        [Fact]
        public void GivenGetVerifiedEntity_WhenEntityHashEqual_ThenVerifiedEntutyIsReturned()
        {
            var entity = new IdempotencyEntity
            {
                PayloadHash = "test",
                Response = "Test response"
            };

            var verifiesEntity = entity.GetVerifiedEntity("test");

            Assert.NotNull(verifiesEntity);
            Assert.Equal(entity, verifiesEntity);
        }
    }
}
