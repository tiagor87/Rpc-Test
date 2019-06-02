using System;
using MediatR;
using RabbitMQ.Client;
using Shared.Bus.Clients;

namespace Shared.Bus.Connections
{
    public class RabbitMQConnection : IBusPublishClient, IBusRpcClient, IDisposable
    {
        private readonly IBusSerializer _busSerializer;
        private readonly IModel _channel;
        private readonly IConnection _connection;
        private bool _disposed;

        public RabbitMQConnection(string connectionString, IBusSerializer busSerializer)
        {
            _busSerializer = busSerializer ?? throw new ArgumentNullException(nameof(busSerializer));
            var connectionFactory = new ConnectionFactory {Uri = new Uri(connectionString)};
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Publish(string exchange, string routingKey, object message)
        {
            var body = _busSerializer.Serialize(message);
            _channel.BasicPublish(
                exchange,
                routingKey,
                false,
                null,
                body);
        }

        public TResponse Send<TRequest, TResponse>() where TRequest : IRequest<TResponse>
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RabbitMQConnection()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _channel.Dispose();
                _connection.Dispose();
            }

            _disposed = true;
        }
    }
}