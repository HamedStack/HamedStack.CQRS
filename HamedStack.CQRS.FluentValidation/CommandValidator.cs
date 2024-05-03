using FluentValidation;

namespace HamedStack.CQRS.FluentValidation;

public abstract class CommandValidator<TCommand, TCommandResult> : AbstractValidator<TCommand>
    where TCommand : ICommand<TCommandResult>
{
}

public abstract class CommandValidator<TCommand> : AbstractValidator<TCommand>
    where TCommand : ICommand
{
}