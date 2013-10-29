using System;
using System.Collections.Generic;

using ExWebServer.WebServer.Handler;

namespace ExWebServer.WebServer.Comet
{
    public class CometCommandHandlerPipeline
    {
        public HttpServer Server { get; set; }
        public CometCommand Command { get; set; }
        public List<IHttpHandler> Handlers { get; set; }

        public int Count { get { return Handlers != null ? Handlers.Count : 0; } }

        public CometCommandHandlerPipeline(HttpServer server)
        {
            if (server == null)
                throw new Exception("HttpServer cannot be null.");

            Server = server;
        }

        public CometCommandHandlerPipeline(HttpServer server, CometCommand cmd)
        {
            if (server == null)
                throw new Exception("HttpServer cannot be null.");

            if (cmd == null)
                throw new Exception("CometCommand cannot be null.");

            Server = server;
            Command = cmd;
        }

        public void RegistHandler(IHttpHandler handler)
        {
            if (handler == null)
                return;

            if (Handlers == null)
                Handlers = new List<IHttpHandler>();

            Handlers.Add(handler);
        }
    }
}
