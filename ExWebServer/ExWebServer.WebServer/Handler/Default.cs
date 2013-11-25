using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExWebServer.WebServer.Comet;
using ExWebServer.SocketBase.Client;
using ExWebServer.WebServer.HttpLib;

namespace ExWebServer.WebServer.Handler
{
    public class Default : HttpHandlerBase
    {
        public Default(HttpServer server, CometCommand command) : base(server, command) { }

        public override void HandleRequest(ClientManager clmngr, HttpContext context)
        {
            if (clmngr == null || context == null || context.Request == null)
                return;

            HttpResponse response = context.Response;

            string result = "{\"Title\":\"Message\", \"Content\":\"Welcome!\"}";
            response.Write(result);
        }
    }
}
