using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Common.Utility
{
    internal class EventLogEmitter : ILogEmitter
    {
        private string m_LogName = null;

        public void Init(Dictionary<string, string> param)
        {
            string name;
            if (param.TryGetValue("name", out name) && name != null && name.Trim().Length > 0)
            {
                m_LogName = name;
            }
            if (m_LogName == null || m_LogName.Trim().Length <= 0)
            {
                m_LogName = Assembly.GetEntryAssembly().FullName;
            }
        }

        public void EmitLog(LogEntry log)
        {
            WriteEventLog(m_LogName, log.SerializationWithoutException(), EventLogEntryType.Error);
        }

        private static void WriteEventLog(string event_source, string content, EventLogEntryType type)
        {
            try
            {
                if (!EventLog.SourceExists(event_source))
                {
                    EventLog.CreateEventSource(event_source, event_source);
                }
                using (EventLog elog = new EventLog())
                {
                    elog.Log = event_source;
                    elog.Source = event_source;
                    elog.WriteEntry(content, type);
                    elog.Close();
                }
            }
            catch { }
        }
    }
}
