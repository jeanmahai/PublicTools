namespace ExWebServer.SocketBase.Protocals
{
    public interface ISocketMessage
    {
        int Length { get; }
        string ToString();
        byte[] GetBytes();
    }
}
