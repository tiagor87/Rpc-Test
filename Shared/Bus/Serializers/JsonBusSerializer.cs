using System;
using System.Text;
using Newtonsoft.Json;

namespace Shared
{
    public class JsonBusSerializer : IBusSerializer
    {
        public byte[] Serialize<TRequest>(TRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            return Encoding.UTF8.GetBytes(json);
        }

        public TResponse Deserialize<TResponse>(byte[] data)
        {
            return (TResponse) Deserialize(typeof(TResponse), data);
        }

        public object Deserialize(Type type, byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject(json, type);
        }
    }
}