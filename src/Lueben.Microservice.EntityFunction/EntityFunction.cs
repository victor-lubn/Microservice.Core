using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Lueben.Microservice.Api.Models;
using Lueben.Microservice.Api.ValidationFunction;
using Lueben.Microservice.Api.ValidationFunction.Exceptions;
using Lueben.Microservice.Api.ValidationFunction.Extensions;
using Lueben.Microservice.EntityFunction.Extensions;
using Lueben.Microservice.EntityFunction.Models;
using Lueben.Microservice.Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;

namespace Lueben.Microservice.EntityFunction
{
    public abstract class EntityFunction<TEntity, TModel> : FunctionBase<TModel>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        protected EntityFunction(IMediator mediator, IMapper mapper, AbstractValidator<TModel> validator) : base(validator)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<T> GetValidatedRequest<T>(HttpRequestData request, AbstractValidator<T> validator, bool allowEmptyPayload = false)
        {
            var form = await request.GetRequestValidatedResult(validator, allowEmptyPayload);
            if (form.IsValid)
            {
                return form.Value;
            }

            throw new ModelNotValidException(form.Errors);
        }

        protected virtual async Task<IActionResult> Create<TCreateCommand, TResult>(HttpRequestData request, Action<TCreateCommand> setCommandAction = null)
            where TCreateCommand : IRequest<TResult>
        {
            var model = await DeserializeJsonBody(request);
            var command = _mapper.Map<TModel, TCreateCommand>(model);
            setCommandAction?.Invoke(command);

            var result = await _mediator.Send<TCreateCommand, TResult>(command);
            return new CreatedObjectResult<TResult>(result);
        }

        protected virtual async Task<IActionResult> CreateWithoutResponse<TCreateCommand>(HttpRequestData request, Action<TCreateCommand> setCommandAction = null)
            where TCreateCommand : IRequest<Unit>
        {
            var model = await DeserializeJsonBody(request);
            var command = _mapper.Map<TModel, TCreateCommand>(model);
            setCommandAction?.Invoke(command);

            await _mediator.Send<TCreateCommand, Unit>(command);
            return new CreatedEmptyResult();
        }

        protected async Task<IActionResult> GetAll<TGetAllQuery>(Action<TGetAllQuery> setQuery = null)
            where TGetAllQuery : class, IRequest<IQueryable<TEntity>>, new()
        {
            var query = new TGetAllQuery();

            setQuery?.Invoke(query);
            var queryResult = await _mediator.Send<TGetAllQuery, IQueryable<TEntity>>(query);

            var response = new Response<IList<TModel>>(_mapper.Map<IList<TEntity>, IList<TModel>>(queryResult.ToList()));
            return new GetJsonResult<IList<TModel>>(response);
        }

        protected async Task<IActionResult> Get<TGetQuery, TArg>(TArg id)
            where TGetQuery : IRequest<TEntity>, IEntityOperation<TArg>, new()
        {
            return await Get<TGetQuery>(query => query.Id = id);
        }

        protected async Task<IActionResult> Get<TGetQuery>(Action<TGetQuery> setGetQuery)
            where TGetQuery : IRequest<TEntity>, new()
        {
            var query = new TGetQuery();
            setGetQuery(query);

            var entity = await _mediator.Send<TGetQuery, TEntity>(query);

            var model = _mapper.Map<TEntity, TModel>(entity);
            var response = new Response<TModel>(model);
            return new GetJsonResult<TModel>(response);
        }

        protected async Task<IActionResult> Patch<TPatchCommand, TArg>(HttpRequestData request, TArg id)
            where TPatchCommand : IRequest<Unit>, IEntityOperation<TArg>
        {
            return await Patch<TPatchCommand>(request, command => command.Id = id);
        }

        protected async Task<IActionResult> Patch<TPatchCommand>(HttpRequestData request, Action<TPatchCommand> setPatchCommand)
            where TPatchCommand : IRequest<Unit>
        {
            var model = await DeserializeJsonBody(request);
            var command = _mapper.Map<TModel, TPatchCommand>(model);
            setPatchCommand(command);

            await _mediator.Send<TPatchCommand, Unit>(command);

            return new NoContentResult();
        }

