using System.Threading.Tasks;

namespace Shared
{
    public interface ISerializer
    {
        Task<byte[]> Serialize<T>(T data);

        Task<T> Deserialize<T>(byte[] data);
    }
}