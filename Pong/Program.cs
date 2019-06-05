using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using Shared.Bus.Servers;

namespace Pong
{
    public class Request : IRequest<Message>
    {
        public string Value { get; set; }
    }
    class HelloWorldRpcServer : IRequestHandler<Request, Message>
    {
        public Task<Message> Handle(Request request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Message()
            {
                Value = $"Hello {request.Value}"
            });
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@localhost")
            };
            
            IBusSerializer busSerializer = new JsonBusSerializer();

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(
                "rpc_queue",
                false,
                false,
                false,
                null);
            channel.BasicQos(
                0,
                1,
                false);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(
                "rpc_queue",
                false,
                consumer);


            consumer.Received += (model, @event) =>
            {
                var body = @event.Body;
                var props = @event.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    @event.BasicProperties.
                    var request = busSerializer.Deserialize<TRequest>(body);
                    response = Execute(request);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e.Message);
                }
                finally
                {
                    var responseBytes = busSerializer.Serialize(response);
                    channel.BasicPublish(
                        "",
                        props.ReplyTo,
                        replyProps,
                        responseBytes);
                    channel.BasicAck(
                        @event.DeliveryTag,
                        false);
                }
            };
        }
    }
}