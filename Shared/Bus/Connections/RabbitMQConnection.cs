using System;
using System.Collections.Concurrent;
using MediatR;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ResultCore;
using Shared.Bus.Clients;

namespace Shared.Bus.Connections
{
    public class RabbitMQConnection : IBusPublishClient, IBusRpcClient, IDisposable
    {
        private readonly IBusSerializer _busSerializer;
        private readonly IModel _channel;
        private readonly IConnection _connection;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed;

        public RabbitMQConnection(IConfiguration configuration, IBusSerializer busSerializer,
            IServiceProvider serviceProvider)
        {
            _busSerializer = busSerializer ?? throw new ArgumentNullException(nameof(busSerializer));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            var connectionFactory = new ConnectionFactory
                {Uri = new Uri(configuration.GetConnectionString("RabbitMQ"))};
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        internal IModel Channel => _channel;

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

            var result = _busSerializer.Deserialize<Result<TResponse>>(replyBody);
            if (result.Failure)
            {
                throw new InvalidOperationException(result.Message);
            }

            return result.Value;
        }

        public void Listen<TCommand, TResponse>(string exchange, string queue)
            where TCommand : IRequest<TResponse>
        {
            Listen(typeof(TCommand), exchange, queue);
        }

        public void Listen(Type commandType, string exchange, string queue)
        {
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