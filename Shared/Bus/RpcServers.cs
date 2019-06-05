using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Bus.Servers;

namespace Shared.Bus
{
    public class RpcServers : IHostedService
    {
        private readonly IServiceScope _scope;

        public RpcServers(IServiceProvider provider)
        {
            _scope = provider.CreateScope();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var rpcServers = _scope.ServiceProvider.GetServices<IBusRpcServer>();
                foreach (var server in rpcServers)
                {
                    server.Start();
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.FromException(ex);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            var rpcServers = _scope.ServiceProvider.GetService<IEnumerable<IBusRpcServer>>();
            foreach (var server in rpcServers)
            {
                server.Stop();
            }

            return Task.CompletedTask;
        }
    }
}