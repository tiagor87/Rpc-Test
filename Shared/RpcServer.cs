using System;
using MediatR;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared
{
    public class RpcRequestHandler<TRequest, TResponse> : RpcServer<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IMediator _mediator;

        protected RpcRequestHandler(
            IMediator mediator,
            IConfiguration configuration,
            IBusSerializer busSerializer) : base(configuration.GetConnectionString("RabbitMQ"), busSerializer)
        {
            _mediator = mediator;
        }

        protected override TResponse Execute(TRequest request)
        {
            return _mediator.Send(request).Result;
        }
    }

    public abstract class RpcServer<TRequest, TResponse> : IDisposable
    {
        private readonly IBusSerializer _busSerializer;
        private readonly IModel _channel;
        private readonly IConnection _connection;
        private bool _disposed;

        protected RpcServer(string connectionString, IBusSerializer busSerializer)
        {
            _busSerializer = busSerializer;

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
                    var request = _busSerializer.Deserialize<TRequest>(body);
                    response = Execute(request);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e.Message);
                }
                finally
                {
                    var responseBytes = _busSerializer.Serialize(response);
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract TResponse Execute(TRequest request);

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