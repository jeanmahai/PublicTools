using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.ServiceModel;
using System.Web;
using System.Net;
using System.Configuration;

namespace Soho.Utility.Web.Framework
{
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception != null)
            {
                bool clientFlag = filterContext.HttpContext.Request.Headers.AllKeys.Contains("x-soho-app-id");
                if (clientFlag)
                {
                    HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.OK;
                    HttpContext.Current.Response.Write(SerializationUtility.JsonSerialize2(new PortalResult
                    {
                        Code = GetExceptionCode(filterContext.Exception, filterContext.HttpContext.Request.IsLocal),
                        Success = false,
                        Message = GetExceptionInfo(filterContext.Exception, filterContext.HttpContext.Request.IsLocal)
                    }));
                    HttpContext.Current.Response.End();
                    return;
                }
                else
                {
                    Exception exception = new Exception(GetExceptionInfo(filterContext.Exception, filterContext.HttpContext.Request.IsLocal));

                    string controller = filterContext.RouteData.Values["controller"].ToString();
                    string action = filterContext.RouteData.Values["action"].ToString();
                    HandleErrorInfo model = new HandleErrorInfo(exception, controller, action);
                    filterContext.Controller.TempData["ExceptionMessage"] = GetExceptionInfo(filterContext.Exception, filterContext.HttpContext.Request.IsLocal);

                    filterContext.Result = new ViewResult
                    {
                        ViewName = this.View,
                        MasterName = this.Master,
                        ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                        TempData = filterContext.Controller.TempData
                    };
                    filterContext.HttpContext.Response.Clear();
                    filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                    filterContext.ExceptionHandled = HandleException(filterContext.Exception);
                }
            }
        }

        /// <summary>
        /// 非业务异常处理
        /// </summary>
        /// <param name="ex">异常</param>
        /// <returns></returns>
        private bool HandleException(Exception ex)
        {
            if (!IsBizException(ex))
            {
                if (!(ex is FaultException))
                {
                    Soho.Utility.Logger.WriteLog(ex.ToString(), "SohoWeb_Exception");
                }
            }
            return true;
        }
        /// <summary>
        /// 是否是业务异常
        /// </summary>
        /// <param name="ex">异常</param>
        /// <returns></returns>
        private bool IsBizException(Exception ex)
        {
            if (ex is BusinessException || ((ex is FaultException) && ((FaultException)ex).Code.Name == "1"))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取异常信息
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="isLocalRequest">是否是本地请求</param>
        /// <returns></returns>
        private string GetExceptionInfo(Exception ex, bool isLocalRequest)
        {
            if (IsBizException(ex))
            {
                return ex.Message;
            }
            if (IsDebug || isLocalRequest)
            {
                if (ex is FaultException)
                {
                    return ((FaultException)ex).Reason.ToString();
                }
                else
                {
                    return ex.ToString();
                }
            }
            else
            {
                return "系统发生异常，请稍后再试！";
            }
        }
        /// <summary>
        /// 获取异常代码
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="isLocalRequest">是否是本地请求</param>
        /// <returns></returns>
        private int GetExceptionCode(Exception ex, bool isLocalRequest)
        {
            if (IsBizException(ex))
            {
                return 2000000;
            }
            if (IsDebug || isLocalRequest)
            {
                return 1000002;
            }
            else
            {
                return 1000002;
            }
        }
        /// <summary>
        /// 是否是Debug模式
        /// </summary>
        private bool IsDebug
        {
            get
            {
                string IsDebug = ConfigurationManager.AppSettings["IsDebug"];
                IsDebug = string.IsNullOrWhiteSpace(IsDebug) ? "" : IsDebug;
                return IsDebug.Equals("True") ? true : false;
            }
        }
    }
}