        protected async Task<IActionResult> Put<TPutCommand, TArg>(HttpRequestData request, TArg id)
            where TPutCommand : IRequest<Unit>, IEntityOperation<TArg>
        {
            return await Put<TPutCommand>(request, command => command.Id = id);
        }

        protected async Task<IActionResult> Put<TPutCommand>(HttpRequestData request, Action<TPutCommand> setPutCommand)
            where TPutCommand : IRequest<Unit>
        {
            var model = await DeserializeJsonBody(request);
            var command = _mapper.Map<TModel, TPutCommand>(model);
            setPutCommand(command);

            await _mediator.Send<TPutCommand, Unit>(command);

            return new NoContentResult();
        }

        protected async Task<IActionResult> Put<TPutCommand, TUpdateModel>(HttpRequestData request, Action<TPutCommand> setPutCommand, AbstractValidator<TUpdateModel> validator)
            where TPutCommand : IRequest<Unit>
        {
            var model = await GetValidatedRequest(request, validator);
            var command = _mapper.Map<TUpdateModel, TPutCommand>(model);
            setPutCommand(command);

            await _mediator.Send<TPutCommand, Unit>(command);

            return new NoContentResult();
        }

        protected async Task<IActionResult> Put<TPutCommand, TUpdateModel, TResult>(HttpRequestData request, Action<TPutCommand> setPutCommand, AbstractValidator<TUpdateModel> validator)
            where TPutCommand : IRequest<TResult>
        {
            var model = await GetValidatedRequest(request, validator);
            var command = _mapper.Map<TUpdateModel, TPutCommand>(model);
            setPutCommand(command);

            var result = await _mediator.Send<TPutCommand, TResult>(command);

            return new ObjectResult<TResult>(result);
        }

        protected async Task<IActionResult> Delete<TDeleteCommand, TArg>(TArg id)
            where TDeleteCommand : IRequest<Unit>, IEntityOperation<TArg>, new()
        {
            return await Delete<TDeleteCommand>(command => command.Id = id);
        }

        protected async Task<IActionResult> Delete<TDeleteCommand, TArg, TResult>(TArg id)
            where TDeleteCommand : IRequest<TResult>, IEntityOperation<TArg>, new()
        {
            return await Delete<TDeleteCommand, TResult>(command => command.Id = id);
        }

        protected async Task<IActionResult> Delete<TDeleteCommand>(Action<TDeleteCommand> setDeleteCommand)
            where TDeleteCommand : IRequest<Unit>, new()
        {
            var command = new TDeleteCommand();
            setDeleteCommand(command);
            await _mediator.Send<TDeleteCommand, Unit>(command);

            return new NoContentResult();
        }

        protected async Task<IActionResult> Delete<TDeleteCommand, TResult>(Action<TDeleteCommand> setDeleteCommand)
            where TDeleteCommand : IRequest<TResult>, new()
        {
            var command = new TDeleteCommand();
            setDeleteCommand(command);
            var result = await _mediator.Send<TDeleteCommand, TResult>(command);

            return new ObjectResult<TResult>(result);
        }

        protected virtual async Task<TModel> DeserializeJsonBody(HttpRequestData request)
        {
            return await GetValidatedRequest(request);
        }

        protected async Task<IActionResult> GetPaginatedEntities<TGetAllQuery>(HttpRequestData request, Action<TGetAllQuery> setQuery = null)
            where TGetAllQuery : class, IRequest<GetAllEntitiesResult<TEntity>>, new()
        {
            var query = request.ToQueryObject<TGetAllQuery>();

            setQuery?.Invoke(query);

            var queryResult = await _mediator.Send<TGetAllQuery, GetAllEntitiesResult<TEntity>>(query);

            var paginatedResponse = new PaginatedResponse<TModel>(_mapper.Map<IList<TEntity>, IList<TModel>>(queryResult.Items.ToList()), queryResult.TotalPages, queryResult.TotalItems);

            return new GetListJsonResult<TModel>(paginatedResponse);
        }
    }
}
