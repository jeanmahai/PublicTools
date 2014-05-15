using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;
using Newtonsoft.Json;
using System.Web.Mvc;

namespace Common.Utility.Web.Framework
{
    /// <summary>
    /// 返回结果解析
    /// </summary>
    public class ResultExecutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            string ServiceAppId = ConfigurationManager.AppSettings["AppId"];
            filterContext.HttpContext.Response.Headers.Add("x-soho-app-id", ServiceAppId);
            var httpResponseMessage = filterContext.HttpContext.Response;
            if (httpResponseMessage != null)
            {
                var content = ((System.Net.Http.ObjectContent)((HttpContent)filterContext.HttpContext.Response.Content));
                var mobileCookie = HttpContext.Current.Response.Headers["x-soho-mobile-cookie"].ToString();
                PortalResult result = content.Value as PortalResult;
                result.Cookie = mobileCookie;
                HttpContext.Current.Response.Write(JsonConvert.SerializeObject(result));
                HttpContext.Current.Response.End();
            }
            else
            {
                base.OnActionExecuted(actionExecutedContext);
            }
        }
    }
}
