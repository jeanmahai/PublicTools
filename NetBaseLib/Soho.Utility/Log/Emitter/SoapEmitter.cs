using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Soho.Utility
{
    internal class SoapEmitter : ILogEmitter
    {
        private string m_Url;
        private string m_MethodName;

        public void Init(Dictionary<string, string> param)
        {
            string url;
            if (param.TryGetValue("url", out url) == false || url == null || url.Trim().Length <= 0)
            {
                throw new ConfigurationErrorsException("Not config 'url' for soap emitter.");
            }
            m_Url = url;
            string method;
            if (param.TryGetValue("method", out method) == false || method == null || method.Trim().Length <= 0)
            {
                throw new ConfigurationErrorsException("Not config 'method' for soap emitter.");
            }
            m_MethodName = method;
        }

        public void EmitLog(LogEntry log)
        {
            SoapEntityMapping.Add<LogEntry>();
            SoapEntityMapping.Add<ExtendedPropertyData>();
            SoapClient.InvokeAsync(m_Url, m_MethodName, null, ex =>
            {
                string message = string.Format("Write log failed.\r\n\r\n Error Info: {0}. \r\n\r\n Log Type: {1}. \r\n\r\n Log Info: {2}", ex.ToString(), this.GetType().AssemblyQualifiedName, log.SerializationWithoutException());
                Logger.WriteEventLog(message, System.Diagnostics.EventLogEntryType.Error);
            }, log);
        }
    }
}
