using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExWebServer.WebServer.Comet;
using ExWebServer.SocketBase.Client;

namespace ExWebServer.WebServer.Handler
{
    public class HttpHandlerBase : IHttpHandler
    {
        public HttpServer Server { get; set; }
        public CometCommand Command { get; set; }

        public HttpHandlerBase() { }

        public HttpHandlerBase(HttpServer server, CometCommand command)
        {
            if (server == null)
                throw new Exception("HttpServer cannot be null.");

            if (command == null)
                throw new Exception("CometCommand cannot be null.");

            Server = server;
            Command = command;
        }

        public virtual void HandleRequest(ClientManager clmngr, HttpLib.HttpContext context)
        {

        }
    }
}
