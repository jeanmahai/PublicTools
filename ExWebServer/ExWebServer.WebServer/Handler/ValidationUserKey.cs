using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExWebServer.WebServer.Comet;
using ExWebServer.SocketBase.Client;
using ExWebServer.WebServer.HttpLib;

namespace ExWebServer.WebServer.Handler
{
    public class ValidationUserKey : HttpHandlerBase
    {
        public ValidationUserKey() { }
        public ValidationUserKey(HttpServer server, CometCommand command) : base(server, command) { }

        public override void HandleRequest(ClientManager clmngr, HttpContext context)
        {
            if (clmngr == null || context == null || context.Request == null)
                return;

            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            int userId = 0;
            if (request.Parameters != null && request.Parameters.AllKeys.Contains("UserID"))
            {
                int.TryParse(request.Parameters["UserID"], out userId);
            }
            if (userId <= 0)
                throw new BussinessException("请输入用户ID。");
            string key = Guid.NewGuid().ToString();

            string result = "{\"UserID\":\"" + userId + "\", \"Key\":\"" + key + "\"}";
            response.Write(result);
        }
    }
}
