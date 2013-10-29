using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExWebServer.SocketBase.Client;
using ExWebServer.WebServer.Comet;
using ExWebServer.WebServer.HttpLib;
using ExWebServer.WebServer.Module.OnlineUser;
using Common.Utility.Json;

namespace ExWebServer.WebServer.Handler
{
    public class QueryUserOnlineList : HttpHandlerBase
    {
        public QueryUserOnlineList(HttpServer server, CometCommand command) : base(server, command) { }

        public override void HandleRequest(ClientManager clmngr, HttpLib.HttpContext context)
        {
            if (clmngr == null || context == null || context.Request == null)
                return;

            string responseText = "";
            HttpResponse response = context.Response;

            string[] result = new string[2];

            try
            {
                string userList = "";
                UserOnlineInfo[] userInfo = null;
                int onlineUsers = 0;

                lock (Server._Users.SyncRoot)
                {
                    if (Server._Users.Count > 0)
                    {
                        onlineUsers = Server._Users.Count;
                        if (context.GetNextRequest().Parameters.AllKeys.Contains("list"))
                        {
                            int i = 0;
                            userInfo = new UserOnlineInfo[Server._Users.Count];
                            foreach (UserOnlineInfo u in Server._Users.Values)
                            {
                                userInfo[i++] = u;
                            }
                            userList = JsonHelper.ObjToJson<UserOnlineInfo[]>(userInfo);
                        }
                    }
                }
                result[0] = onlineUsers.ToString();
                result[1] = userList;
                responseText = JsonHelper.ObjToJson<string[]>(result);
            }
            catch (Exception ex)
            {
                responseText = ex.Message;
            }
            finally { }

            response.Write(responseText);
        }
    }
}
