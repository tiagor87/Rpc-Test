using Shared.Events;

namespace Api
{
    public class Notification1 : IBusEvent<Notification1>
    {
        public Notification1()
        {
            Value = nameof(Notification1);
        }

        public string Value { get; set; }
    }

    public class Notification2 : IBusEvent<Notification2>
    {
        public Notification2()
        {
            Value = nameof(Notification2);
        }

        public string Value { get; set; }
    }
}