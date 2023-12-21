namespace Willow.Core.Eventing.Abstractions;

public interface IEventHandler<in TEvent>
{
    Task HandleAsync(TEvent @event);
}