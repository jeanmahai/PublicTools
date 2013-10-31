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
            //const string CARRIAGE_RETURN = "\r\n";
            //const string DOUBLE_CARRIAGE_RETURN = "\r\n\r\n";

            StringBuilder sb = new StringBuilder();

            //String sBuffer = "";
            //string responseText = string.Format("hello world: {0}", DateTime.Now.ToString());
            //int contentLength = !string.IsNullOrEmpty(ResponseText) ? ResponseText.Length : 0;
            int contentLength = Encoding.UTF8.GetByteCount(ResponseText);
            // if Mime type is not provided set default to text/html
            if (string.IsNullOrEmpty(ContentType))
            {
                //ContentType = "text/plain";  // Default Mime Type is text/html
                ContentType = "text/html";
            }

            sb.Append(string.Format("HTTP/{0} {1}\r\n", HTTP_VERSION, status, ""));
            sb.Append(string.Format("Server: {0}\r\n",Context.Server._Config.ServerName));
            sb.Append(string.Format("Content-Type: {0}\r\n", ContentType));
            sb.Append("Accept-Ranges: bytes\r\n");
            sb.Append(string.Format("Content-Length: {0}\r\n\r\n",contentLength));

            //Byte[] bSendData = Encoding.ASCII.GetBytes(sb.ToString());
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
            ResponseText = text;
            WriteToClient(status);
        }
    }
}
