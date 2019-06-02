using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Shared;

namespace Consumer
{
    class RequestHandler : IRequestHandler<Request, Message>
    {
        public Task<Message> Handle(Request request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Message
            {
                Value = $"Hello {request.Value}"
            });
        }
    }
}