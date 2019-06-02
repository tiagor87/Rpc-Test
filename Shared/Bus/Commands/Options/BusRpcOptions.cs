using MediatR;

namespace Shared.Bus.Commands.Options
{
    public class BusRpcOptions<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
    }
}