using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Common.Utility.WCF
{
    public class RestServiceOperationSelector : IDispatchOperationSelector
    {
        private const string COSNT_HEADER_METHOD = "X-HTTP-Method-Override";

        private IDispatchOperationSelector m_Operation;

        public RestServiceOperationSelector(IDispatchOperationSelector endpoint)
        {
            m_Operation = endpoint;
        }

        public string SelectOperation(ref Message message)
        {
            HttpRequestMessageProperty httpRequest = message.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;

            if (httpRequest != null)
            {
                string methodName = httpRequest.Headers[COSNT_HEADER_METHOD];
                if (!string.IsNullOrEmpty(methodName) && httpRequest.Method.ToLower() != methodName.ToLower())
                {
                    httpRequest.Method = methodName;
                }
            }

            //return new WebHttpDispatchOperationSelector(m_Endpoint).SelectOperation(ref message);
            if (!message.Properties.Keys.Contains("UriTemplateMatchResults"))
            {
                return m_Operation.SelectOperation(ref message);
            }
            return null;
        }
    }
}
