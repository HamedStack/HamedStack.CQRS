// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedTypeParameter

using HamedStack.TheResult;
using MediatR;

namespace HamedStack.CQRS;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, Result<TResult>>
    where TQuery : IQuery<TResult>
{
}