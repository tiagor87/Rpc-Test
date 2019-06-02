using MediatR;
using Shared;

namespace Consumer
{
    public class Request : IRequest<Message>
    {
        public string Value { get; set; }
    }
}