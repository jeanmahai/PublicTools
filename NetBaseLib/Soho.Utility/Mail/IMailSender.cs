using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Soho.Utility
{
    public interface IMailSender
    {
        List<Dictionary<string, string>> AnalyseConfig(XmlNode section, out int recoverSeconds);

        void Send(MailEntity entity, Dictionary<string, string> parameters);
    }
}
