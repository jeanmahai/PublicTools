using ExWebServer.SocketBase.Client;

namespace ExWebServer.SocketBase.Protocals
{
    public enum SendMessageChannelType
    {
        Notify = 1,
        Broadcast = 2
    }

    public enum ResponsePolicy
    {
        DoNothing = 0,
        SendCommand = 1,
        Shutdown = 2,
        Forward = 4
    }

    public class ClientWorkingPolicy
    {
        public ResponsePolicy Policy = ResponsePolicy.DoNothing;
        public ClientManager Client = null;
        public ISocketMessage Message = null;
        public bool NoDelay = false;

        public ClientWorkingPolicy()
        {

        }

        public ClientWorkingPolicy(ResponsePolicy policy, ClientManager client, ISocketMessage msg)
        {
            this.Policy = policy;
            this.Client = client;
            this.Message = msg;
            this.NoDelay = false;
        }

        public ClientWorkingPolicy(ResponsePolicy policy, ClientManager client, ISocketMessage msg, bool noDelay)
        {
            this.Policy = policy;
            this.Client = client;
            this.Message = msg;
            this.NoDelay = noDelay;
        }
    }
}
