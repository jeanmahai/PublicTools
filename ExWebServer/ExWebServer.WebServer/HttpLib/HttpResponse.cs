using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExWebServer.SocketBase.Protocals;

namespace ExWebServer.WebServer.HttpLib
{
    public class HttpResponse
    {
        public HttpContext Context { get; set; }
        public NameValueCollection Headers { get; set; }
        public NameValueCollection Cookies { get; set; }
        public string ContentType { get; set; }
        public Int64 ContentLength { get; set; }
        public string ResponseText { get; set; }

        public event ClientNotifEventHandler NotifyClient;

        public HttpResponse(HttpContext context)
        {
            if (context == null)
                throw new Exception("HttpContext cannot be null.");

            Context = context;
        }

        private void WriteToClient()
        {
            WriteToClient(200);
        }

        private void WriteToClient(int status)
        {
            const string HTTP_VERSION = "1.1";

            StringBuilder sb = new StringBuilder();

            int contentLength = Encoding.UTF8.GetByteCount(ResponseText);
            if (string.IsNullOrEmpty(ContentType))
            {
                ContentType = "text/json; charset=utf-8";
            }

            sb.Append(string.Format("HTTP/{0} {1}\r\n", HTTP_VERSION, status, ""));
            sb.Append(string.Format("Server: {0}\r\n",Context.Server._Config.ServerName));
            sb.Append(string.Format("Content-Type: {0}\r\n", ContentType));
            sb.Append("Accept-Ranges: bytes\r\n");
            sb.Append(string.Format("Content-Length: {0}\r\n\r\n", contentLength));

            sb.Append(ResponseText);
            SendToBrowser(sb.ToString());
        }

        public void SendToBrowser(string text)
        {
            SendToBrowser(0, text);
        }

        public void SendToBrowser(int sender,string text)
        {
            if (NotifyClient == null || string.IsNullOrEmpty(text))
                return;
            ClientNotifEventArgs arg = new ClientNotifEventArgs(sender,new int[]{Context.ClientID},new HttpRawMessage(text));
            NotifyClient(this, arg);
        }

        public void Write(string text)
        {
            Write(200, text);
        }

        public void Write(int status,string text)
        {
            #region Jsonp请求处理
            HttpRequest _request = Context.Request;
            if (_request != null && _request.Parameters != null && _request.Parameters.AllKeys.Contains("callback"))
            {
                text = string.Format("{0}({1})", _request.Parameters["callback"], text);
            }
            #endregion

            ResponseText = text;
            WriteToClient(status);
        }
    }
}
