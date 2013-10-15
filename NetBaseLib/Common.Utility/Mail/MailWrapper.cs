using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Common.Utility
{
    internal class MailWrapper
    {
        private IMailSender m_Mail;
        private List<Dictionary<string, string>> m_Config;
        private DateTime[] m_ErrorTimeList;
        private string[] m_ErrorInfoList;
        private int m_StartIndex;
        private int m_RecoverSeconds;
        private MailSendType m_MailSendType;

        public MailWrapper(XmlNode section, MailSendType type)
        {
            m_MailSendType = type;
            m_Mail = Create(type);
            int tmp;
            m_Config = m_Mail.AnalyseConfig(section, out tmp);
            m_RecoverSeconds = tmp;
            m_ErrorTimeList = new DateTime[m_Config.Count];
            m_ErrorInfoList = new string[m_Config.Count];
            for (int i = 0; i < m_Config.Count; i++)
            {
                m_ErrorTimeList[i] = DateTime.MinValue;
            }
            m_StartIndex = 0;
        }

        private IMailSender Create(MailSendType type)
        {
            switch (type)
            {
                case MailSendType.Queue:
                    return new QueueMailSender();
                case MailSendType.RestfulService:
                    return new RestfulMailSender();
                case MailSendType.SoapService:
                    return new SoapMailSender();
                default: // MailSendType.Smtp
                    return new SmtpMailSender();
            }
        }

        public void Send(MailEntity entity)
        {
            if (m_Config == null || m_Config.Count <= 0)
            {
                throw new ApplicationException("not config any node to send mail for " + m_MailSendType.ToString());
            }
            int count = 0;
            for (int i = m_StartIndex; i < m_Config.Count; i++)
            {
                count++;
                if (m_ErrorTimeList[i].AddSeconds(m_RecoverSeconds) > DateTime.Now)
                {
                    continue;
                }
                try
                {
                    m_Mail.Send(entity, m_Config[i]);
                    m_StartIndex = (i + 1 >= m_Config.Count ? 0 : i + 1);
                    return;
                }
                catch (Exception ex)
                {
                    m_ErrorTimeList[i] = DateTime.Now;
                    m_ErrorInfoList[i] = ex.Message;
                    if (count >= m_Config.Count)
                    {
                        throw;
                    }
                }
            }
            int c = m_Config.Count - count;
            for (int i = 0; i < c; i++)
            {
                count++;
                if (m_ErrorTimeList[i].AddSeconds(m_RecoverSeconds) > DateTime.Now)
                {
                    continue;
                }
                try
                {
                    m_Mail.Send(entity, m_Config[i]);
                    m_StartIndex = (i + 1 >= m_Config.Count ? 0 : i + 1);
                    return;
                }
                catch (Exception ex)
                {
                    m_ErrorTimeList[i] = DateTime.Now;
                    m_ErrorInfoList[i] = ex.Message;
                    if (count >= m_Config.Count)
                    {
                        throw;
                    }
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("[" + DateTime.Now + "]All nodes to send mail by " + m_MailSendType.ToString() + " are failed. Failed detail info:");
            for (int i = 0; i < m_Config.Count; i++)
            {
                sb.Append("\r\n(" + i + ") - [" + m_ErrorTimeList[i] + "] : " + m_ErrorInfoList[i]);
            }
            throw new ApplicationException(sb.ToString());
        }
    }
}
