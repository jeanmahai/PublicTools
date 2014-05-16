using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;
using Newtonsoft.Json;
using System.Web.Mvc;
using System.Net;

namespace Soho.Utility.Web.Framework
{
    /// <summary>
    /// 返回结果解析
    /// </summary>
    public class ResultExecutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            bool clientFlag = filterContext.HttpContext.Request.Headers.AllKeys.Contains("x-soho-app-id");
            if (clientFlag)
            {
                string ServiceAppId = ConfigurationManager.AppSettings["AppId"];
                ServiceAppId = string.IsNullOrWhiteSpace(ServiceAppId) ? "" : ServiceAppId;
                filterContext.HttpContext.Response.AddHeader("x-soho-app-id", ServiceAppId);

                bool bIsMustBusinessRoute = ValidateIsMustBusinessRoute(filterContext.RouteData.Values["controller"].ToString(),
                    filterContext.RouteData.Values["action"].ToString());
                if (bIsMustBusinessRoute)
                {
                    HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.OK;
                    HttpContext.Current.Response.Write(SerializationUtility.JsonSerialize2(new PortalResult
                    {
                        Code = 1000001,
                        Success = false,
                        Message = "您没有操作权限！"
                    }));
                    HttpContext.Current.Response.End();
                    return;
                }
                else
                {
                    //客户端存在自定义标识，则是非Web方式请求，直接返回Json数据
                    ViewResult viewResult = filterContext.Result as ViewResult;
                    var modelData = viewResult.Model as PortalResult;
                    MobilePortalResult responseData = new MobilePortalResult(modelData);

                    var mobileCookie = HttpContext.Current.Response.Headers["x-soho-mobile-cookie"] == null ? "" : HttpContext.Current.Response.Headers["x-soho-mobile-cookie"].ToString();
                    responseData.Cookie = mobileCookie;

                    HttpContext.Current.Response.Write(SerializationUtility.JsonSerialize2(responseData));
                    HttpContext.Current.Response.End();
                }
            }
        }
        /// <summary>
        /// 验证是否只能是MVC的情况
        /// </summary>
        /// <param name="controller">controller</param>
        /// <param name="action">action</param>
        /// <returns></returns>
        private bool ValidateIsMustBusinessRoute(string controller, string action)
        {
            string route = string.Format("{0}|{1}", controller, action);
            var configList = BusinessRouteConfiguration.BusinessRouteList;
            //无业务路由配置，则不为只能是MVC的情况
            if (configList == null)
                return false;
            //当前路由不在业务路由配置中，则不为只能是MVC的情况
            var currItem = configList.Find(m => m.Route == route);
            if (currItem == null)
                return false;
            //当前路由在业务路由配置中，但不为只能是MVC的情况
            if (!currItem.IsMust)
                return false;

            return true;
        }
    }
}
