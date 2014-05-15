using System;
using System.Web;
using System.Web.SessionState;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Soho.Utility
{
    public class HtmlViewHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string content = context.Request.Form["Content"];
            if (!string.IsNullOrWhiteSpace(content))
            {
                content = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(content.Trim()));
            }
            
            // 禁用缓存
            context.Response.Expires = -1;
            context.Response.ExpiresAbsolute = DateTime.Now.AddSeconds(-1);
            context.Response.CacheControl = "no-cache";
            context.Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();

            context.Response.ContentType = "text/html";
            context.Response.Write(content);
            context.Response.Flush();
        }
    }    
}
