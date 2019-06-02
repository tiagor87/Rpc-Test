using System;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Bus.Clients;
using Shared.Bus.Commands.Handlers;
using Shared.Bus.Connections;

namespace Shared.Bus.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddBusMediatR(this IServiceCollection services, IConfiguration configuration)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            services
                .AddMediatR(assemblies)
                .AddScoped(typeof(IRequestHandler<,>), typeof(RpcHandler<,>))
                .AddSingleton(
                    new RabbitMQConnection(
                        configuration.GetConnectionString("RabbitMQ"),
                        new JsonBusSerializer()));
            services.AddSingleton<IBusPublishClient>(provider => provider.GetService<RabbitMQConnection>());
            services.AddSingleton<IBusRpcClient>(provider => provider.GetService<RabbitMQConnection>());
        }
    }
}