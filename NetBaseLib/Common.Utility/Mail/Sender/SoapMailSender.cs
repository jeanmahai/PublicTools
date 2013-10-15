using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Common.Utility
{
    internal class SoapMailSender : BaseMailSender
    {
        public override void Send(MailEntity entity, Dictionary<string, string> parameters)
        {
            if (entity == null)
            {
                return;
            }
            string url;
            if (!parameters.TryGetValue("url", out url) || url == null || url.Trim().Length <= 0)
            {
                throw new ConfigurationErrorsException("Not config 'url' for restful mail sender.");
            }
            string method;
            if (!parameters.TryGetValue("method", out method) || method == null || method.Trim().Length <= 0)
            {
                throw new ConfigurationErrorsException("Not config 'method' for restful mail sender.");
            }

            SoapEntityMapping.Add<MailEntity>();
            SoapEntityMapping.Add<MailTemplateEntity>();
            SoapEntityMapping.Add<VariableTable>();
            SoapEntityMapping.Add<Variable>();
            SoapClient.Invoke(url, method, entity);
        }

        protected override string NodeName
        {
            get { return "soap"; }
        }
    }
}
