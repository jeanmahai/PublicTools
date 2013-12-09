using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Web.Routing;
using System.Configuration;

namespace Common.Utility.Web
{
    /// <summary>
    /// 身份验证，如果Action不需要身份验证，设置NeedAccess为false即可
    /// </summary>
    public class AuthAttribute : ActionFilterAttribute
    {
        public bool NeedAuth { get; set; }

        public AuthAttribute()
        {
            this.NeedAuth = true;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (NeedAuth)
            {                
                if (!ValidateAuth())
                {
                    //判断客户端的请求类型，Ajax或普通请求
                    if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
                    {
                        string acceptType = filterContext.RequestContext.HttpContext.Request.Headers["Accept"];
                        string message = "身份验证信息已经过期，请重新登录！";

                        if (acceptType.Contains("application/json"))
                        {
                            var data = new
                            {
                                error = true,
                                message = message
                            };
                            filterContext.Result = new JsonResult { Data = data };
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("<?xml version=\"1.0\"?>");
                            sb.AppendLine("<result>");
                            sb.AppendLine("<error>true</error>");
                            sb.AppendLine("<message>身份验证信息已经过期，请重新登录！</message>");
                            sb.AppendLine("</result>");
                            filterContext.Result = new ContentResult
                            {
                                Content = sb.ToString(),
                                ContentEncoding = Encoding.UTF8,
                                ContentType = "application/xml"
                            };
                        }
                    }
                    else
                    {
                        string returnUrl = HttpUtility.UrlEncode(filterContext.RequestContext.HttpContext.Request.Url.AbsoluteUri);
                        string loginUrl = ConfigurationManager.AppSettings["LoginUrl"];
                        if (string.IsNullOrEmpty(loginUrl))
                        {
                            loginUrl = "~/Login?ReturnUrl=" + returnUrl;
                        }
                        else
                        {
                            loginUrl = loginUrl + "?ReturnUrl=" + returnUrl;
                        }
                        filterContext.Result = new RedirectResult(loginUrl);
                    }
                }
                else
                {
                    //验证权限信息，网站暂不需要
                }
            }
        }       

        private bool ValidateAuth()
        {                                  
            return AuthMgr.ValidateAuth();
        }
    }
}