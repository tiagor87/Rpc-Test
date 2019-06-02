using MediatR;

namespace Shared.Events
{
    public abstract class IBusEvent<TEvent> : INotification
    {
    }
}