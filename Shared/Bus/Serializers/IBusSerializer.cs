using System;

namespace Shared
{
    public interface IBusSerializer
    {
        byte[] Serialize<TRequest>(TRequest request);

        TResponse Deserialize<TResponse>(byte[] data);
        object Deserialize(Type type, byte[] data);
    }
}