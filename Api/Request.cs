using MediatR;
using Shared;

namespace Api
{
    public class Request : IRequest<Message>
    {
        public Request(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
    }
}