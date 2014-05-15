using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Soho.Utility
{
    internal class QueueMailSender : BaseMailSender
    {
        private static Dictionary<string, object[]> s_Cache = new Dictionary<string, object[]>();
        private static object s_SyncObj = new object();

        protected override void ProcessNode(ref List<Dictionary<string, string>> dic, System.Xml.XmlNode node)
        {
            string subject = GetNodeAttribute(node, "subject");
            if (subject == null || subject.Trim().Length <= 0)
            {
                throw new ConfigurationErrorsException("Not config 'subject' for queue mail sender.");
            }
            MappingEvent(subject);
        }

        private void MappingEvent(string subject)
        {
            Type t = Type.GetType("Nesoft.ServiceBus.Consumer.EventMapping, Nesoft.ServiceBus.Consumer", true);
            MethodInfo m = t.GetMethod("AddEvent", BindingFlags.Static | BindingFlags.Public,
                null, new Type[] { typeof(string) }, null).MakeGenericMethod(typeof(MailEntity));
            m.Invoke(null, new object[] { subject });
        }

        private void GetPublishMethodInfo(string queueId, out object publisher, out MethodInfo method)
        {
            object[] x;
            if (s_Cache.TryGetValue(queueId, out x) && x.Length == 2 && x[1] is MethodInfo)
            {
                publisher = x[0];
                method = (MethodInfo)x[1]; 
                return;
            }
            lock (s_SyncObj)
            {
                if (s_Cache.TryGetValue(queueId, out x) && x.Length == 2 && x[1] is MethodInfo)
                {
                    publisher = x[0];
                    method = (MethodInfo)x[1];
                    return;
                }
                Type t = Type.GetType("Nesoft.ServiceBus.Consumer.EventPublisher, Nesoft.ServiceBus.Consumer", true);
                MethodInfo createMethod = t.GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
                publisher = createMethod.Invoke(null, new object[] { queueId });
                method = t.GetMethod("Publish", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeof(MailEntity));
                s_Cache.Add(queueId, new object[] { publisher, method });
            }
        }

        public override void Send(MailEntity entity, Dictionary<string, string> parameters)
        {
            string queueId;
            if (!parameters.TryGetValue("queueId", out queueId) || queueId == null || queueId.Trim().Length <= 0)
            {
                throw new ConfigurationErrorsException("Not config 'queueId' for the queue mail sender.");
            }
            object publisher;
            MethodInfo method;
            GetPublishMethodInfo(queueId, out publisher, out method);
            method.Invoke(publisher, new object[] { entity });
        }

        protected override string NodeName
        {
            get { return "queue"; }
        }
    }
}
