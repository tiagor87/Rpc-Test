namespace Shared.Events.Options
{
    public class BusPublishOptions<TEvent>
    {
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
    }
}