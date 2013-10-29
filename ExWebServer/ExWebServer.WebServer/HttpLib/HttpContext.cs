using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExWebServer.SocketBase.Client;
using ExWebServer.SocketBase.Protocals;

namespace ExWebServer.WebServer.HttpLib
{
    public class HttpContext
    {
        public HttpServer Server { get; set; }
        public IClient Client { get; set; }
        public HttpRequest Request { get; set; }
        private HttpResponse _Response { get; set; }
        private readonly object responseCreateLock = new object();

        public int ClientID { get { return Client.nSessionID; } }

        //public HttpContext(HttpServer serv, IClient client) 
        //{
        //    if (serv == null || client == null)
        //        throw new Exception("HttpServer and HttpClient cannot be null.");
        //    Server = serv;
        //    Client = client;
        //    //Requests = Queue.Synchronized(new Queue());
        //    Request = null;
        //    _Response = new HttpResponse(this);
        //    _Response.NotifyClient += new ClientNotifEventHandler(Server.NotifyClient);
        //}

        public HttpContext(HttpServer serv, IClient client,HttpRequest request)
        {
            if (serv == null || client == null || request == null)
                throw new Exception("HttpServer HttpClient and HttpRequest cannot be null.");
            Server = serv;
            Client = client;
            //Requests = Queue.Synchronized(new Queue());
            Request = request;
            _Response = new HttpResponse(this);
            _Response.NotifyClient += new ClientNotifEventHandler(Server.NotifyClient);
        }

        public HttpResponse Response
        {
            get
            {
                return _Response;
            }
        }

        public bool AppendRequest(HttpRequest request)
        {
            if (Server == null || request == null)
                return false;

            Request = request;

            //lock (Requests.SyncRoot)
            //{
            //    //  如果超过容量，弹出最旧的
            //    if (Requests.Count >= Server.MaxQueuedRequests)
            //        Requests.Dequeue();

            //    Requests.Enqueue(request);
            //}
            
            return true;
        }

        public HttpRequest GetNextRequest()
        {
            return Request;
            //HttpRequest request = null;
            //try
            //{
            //    request = (HttpRequest)Requests.Dequeue();
            //}
            //catch { }
            //return request;
        }
    }
}
