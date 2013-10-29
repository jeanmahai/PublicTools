using System;

using ExWebServer.SocketBase.Client;

namespace ExWebServer.SocketBase.Protocals
{
    public class ClientNotifEventArgs : EventArgs
    {
        public int Sender { get; set; }
        public int[] Recvs { get; set; }
        public ISocketMessage Command { get; set; }

        public ClientNotifEventArgs(int sender, int[] uids, ISocketMessage cmd)
        {
            this.Sender = sender;
            this.Recvs = uids;
            this.Command = cmd;
        }
    }

    public class ClientShutDownEventArgs : EventArgs
    {
        public IClient Client { get; set; }

        public ClientShutDownEventArgs(IClient client)
        {
            if (client == null)
                throw new Exception("Client Object cannot be null.");
            Client = client;
        }
    }

    //发给客户端的消息
    public delegate CommonResult ClientNotifEventHandler(object sender, ClientNotifEventArgs e);
    public delegate ClientManager QueryClientManagerObject(object sender, int uid);
    //通知服务器shutdown client
    public delegate void ShutDownClientEventHandler(object sender, ClientShutDownEventArgs e);
}
