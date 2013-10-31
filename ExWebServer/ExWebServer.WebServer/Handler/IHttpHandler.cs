using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExWebServer.SocketBase.Client;
using ExWebServer.WebServer.Comet;

namespace ExWebServer.WebServer.Handler
{
    public interface IHttpHandler
    {
        HttpServer Server { get; set; }
        CometCommand Command { get; set; }

        void HandleRequest(ClientManager clmngr, HttpLib.HttpContext context);
    }
}
