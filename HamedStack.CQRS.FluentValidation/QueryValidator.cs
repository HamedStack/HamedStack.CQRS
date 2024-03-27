using FluentValidation;
using MediatR;

namespace HamedStack.CQRS.FluentValidation;

public abstract class QueryValidator<TQuery, TQueryResult> : AbstractValidator<TQuery>
    where TQuery : IRequest<TQueryResult>
{
}