using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Shared.Bus.Clients;
using Shared.Events.Options;

namespace Shared.Events.Handlers
{
    public class BusEventHandler<TEvent> : INotificationHandler<IBusEvent<TEvent>>
    {
        private readonly IBusPublishClient _bus;
        private readonly IOptions<BusPublishOptions<TEvent>> _options;

        public BusEventHandler(IBusPublishClient bus, IOptions<BusPublishOptions<TEvent>> options)
        {
            _bus = bus;
            _options = options;
        }

        public Task Handle(IBusEvent<TEvent> notification, CancellationToken cancellationToken)
        {
            _bus.Publish(_options.Value.Exchange, _options.Value.RoutingKey, notification);
            return Task.CompletedTask;
        }
    }
}