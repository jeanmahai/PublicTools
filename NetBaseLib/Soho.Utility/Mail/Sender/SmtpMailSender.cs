using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Soho.Utility
{
    internal class SmtpMailSender : BaseMailSender
    {
        public override void Send(MailEntity entity, Dictionary<string, string> parameters)
        {
            if (entity == null)
            {
                return;
            }
            string address;
            if (!parameters.TryGetValue("address", out address))
            {
                throw new ConfigurationErrorsException("Not config 'address' for smtp mail sender.");
            }
            int port;
            string portStr;
            if (!parameters.TryGetValue("port", out portStr) || !int.TryParse(portStr, out port) || port <= 0)
            {
                port = 25;
            }
            string username;
            if (!parameters.TryGetValue("username", out username) || username == null || username.Trim().Length <= 0)
            {
                username = string.Empty;
            }
            string password;
            if (!parameters.TryGetValue("password", out password) || password == null || password.Trim().Length <= 0)
            {
                password = string.Empty;
            }
            bool enableSsl;
            string enableSslStr;
            if (!parameters.TryGetValue("enableSsl", out enableSslStr) || !bool.TryParse(enableSslStr, out enableSsl))
            {
                enableSsl = false;
            }
            SendMail(entity.FromAddress, entity.FromDisplay,
                entity.ToAddressList == null ? null : entity.ToAddressList.ToArray(),
                entity.CcAddressList == null ? null : entity.CcAddressList.ToArray(),
                entity.BccAddressList == null ? null : entity.BccAddressList.ToArray(),
                entity.Subject, entity.Body, null, entity.Charset, entity.IsBodyHtml, entity.Priority, address, port, username, password, enableSsl);
        }

        protected override string NodeName
        {
            get { return "smtp"; }
        }

        private void SendMail(string fromAddress, string fromDisplay, string[] toAddressList,
            string[] ccAddressList, string[] bccAddressList, string subject, string body,
            string[] attachmentFilePathList, Encoding charset, bool isBodyHtml, MailPriority priority,
            string address, int port, string username, string password, bool enableSsl)
        {
            System.Net.Mail.MailMessage emailInfo = new System.Net.Mail.MailMessage();
            emailInfo.From = new MailAddress(fromAddress, fromDisplay, charset);
            if (toAddressList != null && toAddressList.Length > 0)
            {
                foreach (string mailto in toAddressList)
                {
                    if (!string.IsNullOrWhiteSpace(mailto))
                    {
                        emailInfo.To.Add(mailto);
                    }
                }
            }

            if (ccAddressList != null && ccAddressList.Length > 0)
            {
                foreach (string cc in ccAddressList)
                {
                    if (!string.IsNullOrWhiteSpace(cc))
                    {
                        emailInfo.CC.Add(cc);
                    }
                }
            }

            if (bccAddressList != null && bccAddressList.Length > 0)
            {
                foreach (string bcc in bccAddressList)
                {
                    if (!string.IsNullOrWhiteSpace(bcc))
                    {
                        emailInfo.Bcc.Add(bcc);
                    }
                }
            }

            if (attachmentFilePathList != null && attachmentFilePathList.Length > 0)
            {
                foreach (string file in attachmentFilePathList)
                {
                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        emailInfo.Attachments.Add(new Attachment(file));
                    }
                }
            }

            emailInfo.Subject = subject;
            emailInfo.Body = body;
            switch (priority)
            {
                case MailPriority.High:
                    emailInfo.Priority = System.Net.Mail.MailPriority.High;
                    break;
                case MailPriority.Low:
                    emailInfo.Priority = System.Net.Mail.MailPriority.Low;
                    break;
                default:
                    emailInfo.Priority = System.Net.Mail.MailPriority.Normal;
                    break;
            }
            emailInfo.IsBodyHtml = isBodyHtml;
            emailInfo.BodyEncoding = charset;
            emailInfo.SubjectEncoding = charset;

            SmtpClient smtpMail = new SmtpClient(address, port);
            smtpMail.Credentials = new NetworkCredential(username, password);
            smtpMail.EnableSsl = enableSsl;
            smtpMail.Send(emailInfo);
        }
    }
}
