using System;
using System.Linq;
using System.Web.Script.Serialization;
using WebSocket.WebSocket.Server;

namespace Server
{
    public class MyWebSocket : SupperWebSocketServerBase
    {
        private JavaScriptSerializer m_Json=new JavaScriptSerializer();
        public MyWebSocket()
        {
            this.OnNewMessage += new NewMessageReceivedHandler(MyWebSocket_OnNewMessage);
        }

        void MyWebSocket_OnNewMessage(SuperWebSocket.WebSocketSession session, WebSocketRequest request, WebSocketResponse response)
        {
            response.RemoteHandler = string.Format(@"showMessage('{0}')", response.Data);
            var res = m_Json.Serialize(response);
            //session.Send(res);
            SendToAll(res);
        }

        protected override void OnNewSessionConnected(SuperWebSocket.WebSocketSession session)
        {
            var res = new WebSocketResponse();
            res.RemoteHandler = string.Format(@"showMessage('{0} 欢迎加入')", DateTime.Now);
            var msg = m_Json.Serialize(res);
            SendToAll(msg);
        }
        protected override void OnSessionClosed(SuperWebSocket.WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            var res = new WebSocketResponse();
            res.RemoteHandler = string.Format(@"showMessage('{0} 用户退出')", DateTime.Now);
            var msg = m_Json.Serialize(res);
            SendToAll(msg);
        }

        void SendToAll(string message)
        {
            foreach (var s in Sessions)
            {
                s.Send(message);
            }
        }
    }
}