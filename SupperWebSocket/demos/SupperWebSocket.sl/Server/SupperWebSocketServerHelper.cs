using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using SuperWebSocket;

namespace Server
{
    public class SupperWebSocketServerHelper
    {
        private WebSocketServer m_WebSocketServer;

        private IBootstrap m_Bootstrap;

        public void Start()
        {
            m_Bootstrap = BootstrapFactory.CreateBootstrap();
            if(!m_Bootstrap.Initialize())
            {
                return;
            }
            var server = m_Bootstrap.AppServers.FirstOrDefault(s => s.Name.Equals("SuperWebSocket"))
                         as WebSocketServer;
            if(server==null)
            {
                return;
            }
            server.NewMessageReceived += new SessionHandler<WebSocketSession, string>(server_NewMessageReceived);
            server.NewSessionConnected += new SessionHandler<WebSocketSession>(server_NewSessionConnected);
            server.SessionClosed += new SessionHandler<WebSocketSession, CloseReason>(server_SessionClosed);
            server.NewDataReceived += new SessionHandler<WebSocketSession, byte[]>(server_NewDataReceived);
            //server.NewRequestReceived += new RequestHandler<WebSocketSession, SuperWebSocket.Protocol.IWebSocketFragment>(server_NewRequestReceived);

            m_WebSocketServer = server;
            m_Bootstrap.Start();
        }

        public void Stop()
        {
            if(m_Bootstrap!=null)
            {
                m_Bootstrap.Stop();
            }
        }

        public IEnumerable<WebSocketSession> Sessions
        {
            get { return m_WebSocketServer.GetAllSessions(); }
        } 

        public WebSocketSession GetSession()
        {
            throw new NotImplementedException();
        }

        //void server_NewRequestReceived(WebSocketSession session, SuperWebSocket.Protocol.IWebSocketFragment requestInfo)
        //{
            
        //}

        void server_NewDataReceived(WebSocketSession session, byte[] value)
        {
        }

        void server_SessionClosed(WebSocketSession session, CloseReason value)
        {
        }

        void server_NewSessionConnected(WebSocketSession session)
        {
            session.Send(string.Format("welcome! 当前连接: {0}",Sessions.Count()));
        }

        void server_NewMessageReceived(WebSocketSession session, string value)
        {
        }
        
    }
}