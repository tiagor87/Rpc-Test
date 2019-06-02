using System;
using MediatR;

namespace Shared.Bus.Clients
{
    public interface IBusRpcClient : IDisposable
    {
        TResponse Send<TRequest, TResponse>(string exchange, string routingKey, TRequest request)
            where TRequest : IRequest<TResponse>;
    }
}