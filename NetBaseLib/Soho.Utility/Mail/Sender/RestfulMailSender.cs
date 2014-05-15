using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Soho.Utility
{
    internal class RestfulMailSender : BaseMailSender
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
            string methodStr;
            HttpMethod method;
            if (!parameters.TryGetValue("method", out methodStr) || methodStr == null || methodStr.Trim().Length <= 0
                || Enum.TryParse<HttpMethod>(methodStr, out method) == false
                || Enum.IsDefined(typeof(HttpMethod), method) == false)
            {
                method = HttpMethod.Post;
            }
            string formatStr;
            RequestFormat format;
            if (!parameters.TryGetValue("format", out formatStr) || formatStr == null || formatStr.Trim().Length <= 0
                || Enum.TryParse<RequestFormat>(formatStr, out format) == false
                || Enum.IsDefined(typeof(RequestFormat), format) == false)
            {
                format = RequestFormat.Json;
            }

            RestClient client = new RestClient();
            client.Invoke(url, entity, method, format);
        }

        protected override string NodeName
        {
            get { return "restful"; }
        }
    }
}
