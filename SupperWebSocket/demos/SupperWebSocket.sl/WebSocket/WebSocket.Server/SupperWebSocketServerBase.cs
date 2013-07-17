using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using SuperWebSocket;

namespace WebSocket.WebSocket.Server
{
    public class SupperWebSocketServerBase
    {
        //public delegate WebSocketResponse NewMessageReceivedHandler(WebSocketSession session, WebSocketRequest request);

        //public event NewMessageReceivedHandler OnNewMessage;

        private readonly JavaScriptSerializer m_JSON = new JavaScriptSerializer();

        private WebSocketServer m_WebSocketServer;

        private IBootstrap m_Bootstrap;

        private IWebSocketHandler m_Handler;

        private List<Assembly> Assemblies = new List<Assembly>();

        public void Start()
        {
            m_Bootstrap = BootstrapFactory.CreateBootstrap();
            if (!m_Bootstrap.Initialize())
            {
                return;
            }
            var server = m_Bootstrap.AppServers.FirstOrDefault(s => s.Name.Equals("SuperWebSocket"))
                         as WebSocketServer;
            if (server == null)
            {
                return;
            }
            server.NewMessageReceived += new SessionHandler<WebSocketSession, string>(OnNewMessageReceived);
            server.NewSessionConnected += new SessionHandler<WebSocketSession>(OnNewSessionConnected);
            server.SessionClosed += new SessionHandler<WebSocketSession, CloseReason>(OnSessionClosed);
            server.NewDataReceived += new SessionHandler<WebSocketSession, byte[]>(OnNewDataReceived);

            m_WebSocketServer = server;
            m_Bootstrap.Start();
        }

        public void Stop()
        {
            if (m_Bootstrap != null)
            {
                m_Bootstrap.Stop();
            }
        }

        protected IEnumerable<WebSocketSession> Sessions
        {
            get { return m_WebSocketServer.GetAllSessions(); }
        }

        protected WebSocketSession GetSession()
        {
            throw new NotImplementedException();
        }

        protected virtual void OnNewDataReceived(WebSocketSession session, byte[] value)
        {
        }

        protected virtual void OnSessionClosed(WebSocketSession session, CloseReason value)
        {
        }

        protected virtual void OnNewSessionConnected(WebSocketSession session)
        {

        }

        protected virtual void OnNewMessageReceived(WebSocketSession session, string value)
        {
            var request = m_JSON.Deserialize<WebSocketRequest>(value);
            var response = new WebSocketResponse();
            response.Handler = request.ClientCallbackID;
            if (request != null)
            {
                var _response = new StringBuilder();
                var _handler = InitHandler(request.Handler);
                if (_handler != null)
                {
                    //_response.AppendFormat("创建处理程序成功\n");
                    var _result = _handler.Analyze(request, response);
                    if (_result != null)
                    {
                        session.Send(m_JSON.Serialize(_result));
                    }
                    //_response.Append(_result + "\n");
                }
                else
                {
                    _response.AppendFormat("创建处理程序失败\n");
                }
                //session.Send(_response.ToString());
            }
        }

        IWebSocketHandler InitHandler(string handlerName)
        {
            IWebSocketHandler _handler = null;
            //var _type = Type.GetType(handlerName);
            //if(_type!=null)
            //{
            //    _handler = (IWebSocketHandler)Activator.CreateInstance(_type);
            //}
            var infos = handlerName.Split(new[] { ';' });
            //var ass = (from a in Assemblies
            //                 where a.FullName == infos[1]
            //                 select a).SingleOrDefault();
            //if(ass==null)
            //{
            var ass = Assembly.Load(infos[1]);
            //    Assemblies.Add(ass);
            //}
            var _type = ass.GetType(infos[0]);
            _handler = (IWebSocketHandler)Activator.CreateInstance(_type);
            return _handler;
        }

        //IWebSocketHandler InitHandler(string message)
        //{
        //    IWebSocketHandler _handler = null;
        //    var _cmd = m_JSON.Deserialize<WebSocketRequest>(message);
        //    var _type = Type.GetType(_cmd.Handler,true);
        //    if (_type != null)
        //        _handler = (IWebSocketHandler)Activator.CreateInstance(_type);
        //    return _handler;
        //}
    }
}