using System;
using MediatR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ResultCore;
using Shared.Bus.Commands.Options;
using Shared.Bus.Connections;

namespace Shared.Bus.Servers
{
    public class BusRpcServer<TCommand, TResponse> : IBusRpcServer
        where TCommand : IRequest<TResponse>
    {
        private readonly IBusSerializer _busSerializer;
        private readonly RabbitMQConnection _connection;
        private readonly IMediator _mediator;
        private readonly IOptions<BusRpcOptions<TCommand, TResponse>> _options;
        private string _consumerTag;

        public BusRpcServer(
            RabbitMQConnection connection,
            IBusSerializer busSerializer,
            IOptions<BusRpcOptions<TCommand, TResponse>> options,
            IMediator mediator)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _busSerializer = busSerializer ?? throw new ArgumentNullException(nameof(busSerializer));
            _options = options;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public void Start()
        {
            if (!string.IsNullOrWhiteSpace(_options.Value.Exchange))
            {
                _connection.Channel.ExchangeDeclare(_options.Value.Exchange, ExchangeType.Direct);
            }

            _connection.Channel.QueueDeclare(
                _options.Value.Queue,
                false,
                false,
                false,
                null);

            var consumer = new EventingBasicConsumer(_connection.Channel);
            _consumerTag = _connection.Channel.BasicConsume(
                _options.Value.Queue,
                false,
                consumer);

            consumer.Received += (model, @event) =>
            {
                Result result = null;

                var body = @event.Body;
                var props = @event.BasicProperties;
                var replyProps = _connection.Channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    var request = _busSerializer.Deserialize<TCommand>(body);
                    var response = _mediator.Send(request).Result;
                    result = Result.Success(response);
                }
                catch (Exception e)
                {
                    result = Result.Fail(e.Message);
                }
                finally
                {
                    var responseBytes = _busSerializer.Serialize(result);
                    _connection.Channel.BasicPublish(
                        _options.Value.Exchange,
                        props.ReplyTo,
                        replyProps,
                        responseBytes);
                    _connection.Channel.BasicAck(
                        @event.DeliveryTag,
                        false);
                }
            };
        }

        public void Stop()
        {
            _connection.Channel.BasicCancel(_consumerTag);
        }
    }
}