using System.Collections.Generic;

namespace ExWebServer.SocketBase
{
    public interface ISocketServerConfigure
    {
        string ServerName { get; set; }
        int Port { get; set; }
        int Backlog { get; set; }
        int SendThreads { get; set; }
        HashSet<long> TrustRemoteIP { get; set; }
    }
}
