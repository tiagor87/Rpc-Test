using System;
using MediatR;
using Shared;
using Shared.Bus.Clients;
using Shared.Bus.Connections;

namespace Ping
{
    public class Request : IRequest<Message>
    {
        public string Value { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (IBusRpcClient client =
                new RabbitMQConnection("amqp://guest:guest@localhost", new JsonBusSerializer()))
            {
                try
                {
                    var name = Console.ReadLine();
                    do
                    {
                        var response = client.Send<Request, Message>("", "rpc_queue", new Request {Value = name});
                        Console.WriteLine(response.Value);
                        name = Console.ReadLine();
                    } while (name != "exit");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}