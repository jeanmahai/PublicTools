using System;
using System.Collections.Generic;
using System.Text;
using System.Messaging;
using System.Diagnostics;
using System.Web;
using System.Net;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Soho.Utility
{
    public static class Logger
    {
        internal static void WriteEventLog(string content, EventLogEntryType type)
        {
            const string event_source = "Soho.Utility.Logger";
            const string event_name = "Soho.Utility.Logger_Exception";
            try
            {
                if (!EventLog.SourceExists(event_source))
                {
                    EventLog.CreateEventSource(event_source, event_name);
                }
                using (EventLog elog = new EventLog())
                {
                    elog.Log = event_name;
                    elog.Source = event_source;
                    elog.WriteEntry(content, type);
                    elog.Close();
                }
            }
            catch { }
        }

        private static string WriteLog(LogEntry log, List<ILogEmitter> logEmitterList)
        {
            if (logEmitterList == null || logEmitterList.Count <= 0)
            {
                return log.LogID.ToString();
            }
            try
            {                
                foreach (var logEmitter in logEmitterList)
                {
                    if (logEmitter == null)
                    {
                        continue;
                    }
                    try
                    {
                        logEmitter.EmitLog(log);
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("Write log failed.\r\n\r\n Error Info: {0}. \r\n\r\n Log Type: {1}. \r\n\r\n Log Info: {2}", ex.ToString(), logEmitter.GetType().AssemblyQualifiedName, log.SerializationWithoutException());
                        Logger.WriteEventLog(message, EventLogEntryType.Error);
                    }
                }
            }
            catch(Exception ex)
            {
                string message = string.Format("Write log failed.\r\n\r\n Error Info: {0}. \r\n\r\n Log Info: {1}", ex.ToString(), log.SerializationWithoutException());
                Logger.WriteEventLog(message, EventLogEntryType.Error);
            }

            return log.LogID.ToString();
        }

        private static string WriteLog(LogEntry log)
        {
            List<ILogEmitter> logEmitterList;
            try
            {
                logEmitterList = EmitterFactory.Create();
            }
            catch(Exception ex)
            {
                string message = string.Format("Failed to create log emitter instance.\r\n\r\n Error Info: {0}. \r\n\r\n Log Info: {1}", ex.ToString(), log.SerializationWithoutException());
                WriteEventLog(message, EventLogEntryType.Error);
                return log.LogID.ToString();
            }
            return WriteLog(log, logEmitterList);
        }

        public static string WriteLog(string content, string category = null, string referenceKey = null, List<KeyValuePair<string, object>> extendedProperties = null)
        {
            LogEntry log = new LogEntry();
            log.ServerTime = DateTime.Now;
            log.LogID = Guid.NewGuid();
            log.Source = GetSource();
            log.RequestUrl = GetRequestUrl();
            log.UserHostName = GetUserHostName();
            log.UserHostAddress = GetUserHostAddress();
            log.ServerIP = GetServerIP();
            try
            {
                log.ServerName = Dns.GetHostName();
            }
            catch { }
            string p_name;
            log.ProcessID = GetProcessInfo(out p_name);
            log.ProcessName = p_name;
            try
            {
                log.ThreadID = Thread.CurrentThread.ManagedThreadId;
            }
            catch { }

            log.Category = category;
            log.Content = content;
            log.ReferenceKey = referenceKey;
            if (extendedProperties != null && extendedProperties.Count > 0)
            {
                foreach (var p in extendedProperties)
                {
                    log.AddExtendedProperty(p.Key, p.Value);
                }
            }

            return WriteLog(log);
        }

        private static string GetSource()
        {
            try
            {
                LogSetting s = LogSection.GetSetting();
                if (s != null)
                {
                    return s.Source;
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string GetRequestUrl()
        {
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    return HttpContext.Current.Request.Url.AbsoluteUri;
                }
                if (OperationContext.Current != null && OperationContext.Current.RequestContext != null
                    && OperationContext.Current.RequestContext.RequestMessage != null
                    && OperationContext.Current.RequestContext.RequestMessage.Headers != null)
                {
                    return OperationContext.Current.RequestContext.RequestMessage.Headers.To.AbsoluteUri;
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string GetUserHostName()
        {
            //try
            //{
            //    if (HttpContext.Current != null && HttpContext.Current.Request != null)
            //    {
            //        return HttpContext.Current.Request.UserHostName;
            //    }
            //    return string.Empty;
            //}
            //catch
            //{
            //    return string.Empty;
            //}
            return string.Empty;
        }

        private static string GetUserHostAddress()
        {
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    return HttpContext.Current.Request.UserHostAddress;
                }
                if (OperationContext.Current != null)
                {
                    RemoteEndpointMessageProperty endpointProperty = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                    if (endpointProperty != null)
                    {
                        return endpointProperty.Address;
                    }
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string s_ServerIP;
        private static string GetServerIP()
        {
            if (string.IsNullOrEmpty(s_ServerIP))
            {
                try
                {
                    IPAddress[] address = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                    if (address != null)
                    {
                        foreach (IPAddress addr in address)
                        {
                            if (addr == null)
                            {
                                continue;
                            }
                            string tmp = addr.ToString().Trim();
                            //过滤IPv6的地址信息
                            if (tmp.Length <= 16 && tmp.Length > 5)
                            {
                                s_ServerIP = tmp;
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    //s_ServerIP = string.Empty;
                }
            }
            if (string.IsNullOrEmpty(s_ServerIP))
            {
                return string.Empty;
            }
            return s_ServerIP;
        }

        private static int GetProcessInfo(out string name)
        {
            try
            {
                Process p = Process.GetCurrentProcess();
                if (p == null)
                {
                    name = null;
                    return -1;
                }
                name = p.ProcessName;
                return p.Id;
            }
            catch
            {
                name = string.Empty;
                return -1;
            }
        }
    }
}