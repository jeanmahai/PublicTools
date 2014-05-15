using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Web.Routing;
using System.Configuration;
using System.Net;
using Newtonsoft.Json;

namespace Common.Utility.Web.Framework
{
    /// <summary>
    /// 授权验证
    /// </summary>
    public class AuthAttribute : ActionFilterAttribute
    {
        public bool NeedAuth { get; set; }

        public AuthAttribute()
        {
            this.NeedAuth = true;
        }

        private bool ValidateLogin()
        {
            return AuthMgr.ValidateLogin();
        }

        private bool ValidateAuth(string controller, string action)
        {
            return AuthMgr.ValidateAuth(controller, action);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string clientFlag = filterContext.HttpContext.Request.Headers["x-soho-flag"];
            string controller = filterContext.RouteData.Values["controller"].ToString();
            string action = filterContext.RouteData.Values["action"].ToString();
            if (!string.IsNullOrWhiteSpace(clientFlag))
            {
                //客户端存在自定义标识，则是非Web方式请求，验证非Web方式的请求是否有效
                if (!filterContext.HttpContext.Request.Headers.AllKeys.Contains("x-soho-app-id"))
                {
                    HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.OK;
                    HttpContext.Current.Response.Write(JsonConvert.SerializeObject(new PortalResult
                    {
                        Code = 1000001,
                        Success = false,
                        Message = "未授权的客户端！"
                    }));
                    HttpContext.Current.Response.End();
                    return;
                }
                string ServiceAppId = ConfigurationManager.AppSettings["AppId"];
                var appId = filterContext.HttpContext.Request.Headers.GetValues("x-soho-app-id").ToList().FirstOrDefault();
                if (appId != ServiceAppId)
                {
                    HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.OK;
                    HttpContext.Current.Response.Write(JsonConvert.SerializeObject(new PortalResult
                    {
                        Code = 1000001,
                        Success = false,
                        Message = "未授权的客户端！"
                    }));
                    HttpContext.Current.Response.End();
                    return;
                }
                if (NeedAuth)
                {
                    if (!ValidateLogin())
                    {
                        HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.OK;
                        HttpContext.Current.Response.Write(JsonConvert.SerializeObject(new PortalResult
                        {
                            Code = 1000000,
                            Success = false,
                            Message = "您还没有登录！"
                        }));
                        HttpContext.Current.Response.End();
                        return;
                    }
                    if (!ValidateAuth(controller, action))
                    {
                        HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.OK;
                        HttpContext.Current.Response.Write(JsonConvert.SerializeObject(new PortalResult
                        {
                            Code = 1000001,
                            Success = false,
                            Message = "您没有操作权限！"
                        }));
                        HttpContext.Current.Response.End();
                        return;
                    }
                }
            }
            if (NeedAuth)
            {
                //客户端不存在自定义标识，则是Web方式请求，验证Web方式的请求是否有效
                if (!ValidateLogin())
                {
                    string returnUrl = HttpUtility.UrlEncode(filterContext.RequestContext.HttpContext.Request.Url.AbsoluteUri);
                    string loginUrl = ConfigurationManager.AppSettings["LoginUrl"];
                    if (string.IsNullOrEmpty(loginUrl))
                    {
                        loginUrl = string.Format("~/Login?ReturnUrl={0}", returnUrl);
                    }
                    else
                    {
                        loginUrl = string.Format("{0}?ReturnUrl={1}", loginUrl, returnUrl);
                    }
                    filterContext.Result = new RedirectResult(loginUrl);
                }
                if (!ValidateAuth(controller, action))
                {
                    string returnUrl = HttpUtility.UrlEncode(filterContext.RequestContext.HttpContext.Request.Url.AbsoluteUri);
                    string authErrorUrl = ConfigurationManager.AppSettings["AuthErrorUrl"];
                    if (string.IsNullOrEmpty(authErrorUrl))
                    {
                        authErrorUrl = string.Format("~/AuthError?ReturnUrl={0}", returnUrl);
                    }
                    else
                    {
                        authErrorUrl = string.Format("{0}?ReturnUrl={1}", authErrorUrl, returnUrl);
                    }
                    filterContext.Result = new RedirectResult(authErrorUrl);
                }
            }
        }
    }
}