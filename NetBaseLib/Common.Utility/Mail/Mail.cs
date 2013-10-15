using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;

namespace Common.Utility
{
    public static class Mail
    {
        private static Dictionary<MailSendType, MailWrapper> s_Cache = new Dictionary<MailSendType, MailWrapper>();
        private static object s_SyncObj = new object();

        public static void Send(MailEntity entity, MailSendType type = MailSendType.Smtp)
        {
            MailWrapper wrapper;
            if (!s_Cache.TryGetValue(type, out wrapper))
            {
                lock (s_SyncObj)
                {
                    if (!s_Cache.TryGetValue(type, out wrapper))
                    {
                        XmlNode section = ConfigurationManager.GetSection("mail") as XmlNode;
                        wrapper = new MailWrapper(section, type);
                        s_Cache.Add(type, wrapper);
                    }
                }
            }
            wrapper.Send(entity);
        }

        public static void Send(string fromAddress, string toAddress, string subject, string body,
            bool isBodyHtml = true, MailPriority priority = MailPriority.Normal, MailSendType type = MailSendType.Smtp)
        {
            Send(fromAddress, fromAddress, toAddress, subject, body, isBodyHtml, priority, type);
        }

        public static void Send(string fromAddress, string fromDisplay, string toAddress, string subject, string body,
            bool isBodyHtml = true, MailPriority priority = MailPriority.Normal, MailSendType type = MailSendType.Smtp)
        {
            Send(fromAddress, fromDisplay, toAddress, subject, body, Encoding.UTF8, isBodyHtml, priority, type);
        }

        public static void Send(string fromAddress, string fromDisplay, string toAddress, string subject, string body, Encoding charset,
            bool isBodyHtml = true, MailPriority priority = MailPriority.Normal, MailSendType type = MailSendType.Smtp)
        {
            Send(fromAddress, fromDisplay, new List<string> { toAddress }, null, null, subject, body, charset, isBodyHtml, priority, type);
        }

        public static void Send(string fromAddress, string fromDisplay, List<string> toAddressList, List<string> ccAddressList,
            List<string> bccAddressList, string subject, string body, Encoding charset, bool isBodyHtml = true,
            MailPriority priority = MailPriority.Normal, MailSendType type = MailSendType.Smtp)
        {
            MailEntity entity = new MailEntity
            {
                FromAddress = fromAddress,
                FromDisplay = fromDisplay,
                ToAddressList = toAddressList,
                CcAddressList = ccAddressList,
                BccAddressList = bccAddressList,
                Subject =subject,
                Body = body,
                Charset = charset,
                IsBodyHtml = isBodyHtml,
                Priority = priority
            };
            Send(entity, type);
        }
    }
}
