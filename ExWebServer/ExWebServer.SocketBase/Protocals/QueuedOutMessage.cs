using ExWebServer.SocketBase.Client;

namespace ExWebServer.SocketBase.Protocals
{
    public class QueuedOutMessage
    {
        public ClientManager Client { get; set; }
        public ISocketMessage Command { get; set; }
        public uint Length { get; set; }
        public bool ShutdownClientAfterSend { get; set; }

        public QueuedOutMessage(ClientManager clmngr, ISocketMessage msg)
        {
            this.Client = clmngr;
            this.Command = msg;
            this.Length = 0;
            this.ShutdownClientAfterSend = false;
        }

        public QueuedOutMessage(ClientManager clmngr, ISocketMessage msg, bool shutdown)
        {
            this.Client = clmngr;
            this.Command = msg;
            this.Length = 0;
            this.ShutdownClientAfterSend = shutdown;
        }

        public QueuedOutMessage()
        {
            this.Length = 0;
            this.ShutdownClientAfterSend = false;
        }
    }
}
