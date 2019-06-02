using MediatR;

namespace Shared.Bus.Clients
{
    public interface IBusRpcClient
    {
        TResponse Send<TRequest, TResponse>()
            where TRequest : IRequest<TResponse>;
    }
}