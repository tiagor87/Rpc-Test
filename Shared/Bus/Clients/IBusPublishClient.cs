using System;

namespace Shared.Bus.Clients
{
    public interface IBusPublishClient : IDisposable
    {
        void Publish(string exchange, string routingKey, object message);
    }
}