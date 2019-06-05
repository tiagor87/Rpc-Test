using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Shared;

namespace Consumer
{
    public class Request : IRequest<Response>
    {
        public string Value { get; set; }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        public Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Response()
            {
                Value = $"Hello {request.Value}"
            });
        }
    }
}