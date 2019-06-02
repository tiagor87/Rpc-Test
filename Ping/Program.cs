using System;
using Shared;

namespace Ping
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = new RpcClient("amqp://guest:guest@localhost", new JsonBusSerializer()))
            {
                try
                {
                    var name = Console.ReadLine();
                    do
                    {
                        var response = client.SendAsync<Message, Message>(new Message() {Value = name}).Result;
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