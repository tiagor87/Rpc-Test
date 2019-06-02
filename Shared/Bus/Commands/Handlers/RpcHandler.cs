using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Shared.Bus.Clients;
using Shared.Bus.Commands.Options;

namespace Shared.Bus.Commands.Handlers
{
    public class RpcHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>

    {
        private readonly IBusRpcClient _busClient;
        private readonly IOptions<BusRpcOptions<TRequest, TResponse>> _options;

        public RpcHandler(IBusRpcClient busClient, IOptions<BusRpcOptions<TRequest, TResponse>> options)
        {
            _busClient = busClient ?? throw new ArgumentNullException(nameof(busClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var response = _busClient.Send<TRequest, TResponse>(
                _options.Value.Exchange,
                _options.Value.RoutingKey,
                request);

            return Task.FromResult(response);
        }
    }
}