using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExWebServer.SocketBase.Client;
using ExWebServer.WebServer.Comet;
using ExWebServer.WebServer.HttpLib;

namespace ExWebServer.WebServer.Handler
{
    public class SimpleCometRequestHandler : HttpHandlerBase
    {
        public SimpleCometRequestHandler(HttpServer server, CometCommand command) : base(server, command) { }

        public override void HandleRequest(ClientManager clmngr, HttpLib.HttpContext context)
        {
            if (clmngr == null || context == null)
                return;

            HttpResponse response = context.Response;

            DateTime dtEnd = DateTime.Now.AddMilliseconds(3000);
            while (true)
            {
                if (DateTime.Now > dtEnd)
                    break;
                Thread.Sleep(50);
            }

            response.Write("{\"Success\":\"True\"}");
        }
    }
}
