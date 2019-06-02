namespace Shared.Bus.Clients
{
    public interface IBusPublishClient
    {
        void Publish(string exchange, string routingKey, object message);
    }
}