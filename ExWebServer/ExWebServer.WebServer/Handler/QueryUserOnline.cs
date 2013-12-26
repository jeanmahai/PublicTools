using System;
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
    public class QueryUserOnline : HttpHandlerBase
    {
        public QueryUserOnline(HttpServer server, CometCommand command) : base(server, command) { }

        public override void HandleRequest(ClientManager clmngr, HttpLib.HttpContext context)
        {
            if (clmngr == null || context == null || context.Request == null)
                return;

            UserOnlineStatusCompact[] result = null;
            string responseText = "";
            HttpResponse response = context.Response;

            try
            {
                string test = JsonHelper.ObjToJson<int[]>(new int[] { 1, 2 });

                CompactUserIDList[] temp = new CompactUserIDList[] { new CompactUserIDList(), new CompactUserIDList(51231) };
                string tempString = JsonHelper.ObjToJson<CompactUserIDList[]>(temp);
                HttpRequest request = context.Request;

                string uidListString = System.Web.HttpUtility.UrlDecode(request.Parameters["uid"]);

                if (!string.IsNullOrEmpty(uidListString))
                {
                    int[] userIDList = JsonHelper.JsonToObj<int[]>(uidListString);
                    if (userIDList != null && userIDList.Length > 0)
                    {
                        result = new UserOnlineStatusCompact[userIDList.Length];
                        UserOnlineInfo onlineInfo = null;
                        for (int i = 0; i < userIDList.Length; i++)
                        {
                            int uid = userIDList[i];
                            onlineInfo = Server.GetUserOnlineInfo(uid);
                            result[i] = new UserOnlineStatusCompact();
                            result[i].U = uid;
                            if (onlineInfo == null)
                            {
                                result[i].L = (int)OnlineCodeValue.Offline;                 //  online code
                                result[i].P = 0;                                            //  position
                            }
                            else
                            {
                                result[i].L = onlineInfo.IsOnline ? 1 : 0;                   //  online code
                            }
                        }
                    }
                }
                responseText = JsonHelper.ObjToJson<UserOnlineStatusCompact[]>(result);
            }
            catch
            {
                responseText = "";
            }
            finally
            {

            }
            response.Write(responseText);
        }
    }
}
