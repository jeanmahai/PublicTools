using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Common.Utility
{
    internal class QueueEmitter : ILogEmitter
    {
        private bool m_InitOK = false;
        private string m_QueueId;
        private MethodInfo m_Method;
        private object m_QueueObj;

        public void Init(Dictionary<string, string> param)
        {
            if (!param.ContainsKey("queueId"))
            {
                throw new ConfigurationErrorsException("Not config the 'queueId' of esb queue for message emitter.");
            }
            if (!param.ContainsKey("subject"))
            {
                throw new ConfigurationErrorsException("Not config the 'subject' for message emitter.");
            }
            this.m_QueueId = param["queueId"];
            string subject = param["subject"];
            try
            {
                MappingEvent(subject);
                InitPublishMethodInfo();
                m_InitOK = true;
            }
            catch(Exception ex)
            {
                string msg = "QueueEmitter exception when initializing, detail :\r\n" + ex.ToString();
                Logger.WriteEventLog(msg, System.Diagnostics.EventLogEntryType.Error);
                m_InitOK = false;
            }
        }

        private void MappingEvent(string subject)
        {
            Type t = Type.GetType("Nesoft.ServiceBus.Consumer.EventMapping, Nesoft.ServiceBus.Consumer", true);
            MethodInfo m = t.GetMethod("AddEvent", BindingFlags.Static | BindingFlags.Public, 
                null, new Type[] { typeof(string) }, null).MakeGenericMethod(typeof(LogEntry));
            m.Invoke(null, new object[] { subject });
        }

        private void InitPublishMethodInfo()
        {
            Type t = Type.GetType("Nesoft.ServiceBus.Consumer.EventPublisher, Nesoft.ServiceBus.Consumer", true);
            MethodInfo createMethod = t.GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
            m_QueueObj = createMethod.Invoke(null, new object[] { m_QueueId });
            m_Method = t.GetMethod("Publish", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeof(LogEntry));
        }

        public void EmitLog(LogEntry log)
        {
            if (m_InitOK == false)
            {
                return;
            }
            m_Method.Invoke(m_QueueObj, new object[] { log });
        }
    }
}
