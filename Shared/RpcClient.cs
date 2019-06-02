using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared
{
    public class RpcClient : IDisposable
    {
        private readonly IBusSerializer _busSerializer;
        private readonly IModel _channel;
        private readonly IConnection _connection;
        private readonly EventingBasicConsumer _consumer;
        private readonly IBasicProperties _props;
        private readonly string _replyQueueName;
        private readonly BlockingCollection<byte[]> _responseQueue = new BlockingCollection<byte[]>();
        private bool _disposed;

        public RpcClient(string connectionString, IBusSerializer busSerializer)
        {
            _busSerializer = busSerializer;
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(connectionString);

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _replyQueueName = _channel.QueueDeclare().QueueName;
            _consumer = new EventingBasicConsumer(_channel);
            _props = _channel.CreateBasicProperties();
            _props.CorrelationId = Guid.NewGuid().ToString();
            _props.ReplyTo = _replyQueueName;

            _consumer.Received += (model, @event) =>
            {
                var body = @event.Body;
                if (@event.BasicProperties.CorrelationId == _props.CorrelationId)
                {
                    _responseQueue.Add(body);
                }
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request)
        {
            var body = request is string
                ? Encoding.UTF8.GetBytes(request as string)
                : _busSerializer.Serialize(request);

            _channel.BasicPublish(
                "",
                "rpc_queue",
                _props,
                body);

            _channel.BasicConsume(
                _consumer,
                _replyQueueName,
                true);

            var replyBody = _responseQueue.Take();
            return _busSerializer.Deserialize<TResponse>(replyBody);
        }

        ~RpcClient()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _channel.Dispose();
                _connection.Dispose();
            }

            _disposed = true;
        }
    }
}