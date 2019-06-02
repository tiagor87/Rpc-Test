using System;
using System.Collections.Concurrent;
using MediatR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
            Publish(
                exchange,
                routingKey,
                message,
                null);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public TResponse Send<TRequest, TResponse>(string exchange, string routingKey, TRequest request)
            where TRequest : IRequest<TResponse>
        {
            var replyQueueName = _channel.QueueDeclare().QueueName;
            var basicProperties = _channel.CreateBasicProperties();
            basicProperties.CorrelationId = Guid.NewGuid().ToString();
            basicProperties.ReplyTo = replyQueueName;

            var responseQueue = new BlockingCollection<byte[]>();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, @event) =>
            {
                if (@event.BasicProperties.CorrelationId == basicProperties.CorrelationId)
                {
                    responseQueue.Add(@event.Body);
                    _channel.BasicAck(@event.DeliveryTag, false);
                }
            };

            Publish(
                exchange,
                routingKey,
                request,
                basicProperties);

            var consumerTag = _channel.BasicConsume(
                consumer,
                replyQueueName);

            var replyBody = responseQueue.Take();

            _channel.BasicCancel(consumerTag);

            return _busSerializer.Deserialize<TResponse>(replyBody);
        }

        private void Publish(string exchange, string routingKey, object message, IBasicProperties basicProperties)
        {
            var body = _busSerializer.Serialize(message);
            _channel.BasicPublish(
                exchange,
                routingKey,
                false,
                basicProperties,
                body);
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