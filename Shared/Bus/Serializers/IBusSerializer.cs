namespace Shared
{
    public interface IBusSerializer
    {
        byte[] Serialize<TRequest>(TRequest request);

        TResponse Deserialize<TResponse>(byte[] responseData);
    }
}