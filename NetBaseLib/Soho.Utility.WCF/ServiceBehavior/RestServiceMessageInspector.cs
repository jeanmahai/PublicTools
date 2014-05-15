using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.ServiceModel.Dispatcher;
using System.Security.Cryptography;
using System.Web;

namespace Soho.Utility.WCF
{
    /// <summary>
    /// 使用url参数上的数据来修改Get方式时的http头，同时如果有传当前用户信息，还会验证签名，并作授权验证
    /// </summary>
    public class RestServiceMessageInspector : IDispatchMessageInspector
    {
        private const string ACCEPT_TYPE = "Portal_Accept";
        private const string LANGUAGE_CODE = "Portal_Language";
        private const string X_ACCEPT_LANGUAGE_OVERRIDE = "X-Accept-Language-Override";

        private const string Portal_UserAcct = "Portal_UserAcct";
        private const string Portal_UserSysNo = "Portal_UserSysNo";
        private const string Portal_UserDisplayName = "Portal_UserDisplayName";
        private const string Portal_TimeZone = "Portal_TimeZone";
        private const string Portal_Sign = "Portal_Sign";

        private const string X_User_Acct = "X-User-Acct";
        private const string X_User_SysNo = "X-User-SysNo";
        private const string X_User_Display_Name = "X-User-Display-Name";
        private const string X_Portal_TimeZone = "X-Portal-TimeZone";
        private const string X_Portal_Sign = "X-Portal-Sign";

        private const string SIGN_KEY = "ds@H73d12Jds%dN&2#";
        private const string PREFIX = "nesoft_wcf_service_";

        internal static int UserSysNo
        {
            get;
            private set;
        }

        internal static int UserTimeZoneUtcOffset
        {
            get;
            private set;
        }

        #region IDispatchMessageInspector Members

        public object AfterReceiveRequest(ref Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
        {
            if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest != null
                && WebOperationContext.Current.IncomingRequest.UriTemplateMatch != null
                && WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters != null)
            {
                var collection = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
                foreach (var key in collection.AllKeys)
                {
                    var header = WebOperationContext.Current.IncomingRequest.Headers;
                    if (header.AllKeys.Contains(PREFIX + key))
                    {
                        header[PREFIX + key] = collection[key];
                    }
                    else
                    {
                        header.Add(PREFIX + key, collection[key]);
                    }
                }
            }
            string sign;
            string userSysNo;
            string timeZone;
            string userAcct;
            string displayName;
            if (WebOperationContext.Current.IncomingRequest.Method.ToLower() == "get")
            {
                var type = GetQueryStringValue(WebOperationContext.Current.IncomingRequest, ACCEPT_TYPE);
                if (type != null && type.Length != 0)
                {
                    WebOperationContext.Current.IncomingRequest.Headers["Accept"] = type;
                }
                var languageCode = GetQueryStringValue(WebOperationContext.Current.IncomingRequest, LANGUAGE_CODE);
                if (languageCode != null && languageCode.Length != 0)
                {
                    WebOperationContext.Current.IncomingRequest.Headers[X_ACCEPT_LANGUAGE_OVERRIDE] = languageCode;
                }

                sign = GetQueryStringValue(WebOperationContext.Current.IncomingRequest, Portal_Sign);

                userSysNo = GetQueryStringValue(WebOperationContext.Current.IncomingRequest, Portal_UserSysNo);
                if (userSysNo != null && userSysNo.Trim().Length > 0)
                {
                    WebOperationContext.Current.IncomingRequest.Headers.Add(X_User_SysNo, HttpUtility.UrlEncode(userSysNo.Trim()));
                }

                timeZone = GetQueryStringValue(WebOperationContext.Current.IncomingRequest, Portal_TimeZone);
                if (timeZone != null && timeZone.Trim().Length > 0)
                {
                    WebOperationContext.Current.IncomingRequest.Headers.Add(X_Portal_TimeZone, timeZone.Trim());
                }

                userAcct = GetQueryStringValue(WebOperationContext.Current.IncomingRequest, Portal_UserAcct);
                if (userAcct != null && userAcct.Trim().Length > 0)
                {
                    WebOperationContext.Current.IncomingRequest.Headers.Add(X_User_Acct, HttpUtility.UrlEncode(userAcct.Trim()));
                }

                displayName = GetQueryStringValue(WebOperationContext.Current.IncomingRequest, Portal_UserDisplayName);
                if (displayName != null && displayName.Trim().Length > 0)
                {
                    WebOperationContext.Current.IncomingRequest.Headers.Add(X_User_Display_Name, HttpUtility.UrlEncode(displayName.Trim()));
                }
            }
            else
            {
                var requestContentType = WebOperationContext.Current.IncomingRequest.Headers["Content-Type"];
                var agent = WebOperationContext.Current.IncomingRequest.Headers["User-Agent"];
                bool isFirefox = agent != null && agent.ToUpper().Contains("FIREFOX");
                if (isFirefox && !string.IsNullOrWhiteSpace(requestContentType)) // 针对.net 4.5的iis对firefox浏览器的http请求，无法读取到正常的accpt问题
                {
                    WebOperationContext.Current.IncomingRequest.Headers["Accept"] = requestContentType;
                }

                sign = WebOperationContext.Current.IncomingRequest.Headers[X_Portal_Sign];
                userSysNo =WebOperationContext.Current.IncomingRequest.Headers[X_User_SysNo];
                if (!string.IsNullOrWhiteSpace(userSysNo))
                {
                    userSysNo = HttpUtility.UrlDecode(userSysNo);
                }
                timeZone = WebOperationContext.Current.IncomingRequest.Headers[X_Portal_TimeZone];
                userAcct = WebOperationContext.Current.IncomingRequest.Headers[X_User_Acct];
                if (!string.IsNullOrWhiteSpace(userAcct))
                {
                    userAcct = HttpUtility.UrlDecode(userAcct);
                }
                displayName = WebOperationContext.Current.IncomingRequest.Headers[X_User_Display_Name];
                if (!string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = HttpUtility.UrlDecode(displayName);
                }
            }
            bool needCheck = true;
            if (needCheck && (userSysNo != null || timeZone != null || userAcct != null))
            {
                byte[] array = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(userSysNo + userAcct + timeZone + SIGN_KEY));
                string newSign = Convert.ToBase64String(array);
                if (sign.Trim() != newSign)
                {
                    throw new ApplicationException("The sign of request is error.");
                }
            }
            if (WebOperationContext.Current.IncomingRequest.UriTemplateMatch != null)
            {
                string url = request.Headers.To.AbsolutePath;
                string urlTemplate = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.Template.ToString();
                string methodName = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.Data.ToString();
                IAuthorize authorizer = ServiceConfig.GetAuthorizer();
                if (authorizer != null && !authorizer.Check(userSysNo, methodName, urlTemplate, url))
                {
                    throw new ApplicationException("Not authorized to access '" + url + "' for user '" + userSysNo + "'.");
                }
            }
            return null;
        }

        private string GetQueryStringValue(IncomingWebRequestContext context, string key)
        {
            try
            {
                var queryStrings = context.UriTemplateMatch.QueryParameters;
                return queryStrings[key];
            }
            catch
            {
                return null;
            }
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            HttpResponseMessageProperty response = reply.Properties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
            response.Headers["Cache-Control"] = "No-Cache";
        }

        #endregion
    }
}
