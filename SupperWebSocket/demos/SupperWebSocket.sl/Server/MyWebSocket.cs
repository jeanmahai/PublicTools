using System.Linq;
using WebSocket.WebSocket.Server;

namespace Server
{
    public class MyWebSocket:SupperWebSocketServerBase
    {
        protected override void OnNewSessionConnected(SuperWebSocket.WebSocketSession session)
        {
            session.Send(string.Format("welcome!"));
            foreach (var s in Sessions)
            {
                s.Send("sdf");
            }
        }
    }
}