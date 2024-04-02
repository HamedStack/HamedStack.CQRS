using FluentValidation;

namespace HamedStack.CQRS.FluentValidation;

public abstract class QueryValidator<TQuery, TQueryResult> : AbstractValidator<TQuery>
    where TQuery : IQuery<TQueryResult>
{
}