using System;
using Shared;

namespace Pong
{
    class HelloWorldRpcServer : RpcServer<Message, Message>
    {
        public HelloWorldRpcServer(string connectionString) : base(connectionString, new JsonBusSerializer())
        {
        }

        protected override Message Execute(Message request)
        {
            return new Message()
            {
                Value = $"Hello {request.Value}"
            };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var server = new HelloWorldRpcServer("amqp://guest:guest@localhost"))
            {
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}