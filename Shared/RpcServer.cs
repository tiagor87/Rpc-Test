using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared
{
    public abstract class RpcServer<TRequest, TResponse> : IDisposable
    {
        private bool _disposed;
        
        private readonly ISerializer _serializer;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        protected RpcServer(string connectionString, ISerializer serializer)
        {
            _serializer = serializer;

            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _channel.QueueDeclare(
                "rpc_queue",
                false,
                false,
                false,
                null);
            _channel.BasicQos(
                0,
                1,
                false);
                
            var consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(
                "rpc_queue",
                false,
                consumer);
            
            consumer.Received += (model, @event) =>
            {
                var response = default(TResponse);

                var body = @event.Body;
                var props = @event.BasicProperties;
                var replyProps = _channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    var request = _serializer.Deserialize<TRequest>(body).Result;
                    response = Execute(request);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e.Message);
                }
                finally
                {
                    var responseBytes = _serializer.Serialize(response).Result;
                    _channel.BasicPublish(
                        "", 
                        props.ReplyTo,
                        replyProps,
                        responseBytes);
                    _channel.BasicAck(
                        @event.DeliveryTag,
                        false);
                }
            };
        }

        protected abstract TResponse Execute(TRequest request);
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _connection.Dispose();
                _channel.Dispose();
            }

            _disposed = true;
        }
    }
}