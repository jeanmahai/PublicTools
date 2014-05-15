using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using System.Globalization;

namespace Soho.Utility.WCF
{
    /// <summary>
    /// 处理线程语言环境
    /// </summary>
    public class RestServiceParameterInspector : IParameterInspector
    {
        public object BeforeCall(string operationName, object[] inputs)
        {
            OperationContext operationContext = OperationContext.Current;
            if (operationContext != null)
            {
                if (operationContext.OutgoingMessageProperties.Keys.Contains("_RequestParams_"))
                {
                    operationContext.OutgoingMessageProperties["_RequestParams_"] = inputs;
                }
                else
                {
                    operationContext.OutgoingMessageProperties.Add("_RequestParams_", inputs);
                }
            }

            WebOperationContext webOperationContext = WebOperationContext.Current;
            if (webOperationContext != null)
            {
                string language = GetAcceptLanguage(webOperationContext);

                CultureInfo requestCulture = new CultureInfo(language);

                Thread.CurrentThread.CurrentCulture = requestCulture;
                Thread.CurrentThread.CurrentUICulture = requestCulture;
            }

            return null;
        }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            WebOperationContext context = WebOperationContext.Current;

            string cacheControl = context.IncomingRequest.Headers["Cache-Control"];
            if (cacheControl != null && cacheControl.ToLower() == "no-cache")
            {
                context.OutgoingResponse.Headers["Cache-Control"] = "No-Cache";
            }
        }

        private string GetAcceptLanguage(WebOperationContext webOperationContext)
        {
            string languageValue = webOperationContext.IncomingRequest.Headers["X-Accept-Language-Override"];
            if (languageValue == null || languageValue.Trim() == String.Empty)
            {
                languageValue = webOperationContext.IncomingRequest.Headers["Accept-Language"];
            }

            if (languageValue != null)
            {
                string[] array = languageValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = array.Length; i > 0; i--)
                {
                    string str = array[i - 1].ToLower();
                    switch (str)
                    {
                        case "en":
                        case "en-us":
                            return "en-US";
                        case "zh":
                        case "zh-chs":
                        case "zh-cn":
                            return "zh-CN";
                        case "zh-cht":
                        case "zh-tw":
                            return "zh-TW";
                    }
                }
            }

            return "en-US";
        }
    }
}
