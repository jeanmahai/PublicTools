using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Soho.Utility
{
    internal class RestfulEmitter : ILogEmitter
    {
        private string m_Url;
        private HttpMethod m_Method;
        private RequestFormat m_Format;

        public void Init(Dictionary<string, string> param)
        {
            string url;
            if (param.TryGetValue("url", out url) == false || url == null || url.Trim().Length <= 0)
            {
                throw new ConfigurationErrorsException("Not config 'url' for restful emitter.");
            }
            m_Url = url;

            string method;
            if (param.TryGetValue("method", out method) == false || method == null || method.Trim().Length <= 0)
            {
                m_Method = HttpMethod.Post;
            }
            else
            {
                HttpMethod tmp_method;
                if (Enum.TryParse<HttpMethod>(method, out tmp_method) == false
                    || Enum.IsDefined(typeof(HttpMethod), tmp_method) == false)
                {
                    throw new ConfigurationErrorsException("Invalid value of 'method' in config for soap emitter.");
                }
                m_Method = tmp_method;
            }

            string format;
            if (param.TryGetValue("format", out format) == false || format == null || format.Trim().Length <= 0)
            {
                m_Format = RequestFormat.Json;
            }
            else
            {
                RequestFormat tmp_format;
                if (Enum.TryParse<RequestFormat>(format, out tmp_format) == false
                    || Enum.IsDefined(typeof(RequestFormat), tmp_format) == false)
                {
                    throw new ConfigurationErrorsException("Invalid value of 'format' in config for soap emitter.");
                }
                m_Format = tmp_format;
            }
        }

        public void EmitLog(LogEntry log)
        {
            RestClient client = new RestClient();
            client.InvokeAsync(m_Url, log, m_Method, m_Format, ex =>
            {
                string message = string.Format("Write log failed.\r\n\r\n Error Info: {0}. \r\n\r\n Log Type: {1}. \r\n\r\n Log Info: {2}", ex.ToString(), this.GetType().AssemblyQualifiedName, log.SerializationWithoutException());
                Logger.WriteEventLog(message, System.Diagnostics.EventLogEntryType.Error);
            });
        }
    }
}
