// ReSharper disable UnusedTypeParameter

using HamedStack.TheResult;
using MediatR;

namespace HamedStack.CQRS;

public interface IQuery<TResult> : IRequest<Result<TResult>>
{
}