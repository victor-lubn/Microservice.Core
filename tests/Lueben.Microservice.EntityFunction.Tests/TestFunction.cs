using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Lueben.Microservice.EntityFunction.Models;
using Lueben.Microservice.Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;

namespace Lueben.Microservice.EntityFunction.Tests
{
    public class TestFunction : EntityFunction<TestEntity, TestModel>
    {
        public TestFunction(IMediator mediator, IMapper mapper)
            : base(mediator, mapper, null)
        {
        }

        public async Task<IActionResult> Create(HttpRequestData request)
        {
            return await Create<CreateTestCommand, long>(request);
        }

        public async Task<IActionResult> CreateWithoutResponse(HttpRequestData request)
        {
            return await CreateWithoutResponse<TestCommand>(request);
        }

        public async Task<IActionResult> GetAll()
        {
            return await GetAll<GetAllTestQuery>();
        }

        public async Task<IActionResult> GetAllPaginatedEntities(HttpRequestData request)
        {
            return await GetPaginatedEntities<GetAllPaginatedTestQuery>(request);
        }

        public async Task<IActionResult> Get(long id)
        {
            return await Get<GetTestQuery, long>(id);
        }

        public async Task<IActionResult> Patch(HttpRequestData request, long id)
        {
            return await Patch<TestCommand, long>(request, id);
        }

        public async Task<IActionResult> Put(HttpRequestData request, long id)
        {
            return await Put<TestCommand, long>(request, id);
        }

        public async Task<IActionResult> Put(HttpRequestData request, long id, AbstractValidator<TestModel> validator)
        {
            return await Put<TestCommand, TestModel>(request, command => command.Id = id, validator);
        }

        public async Task<IActionResult> PutWithUnitResult(HttpRequestData request, long id, AbstractValidator<TestModel> validator)
        {
            return await Put<TestCommand, TestModel, Unit>(request, command => command.Id = id, validator);
        }

        public async Task<IActionResult> Delete(long id)
        {
            return await Delete<TestCommand, long>(id);
        }

        public async Task<IActionResult> DeleteGeneric(long id)
        {
            return await Delete<TestCommand, long, Unit>(id);
        }
    }

    public class CreateTestCommand : IRequest<long>
    {
        public string Test { get; set; }
    }

    public class TestCommand : IRequest<Unit>, IEntityOperation<long>
    {
        public long Id { get; set; }
    }

    public class GetAllTestQuery : IRequest<IQueryable<TestEntity>>
    {
    }

    public class GetAllPaginatedTestQuery : IRequest<GetAllEntitiesResult<TestEntity>>
    {
    }

    public class GetTestQuery : IRequest<TestEntity>, IEntityOperation<long>
    {
        public long Id { get; set; }
    }

    public class TestEntity
    {
        public long Id { get; set; }

        public string Test { get; set; }
    }

    public class TestModel
    {
        public long Id { get; set; }

        public string Test { get; set; }
    }

    public class TestPaginatedModel
    {
        public long Id { get; set; }

        public string Test { get; set; }
    }
}
