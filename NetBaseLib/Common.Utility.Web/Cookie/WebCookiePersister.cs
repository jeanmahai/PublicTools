using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Common.Utility.Web
{
    internal class WebCookiePersister : ICookiePersist
    {
        #region ICookiePersist Members

        public void Save(string cookieName, string cookieValue, Dictionary<string, string> parameters)
        {
            int expires = 0;
            int.TryParse(parameters["expires"], out expires);
            HttpCookie cookie = new HttpCookie(cookieName, cookieValue);
            if (HttpContext.Current.Request.Url.ToString().Contains(parameters["domain"]) && parameters["domain"] != "localhost")
            {
                cookie.Domain = parameters["domain"];
            }
            cookie.Path = parameters["path"];
            //配置为0，则cookie在会话结束后失效
            if (expires > 0)
                cookie.Expires = DateTime.Now.AddMinutes(expires);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public string Get(string cookieName, Dictionary<string, string> parameters)
        {
            var cookie = HttpContext.Current.Request.Cookies.Get(cookieName);

            string cookieValue = cookie != null ? cookie.Value : string.Empty;
            switch (parameters["encode"])
            {
                case "json":
                    return cookieValue;
                case "normal":
                    return string.Format("\"{0}\"", cookieValue);
                default:
                    return string.Format("\"{0}\"", cookieValue);
            }
        }

        #endregion
    }
}
