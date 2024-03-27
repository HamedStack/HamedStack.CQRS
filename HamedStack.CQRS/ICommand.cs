// ReSharper disable UnusedTypeParameter

using HamedStack.TheResult;
using MediatR;

namespace HamedStack.CQRS;

public interface ICommand<TResult> : IRequest<Result<TResult>>, IBaseCommand
{
}

public interface ICommand : IRequest<Result>, IBaseCommand
{
}