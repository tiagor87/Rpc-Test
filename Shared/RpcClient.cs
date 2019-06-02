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
        private bool _disposed;
        
        private readonly ISerializer _serializer;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly BlockingCollection<byte[]> _responseQueue = new BlockingCollection<byte[]>();
        private readonly IBasicProperties _props;
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;

        public RpcClient(string connectionString, ISerializer serializer)
        {
            _serializer = serializer;
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

        public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request)
        {
            var body = request is string
                ? Encoding.UTF8.GetBytes(request as string)
                : _serializer.Serialize(request).Result;
            
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
            return await _serializer.Deserialize<TResponse>(replyBody);
        }

        ~RpcClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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