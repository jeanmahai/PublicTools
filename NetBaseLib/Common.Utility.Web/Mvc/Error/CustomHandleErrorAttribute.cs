using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Common.Utility.Web.Mvc
{
    public abstract class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        protected abstract bool HandleException(Exception ex);
        protected abstract ActionResult BuildAjaxJsonActionResult(Exception ex, bool isLocalRequest);
        protected abstract ActionResult BuildAjaxHtmlActionResult(Exception ex, bool isLocalRequest);
        protected abstract ActionResult BuildAjaxXmlActionResult(Exception ex, bool isLocalRequest);
        protected abstract ActionResult BuildWebPageActionResult(Exception ex, bool isLocalRequest, ExceptionContext filterContext);

        protected virtual ActionResult BuildResult(Exception ex, ExceptionContext filterContext)
        {
            HttpRequestBase request = filterContext.RequestContext.HttpContext.Request;
            ActionResult result;
            if (request.IsAjaxRequest())
            {
                string acceptType = request.Headers["Accept"].ToLower();
                if (acceptType.Contains("application/json"))
                {
                    result = BuildAjaxJsonActionResult(ex, request.IsLocal);
                }
                else if (acceptType.Contains("text/html"))
                {
                    result = BuildAjaxHtmlActionResult(ex, request.IsLocal);
                }
                else
                {
                    result = BuildAjaxXmlActionResult(ex, request.IsLocal);
                }
            }
            else
            {
                result = BuildWebPageActionResult(ex, request.IsLocal, filterContext);
            }
            return result;
        }

        protected virtual bool TrySkipIisCustomErrors
        {
            get { return true; }
        }

        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }

            #region
            //非BizException才调用此方法，此方法内部都是同步调用Service写log，同步写入文件，速度较慢
            //ExceptionHelper.HandleException(filterContext.Exception);

            //ActionResult result;
            //if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
            //{
            //    string acceptType = filterContext.RequestContext.HttpContext.Request.Headers["Accept"];
            //    string message;
            //    if (!filterContext.RequestContext.HttpContext.Request.IsLocal)
            //    {
            //        message = "应用程序发生异常，请联系管理员。";
            //    }
            //    else
            //    {
            //        message = filterContext.Exception.Message;
            //    }
            //    if (acceptType.Contains("application/json"))
            //    {
            //        var data = new
            //        {
            //            error = true,
            //            message = message
            //        };
            //        result = new JsonResult { Data = data };
            //    }
            //    else if (acceptType.Contains("text/html"))
            //    {
            //        StringBuilder sb = new StringBuilder();
            //        sb.Append("<div id=\"service_Error_Message_Panel\">");
            //        sb.AppendFormat("<input id=\"errorMessage\" type=\"hidden\" value=\"{0}\" />", filterContext.HttpContext.Server.HtmlEncode(message));
            //        sb.Append("</div>");
            //        result = new ContentResult
            //        {
            //            Content = sb.ToString(),
            //            ContentEncoding = Encoding.UTF8,
            //            ContentType = "application/xml"
            //        };
            //    }
            //    else
            //    {
            //        StringBuilder sb = new StringBuilder();
            //        sb.AppendLine("<?xml version=\"1.0\"?>");
            //        sb.AppendLine("<result>");
            //        sb.AppendLine("<error>true</error>");
            //        sb.AppendLine("<message>" + message.Replace("<", "&lt;").Replace(">", "&gt;") + "</message>");
            //        sb.AppendLine("</result>");
            //        result = new ContentResult
            //        {
            //            Content = sb.ToString(),
            //            ContentEncoding = Encoding.UTF8,
            //            ContentType = "application/xml"
            //        };
            //    }
            //}
            ////非Ajax错误则在Error页面展示异常信息
            //else
            //{
            //    string controller = filterContext.RouteData.Values["controller"].ToString();
            //    string action = filterContext.RouteData.Values["action"].ToString();
            //    HandleErrorInfo model = new HandleErrorInfo(filterContext.Exception, controller, action);
            //    result = new ViewResult
            //    {
            //        ViewName = this.View,
            //        MasterName = this.Master,
            //        ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
            //        TempData = filterContext.Controller.TempData
            //    };
            //}
            #endregion

            filterContext.Result = BuildResult(filterContext.Exception, filterContext);
            filterContext.HttpContext.Response.Clear();
            //不能把状态码设置为500，否则客户端不能显示真实的异常信息
            //filterContext.HttpContext.Response.StatusCode = 500;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = TrySkipIisCustomErrors;
            filterContext.ExceptionHandled = HandleException(filterContext.Exception);
        }
    }
}
