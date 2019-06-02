using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Shared
{
    public class JsonSerializer : ISerializer
    {
        public Task<byte[]> Serialize<T>(T data)
        {
            return Task.Factory.StartNew(() =>
            {
                var json = JsonConvert.SerializeObject(data);
                return Encoding.UTF8.GetBytes(json);
            });
        }

        public Task<T> Deserialize<T>(byte[] data)
        {
            return Task.Factory.StartNew(() =>
            {
                var json = Encoding.UTF8.GetString(data);
                return (T) JsonConvert.DeserializeObject(json, typeof(T));
            });
        }
    }
}