using HamedStack.TheAggregateRoot.Events;
using MediatR;

namespace HamedStack.CQRS;

public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : DomainEvent
{
}