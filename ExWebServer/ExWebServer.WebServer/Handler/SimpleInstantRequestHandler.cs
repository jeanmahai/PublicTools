using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExWebServer.SocketBase.Client;
using ExWebServer.WebServer.Comet;
using ExWebServer.WebServer.HttpLib;
using System.Threading;

namespace ExWebServer.WebServer.Handler
{
    public class SimpleInstantRequestHandler : HttpHandlerBase
    {
        public SimpleInstantRequestHandler(HttpServer server, CometCommand command) : base(server, command) { }

        public override void HandleRequest(ClientManager clmngr, HttpLib.HttpContext context)
        {
            if (clmngr == null || context == null || context.Request == null)
                return;

            HttpResponse response = context.Response;
            HttpRequest _request = context.GetNextRequest();

            //允许发生消息ID列表
            //List<string> allowIDList = new List<string>(100);
            //allowIDList.Add("129037658");
            //allowIDList.Add("123377431");
            //allowIDList.Add("116748772");

            string output = "";
            //string jjuserid = (_request.Cookies != null && _request.Cookies.AllKeys.Contains("User_Id")) ? _request.Cookies["User_Id"] : "0";
            //string siteStr = _request.Parameters != null && !string.IsNullOrEmpty(_request.Parameters["site"]) ? _request.Parameters["site"] : "";
            //string posStr = _request.Parameters != null && !string.IsNullOrEmpty(_request.Parameters["pos"]) ? _request.Parameters["pos"] : "";

            //try 
            //{
            //    int uid = Int32.Parse(jjuserid);
            //    int siteID = Int32.Parse(siteStr);
            //    int posID = Int32.Parse(posStr);

            //    this.Server.RegistUserPosition(uid, siteID, posID);
            //}
            //catch { }

            //foreach (string allowID in allowIDList)
            //{
            //    if (string.Compare(jjuserid, allowID, true) == 0)
            //    {
            //        output = GetRandomMsgByJson();
            //        //output = GetMSMQMsgByJson();
            //        break;
            //    }
            //}

            output = GetRandomMsgByJson();
            //if (_request != null && _request.Parameters != null && _request.Parameters.AllKeys.Contains("callback"))
            //{
            //    output = string.Format("{0}({1})", _request.Parameters["callback"], output);
            //}

            Thread.Sleep(3000);
            output = string.Format("{0}({1})", _request.Parameters["callback"], output);
            response.Write(output);
            //response.Write("来自InstantRequestHandler");
        }

        private string GetMSMQMsgByJson()
        {
            string mqMsg = "";
            try
            {
                lock (Server.mqBodys)
                {
                    if (Server.mqBodys.Count > 0)
                    {
                        mqMsg = Server.mqBodys.Dequeue().ToString();
                    }
                }
            }
            catch
            { }
            return mqMsg;
        }
        private string GetRandomMsgByJson()
        {
            string output = "";

            // 随机产生消息数和消息
            int messageNotifyType = 1;
            int messageCnt = 1;
            int messageTitleNo = 1;
            int messageBodyNo = 1;
            Random rdm = new Random();
            messageNotifyType = (new Random()).Next(1, 4);
            if (messageNotifyType > 1)
            {
                messageTitleNo = (new Random()).Next(1, 100);
                messageBodyNo = (new Random()).Next(1, 100);
            }
            else
            {
                messageCnt = (new Random()).Next(1, 50);
            }

            output = string.Format("\"messageNotifyType\":{0},\"messageCnt\":{1},\"messageTitle\":\"test{2}\",\"messageBody\":\"Hello, No.{3}!\"", messageNotifyType, messageCnt, messageTitleNo, messageBodyNo);
            output = "{" + output + "}";

            return output;
        }
    }
}
