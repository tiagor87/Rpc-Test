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

        public TResponse Deserialize<TResponse>(byte[] responseData)
        {
            var json = Encoding.UTF8.GetString(responseData);
            return (TResponse) JsonConvert.DeserializeObject(json, typeof(TResponse));
        }
    }
}