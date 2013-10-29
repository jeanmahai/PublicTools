using System;
using System.Globalization;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace ExWebServer.WebServer.HttpLib
{
    public class HttpRequest
    {
        public static long RequestIDSeed = 0;

        public long ID { get; set; }
        public HttpServer Server { get; set; }

        public NameValueCollection Headers { get; set; }
        public NameValueCollection Parameters { get; set; }
        public NameValueCollection Cookies { get; set; }
        public HttpMethod Method { get; set; }
        public string HttpVersion { get; set; }
        public string ContentType { get; set; }
        public Int64 ContentLength { get; set; }
        public Uri PathUri { get; set; }
        public ProcessingState ProcState { get; set; }
        public RequestError Error { get; set; }
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Accept { get; set; }
        public string AcceptLanguage { get; set; }
        public string UserAgent { get; set; }
        public ConnectionMode ConnMode { get; set; }
        public DateTime IfModifiedSince { get; set; }
        public DateTime IfUnModifiedSince { get; set; }
        public ByteRange[] Range { get; set; }

        /// <summary>
        /// CometCommand
        /// </summary>
        public Comet.CometCommand Command { get; set; }

        public bool IsRequestError { get { return Error != null && Error.StatusCode != 200 ? true : false; } }


        public HttpRequest(HttpServer serv)
        {
            if (serv == null)
                throw new Exception("HttpServer cannot be null.");
            ID = Interlocked.Increment(ref RequestIDSeed);
            Server = serv;
        }

        public void RequestProcOK()
        {
            int code = 200;
            if (Error == null)
                Error = new RequestError(200);
            else
            {
                Error.StatusCode = code;
                Error.Message = HttpUtility.GetStatusCodeDescriptions(code);
            }

            ProcState = ProcessingState.RequestProcCompleted;
        }


        public void LogRequestError(int code)
        {
            if (Error == null)
                Error = new RequestError(code);
            else
            {
                Error.StatusCode = code;
                Error.Message = HttpUtility.GetStatusCodeDescriptions(code);
            }
        }
        
        public static HttpRequest ParseFromRawMessage(HttpServer server, string raw)
        {
            if (server == null || string.IsNullOrEmpty(raw))
                throw new Exception("HttpServer or raw message cannot be null.");

            HttpRequest request = new HttpRequest(server);
            string path = "/";
            string line = string.Empty;
            try
            {
                //  first of all,read request line
                using(StringReader sr = new StringReader(raw))
                {
                    string requestLine = sr.ReadLine();
                    string[] protocol = requestLine.Split(' ');
                    if (protocol == null || protocol.Length != 3)
                    {
                        request.LogRequestError(400);
                        return request;
                    }

                    request.ProcState = ProcessingState.RequestLine;
                    switch (protocol[0])
                    { 
                        case "GET":
                            request.Method = HttpMethod.GET;
                            break;
                        case "POST":
                            request.Method = HttpMethod.POST;
                            break;
                        case "PUT":
                            request.Method = HttpMethod.PUT;
                            break;
                        case "DELETE":
                            request.Method = HttpMethod.DELETE;
                            break;
                        case "HEAD":
                            request.Method = HttpMethod.HEAD;
                            break;
                        default:
                            request.Method = HttpMethod.NA;
                            break;
                    }
                    if (protocol[1].Length > 2500)
                    {
                        request.LogRequestError(414);
                        return request;
                    }

                    //request.PathBuilder = new UriBuilder(protocol[1]);
                    path = protocol[1];

                    if (!protocol[2].StartsWith("HTTP/") || !(protocol[2].Length > "HTTP/".Length))
                    {
                        request.LogRequestError(400);
                        return request;
                    }
                    request.HttpVersion = protocol[2].Substring("HTTP/".Length);

                    //  parse request line completed,prepared to read headers
                    request.ProcState = ProcessingState.RequestHeaders;

                    if (request == null || request.ProcState != ProcessingState.RequestHeaders)
                        return request;

                    //  read headers
                    if (request.Headers == null)
                        request.Headers = new NameValueCollection();

                    line = sr.ReadLine();
                    while (!string.IsNullOrEmpty(line) && request.Headers.Count < server.MaxHeaderLines)
                    {
                        int colonIndex = line.IndexOf(":");
                        if (colonIndex <= 1)
                        {
                            line = sr.ReadLine();
                            continue;
                        }
                        string val = line.Substring(colonIndex + 1).Trim();
                        string name = line.Substring(0, colonIndex).Trim();

                        request.Headers[name] = val;

                        switch (name.ToLower(CultureInfo.InvariantCulture))
                        {
                            case "host":
                                request.Host = val;
                                request.PathUri = new Uri(string.Format("http://{0}{1}", val, path));
                                request.Parameters = HttpUtility.ParseParametersFromQueryString(request.PathUri.Query);
                                break;
                            case "authorization":
                                if (val.Length < 6)
                                    break;

                                string encoded = val.Substring(6, val.Length - 6);
                                byte[] byteAuth;
                                try
                                {
                                    byteAuth = Convert.FromBase64String(encoded);
                                }
                                catch (FormatException)
                                {
                                    break;
                                }

                                string[] strings = Encoding.UTF8.GetString(byteAuth).Split(':');
                                if (strings.Length != 2)
                                    break;

                                request.Username = strings[0];
                                request.Password = strings[1];

                                break;
                                
                            case "content-type":
                                request.ContentType = val;
                                break;
                            case "content-length":
                                try
                                {
                                    Int64 length = long.Parse(val, NumberStyles.Integer, CultureInfo.InvariantCulture);
                                    if (length < 0)
                                    {
                                        request.LogRequestError(400);
                                        break;
                                    }
                                    if (length > server.MaxPostLength)
                                    {
                                        request.LogRequestError(413);
                                        break;
                                    }
                                }
                                catch (FormatException)
                                {
                                    request.LogRequestError(413);
                                }
                                break;
                            case "accept":
                                request.Accept = val;
                                break;
                            case "accept-language":
                                request.AcceptLanguage = val;
                                break;
                            case "user-agent":
                                request.UserAgent = val;
                                break;
                            case "connection":
                                if (string.Compare(val, "close", true, CultureInfo.InvariantCulture) == 0)
                                    request.ConnMode = ConnectionMode.Close;
                                else
                                    request.ConnMode = ConnectionMode.KeepAlive;
                                break;
                            case "if-modified-since":
                                try
                                {
                                    request.IfModifiedSince = HttpUtility.ParseHttpTime(val);
                                }
                                catch (FormatException)
                                {
                                }
                                break;
                            case "if-unmodified-since":
                                try
                                {
                                    request.IfUnModifiedSince = HttpUtility.ParseHttpTime(val);
                                }
                                catch (FormatException)
                                {
                                }
                                break;
                            case "range":
                                try
                                {
                                    string[] rangeStrings = val.Split(',');
                                    request.Range = new ByteRange[rangeStrings.Length];
                                    for (int i = 0; i < rangeStrings.Length; i++)
                                        request.Range[i] = new ByteRange(rangeStrings[i]);
                                }
                                catch (FormatException)
                                {
                                    request.Range = null;
                                }
                                break;
                            case "cookie":
                                request.Cookies = HttpUtility.ParseCookiesFromCookieString(val);
                                break;
                            default:
                                break;
                        }
                        line = sr.ReadLine();
                    }
                    request.RequestProcOK();
                }
            }
            catch
            {
                request.LogRequestError(415);
            }
            finally
            {
                
            }
            
            return request;            
                
        }
    }
}
