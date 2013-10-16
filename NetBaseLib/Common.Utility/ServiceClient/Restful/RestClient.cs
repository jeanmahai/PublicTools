using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Common.Utility
{
    public class RestClient
    {
        public static Func<Dictionary<string, string>> ContextSetting
        {
            get;
            set;
        }

        private static string s_BaseUrl;
        public static string BaseUrl
        {
            get
            {
                return s_BaseUrl;
            }
            set
            {
                s_BaseUrl = value;
                if (s_BaseUrl != null)
                {
                    s_BaseUrl = s_BaseUrl.Trim().TrimEnd('\\').TrimEnd('/');
                }
            }
        }

        private event EventHandler<BeforeRequestEventArgs> m_BeforeRequestHandler;
        public event EventHandler<BeforeRequestEventArgs> BeforeRequest
        {
            add { m_BeforeRequestHandler += value; }
            remove { m_BeforeRequestHandler -= value; }
        }

        private event EventHandler<AfterResponseEventArgs> m_AfterResponseHandler;
        public event EventHandler<AfterResponseEventArgs> AfterResponse
        {
            add { m_AfterResponseHandler += value; }
            remove { m_AfterResponseHandler -= value; }
        }

        public RestClient()
        {

        }

        protected virtual void OnBeforeRequest(HttpWebRequest request, HttpMethod method)
        {
            EventHandler<BeforeRequestEventArgs> handler = m_BeforeRequestHandler;
            if (handler != null)
            {
                handler(this, new BeforeRequestEventArgs(request, method));
            }
        }

        protected virtual void OnAfterResponse(string responseTxt)
        {
            EventHandler<AfterResponseEventArgs> handler = m_AfterResponseHandler;
            if (handler != null)
            {
                handler(this, new AfterResponseEventArgs(responseTxt));
            }
        }

        protected virtual object DeserializeResponseData(string responseTxt, RequestFormat format, Type type)
        {
            if (responseTxt == null || responseTxt.Trim().Length <= 0)
            {
                return null;
            }
            switch (format)
            {
                case RequestFormat.Json:
                    return SerializationUtility.JsonDeserialize(responseTxt, type);
                case RequestFormat.Xml:
                    return SerializationUtility.XmlDeserialize(responseTxt, type);
                case RequestFormat.Raw:
                    return responseTxt;
                default:
                    return SerializationUtility.BinaryDeserialize(responseTxt);
            }
        }

        protected virtual string SerializeRequestData(object data, RequestFormat format)
        {
            if (data == null)
            {
                return null;
            }
            switch (format)
            {
                case RequestFormat.Json:
                    return SerializationUtility.JsonSerializeCommon(data);
                case RequestFormat.Xml:
                    return SerializationUtility.XmlSerialize(data);
                case RequestFormat.Raw:
                    return data.ToString();
                default:
                    return SerializationUtility.BinarySerialize(data);
            }
        }

        private string CombineUrl(string root, string sub)
        {
            if (root == null)
            {
                root = string.Empty;
            }
            if (sub == null || sub.Trim().Length <= 0)
            {
                return root.TrimEnd(new char[] { '/', '\\' });
            }
            string url = sub.ToLower().Trim();
            if (url.IndexOf("http") == 0)
            {
                return sub;
            }
            return root.TrimEnd(new char[] { '/', '\\' }) + "/" + sub.TrimStart(new char[] { '/', '\\' });
        }

        private string GetContentTypeStr(RequestFormat format)
        {
            switch (format)
            {
                case RequestFormat.Raw:
                    return "application/x-www-form-urlencoded";
                case RequestFormat.Xml:
                    return "application/xml";
                default:
                    return "application/json";
            }
        }

        private string GetHttpMethodStr(HttpMethod method)
        {
            switch (method)
            {
                case HttpMethod.Get:
                    return "GET";
                case HttpMethod.Post:
                    return "POST";
                case HttpMethod.Put:
                    return "PUT";
                default:
                    return "DELETE";
            }
        }

        private void HandleHttpHeader(HttpWebRequest request)
        {
            Dictionary<string, string> dic = null;
            var handler = RestClient.ContextSetting;
            if (handler != null)
            {
                dic = handler();
            }
            if (dic == null)
            {
                return;
            }
            Dictionary<string, string>.Enumerator iterator = dic.GetEnumerator();
            while (iterator.MoveNext())
            {
                string val = iterator.Current.Value;
                if (val == null)
                {
                    continue;
                }
                val = HeaderContextHelper.EncryptAndSign(val);
                request.Headers[HeaderContextHelper.BuildKey(iterator.Current.Key)] = val;
            }
        }

        #region 同步调用

        private HttpWebRequest CreateAndInitHttpRequest(string url, HttpMethod method, RequestFormat format,
            string dataContent, Encoding requestEncoding)
        {
            HttpWebRequest request = WebRequest.Create(new Uri(CombineUrl(BaseUrl, url))) as HttpWebRequest;
            request.Method = GetHttpMethodStr(method);
            string contentType = GetContentTypeStr(format);
            request.Accept = contentType;
            request.ContentType = contentType;
            HandleHttpHeader(request);
            OnBeforeRequest(request, method);
            if (dataContent != null && dataContent.Trim().Length > 0)
            {
                if (method == HttpMethod.Get)
                {
                    throw new ApplicationException("The http method 'GET' can't support data content in http request stream.");
                }
                byte[] aryBuf = requestEncoding.GetBytes(dataContent);
                request.ContentLength = aryBuf.Length;
                using (Stream writer = request.GetRequestStream())
                {
                    writer.Write(aryBuf, 0, aryBuf.Length);
                    writer.Close();
                }
            }
            return request;
        }

        #region Invoke

        public T Invoke<T>(string url, object data, HttpMethod method, RequestFormat format,
            Encoding requestEncoding, Encoding responseEncoding)
        {
            if (format == RequestFormat.Raw && typeof(T) != typeof(string) && typeof(T) != typeof(object))
            {
                throw new ApplicationException("The generic argument 'T' of this method 'Invoke<T>' must only be 'string' or 'object' when the format is 'RequestFormat.Raw'");
            }
            string dataContent = SerializeRequestData(data, format);
            HttpWebRequest request = CreateAndInitHttpRequest(url, method, format, dataContent, requestEncoding);
            string rsp;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream receiveStream = response.GetResponseStream())
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, responseEncoding))
                    {
                        rsp = readStream.ReadToEnd();
                    }
                }
            }
            OnAfterResponse(rsp);
            return (T)DeserializeResponseData(rsp, format, typeof(T));
        }

        public void Invoke(string url, object data, HttpMethod method, RequestFormat format,
            Encoding requestEncoding, Encoding responseEncoding)
        {
            Invoke<object>(url, data, method, format, requestEncoding, responseEncoding);
        }

        public T Invoke<T>(string url, object data = null, HttpMethod method = HttpMethod.Get, RequestFormat format = RequestFormat.Json)
        {
            return Invoke<T>(url, data, method, format, Encoding.UTF8, Encoding.UTF8);
        }

        public void Invoke(string url, object data = null, HttpMethod method = HttpMethod.Get, RequestFormat format = RequestFormat.Json)
        {
            Invoke(url, data, method, format, Encoding.UTF8, Encoding.UTF8);
        }

        #endregion

        #region Get

        public void Get(string url, RequestFormat format = RequestFormat.Json)
        {
            Invoke(url, null, HttpMethod.Get, format);
        }

        public T Get<T>(string url, RequestFormat format = RequestFormat.Json)
        {
            return Invoke<T>(url, null, HttpMethod.Get, format);
        }

        #endregion

        #region Post

        public void Post(string url, object data = null, RequestFormat format = RequestFormat.Json)
        {
            Invoke(url, data, HttpMethod.Post, format);
        }

        public T Post<T>(string url, object data = null, RequestFormat format = RequestFormat.Json)
        {
            return Invoke<T>(url, data, HttpMethod.Post, format);
        }

        #endregion

        #region Put

        public void Put(string url, object data = null, RequestFormat format = RequestFormat.Json)
        {
            Invoke(url, data, HttpMethod.Put, format);
        }

        public T Put<T>(string url, object data = null, RequestFormat format = RequestFormat.Json)
        {
            return Invoke<T>(url, data, HttpMethod.Put, format);
        }

        #endregion

        #region Delete

        public void Delete(string url, object data = null, RequestFormat format = RequestFormat.Json)
        {
            Invoke(url, data, HttpMethod.Delete, format);
        }

        public T Delete<T>(string url, object data = null, RequestFormat format = RequestFormat.Json)
        {
            return Invoke<T>(url, data, HttpMethod.Delete, format);
        }

        #endregion

        #endregion

        #region 异步调用

        private void CreateAndInitHttpRequestAsync(string url, string dataContent, HttpMethod method, RequestFormat format,
            Encoding requestEncoding, Action<Exception> errorHandler, Action<HttpWebRequest> callback)
        {
            HttpWebRequest request = WebRequest.Create(new Uri(CombineUrl(BaseUrl, url))) as HttpWebRequest;
            request.Method = GetHttpMethodStr(method);
            string contentType = GetContentTypeStr(format);
            request.Accept = contentType;
            request.ContentType = contentType;
            HandleHttpHeader(request);
            OnBeforeRequest(request, method);
            if (dataContent != null && dataContent.Trim().Length > 0)
            {
                if (method == HttpMethod.Get)
                {
                    throw new ApplicationException("The http method 'GET' can't support data content in http request stream.");
                }
                byte[] aryBuf = requestEncoding.GetBytes(dataContent);
                request.ContentLength = aryBuf.Length;
                request.BeginGetRequestStream(new AsyncCallback(ar =>
                {
                    bool hasError = false;
                    HttpWebRequest tmp = (HttpWebRequest)ar.AsyncState;
                    try
                    {
                        using (Stream writer = tmp.EndGetRequestStream(ar))
                        {
                            writer.Write(aryBuf, 0, aryBuf.Length);
                            writer.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        hasError = true;
                        if (errorHandler != null)
                        {
                            errorHandler(ex);
                        }
                    }
                    if (hasError == false && callback != null)
                    {
                        callback(tmp);
                    }
                }), request);
            }
            else
            {
                if (callback != null)
                {
                    callback(request);
                }
            }
        }

        #region InvokeAsync

        public void InvokeAsync<T>(string url, object data, HttpMethod method, RequestFormat format,
            Encoding requestEncoding, Encoding responseEncoding, Action<Exception> callbackForFailed = null, Action<T> callbackForSucceed = null)
        {
            if (format == RequestFormat.Raw && typeof(T) != typeof(string) && typeof(T) != typeof(object))
            {
                throw new ApplicationException("The generic argument 'T' of this method 'Invoke<T>' must only be 'string' or 'object' when the format is 'RequestFormat.Raw'");
            }
            string dataContent = SerializeRequestData(data, format);
            CreateAndInitHttpRequestAsync(url, dataContent, method, format,
                requestEncoding, callbackForFailed, request =>
                {
                    request.BeginGetResponse(new AsyncCallback(ar =>
                    {
                        Action<Exception> h = null;
                        Action<T> c = null;
                        string respText = null;
                        bool hasError = false;
                        try
                        {
                            object[] tmp = (object[])ar.AsyncState;
                            h = (Action<Exception>)tmp[2];
                            c = (Action<T>)tmp[1];
                            HttpWebRequest r = (HttpWebRequest)tmp[0];
                            using (HttpWebResponse response = r.EndGetResponse(ar) as HttpWebResponse)
                            {
                                using (Stream receiveStream = response.GetResponseStream())
                                {
                                    using (StreamReader readStream = new StreamReader(receiveStream, responseEncoding))
                                    {
                                        respText = readStream.ReadToEnd();
                                    }
                                }
                            }
                            OnAfterResponse(respText);
                        }
                        catch (Exception ex)
                        {
                            hasError = true;
                            if (h != null)
                            {
                                h(ex);
                            }
                        }
                        if (hasError == false && c != null)
                        {
                            c((T)DeserializeResponseData(respText, format, typeof(T)));
                        }
                    }), new object[] { request, callbackForSucceed, callbackForFailed });
                });
        }

        public void InvokeAsync(string url, object data, HttpMethod method, RequestFormat format,
            Encoding requestEncoding, Encoding responseEncoding, Action<Exception> callbackForFailed = null, Action callbackForSucceed = null)
        {
            InvokeAsync<object>(url, data, method, format, requestEncoding, responseEncoding, callbackForFailed, o => 
            {
                if (callbackForSucceed != null)
                {
                    callbackForSucceed();
                }
            });
        }

        public void InvokeAsync<T>(string url, object data, HttpMethod method = HttpMethod.Get, RequestFormat format = RequestFormat.Json,
            Action<Exception> callbackForFailed = null, Action<T> callbackForSucceed = null)
        {
            InvokeAsync<T>(url, data, method, format, Encoding.UTF8, Encoding.UTF8, callbackForFailed, callbackForSucceed);
        }

        public void InvokeAsync(string url, object data, HttpMethod method = HttpMethod.Get, RequestFormat format = RequestFormat.Json,
            Action<Exception> callbackForFailed = null, Action callbackForSucceed = null)
        {
            InvokeAsync<object>(url, data, method, format, callbackForFailed, o => 
            {
                if (callbackForSucceed != null)
                {
                    callbackForSucceed();
                }
            });
        }

        #endregion

        #region GetAsync

        public void GetAsync<T>(string url, RequestFormat format = RequestFormat.Json,
            Action<Exception> callbackForFailed = null, Action<T> callbackForSucceed = null)
        {
            InvokeAsync<T>(url, null, HttpMethod.Get, format, callbackForFailed, callbackForSucceed);
        }

        public void GetAsync(string url, RequestFormat format = RequestFormat.Json,
            Action<Exception> callbackForFailed = null, Action callbackForSucceed = null)
        {
            InvokeAsync(url, null, HttpMethod.Get, format, callbackForFailed, callbackForSucceed);
        }

        #endregion

        #region PostAsync

        public void PostAsync<T>(string url, object data, RequestFormat format = RequestFormat.Json,
            Action<Exception> callbackForFailed = null, Action<T> callbackForSucceed = null)
        {
            InvokeAsync<T>(url, data, HttpMethod.Post, format, callbackForFailed, callbackForSucceed);
        }

        public void PostAsync(string url, object data, RequestFormat format = RequestFormat.Json,
            Action<Exception> callbackForFailed = null, Action callbackForSucceed = null)
        {
            InvokeAsync(url, data, HttpMethod.Post, format, callbackForFailed, callbackForSucceed);
        }

        #endregion

        #region PutAsync

        public void PutAsync<T>(string url, object data, RequestFormat format = RequestFormat.Json,
            Action<Exception> callbackForFailed = null, Action<T> callbackForSucceed = null)
        {
            InvokeAsync<T>(url, data, HttpMethod.Put, format, callbackForFailed, callbackForSucceed);
        }

        public void PutAsync(string url, object data, RequestFormat format = RequestFormat.Json,
            Action<Exception> callbackForFailed = null, Action callbackForSucceed = null)
        {
            InvokeAsync(url, data, HttpMethod.Put, format, callbackForFailed, callbackForSucceed);
        }

        #endregion

        #region DeleteAsync

        public void DeleteAsync<T>(string url, object data, RequestFormat format = RequestFormat.Json,
            Action<Exception> callbackForFailed = null, Action<T> callbackForSucceed = null)
        {
            InvokeAsync<T>(url, data, HttpMethod.Delete, format, callbackForFailed, callbackForSucceed);
        }

        public void DeleteAsync(string url, object data, RequestFormat format = RequestFormat.Json,
            Action<Exception> callbackForFailed = null, Action callbackForSucceed = null)
        {
            InvokeAsync(url, data, HttpMethod.Delete, format, callbackForFailed, callbackForSucceed);
        }

        #endregion

        #endregion
    }

    public class BeforeRequestEventArgs : EventArgs
    {
        public HttpWebRequest Request { get; private set; }
        public HttpMethod Method { get; private set; }
        public BeforeRequestEventArgs(HttpWebRequest request, HttpMethod method)
        {
            Request = request;
            Method = method;
        }
    }

    public class AfterResponseEventArgs : EventArgs
    {
        public string ResponseContent { get; private set; }
        public AfterResponseEventArgs(string content)
        {
            ResponseContent = content;
        }
    }

    public enum HttpMethod
    {
        Get,
        Post,
        Put,
        Delete
    }

    public enum RequestFormat
    {
        Raw,
        Xml,
        Json,
        Binary
    }
}
