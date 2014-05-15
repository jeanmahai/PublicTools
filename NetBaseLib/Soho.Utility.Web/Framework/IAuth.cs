using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soho.Utility.Web.Framework
{
    /// <summary>
    /// 框架权限验证
    /// </summary>
    public interface IAuth
    {
        /// <summary>
        /// 验证客户是否登录
        /// </summary>       
        /// <returns></returns>
        bool ValidateLogin();

        /// <summary>
        /// 验证客户是否有权限访问
        /// </summary>
        /// <param name="controller">controller</param>
        /// <param name="action">action</param>
        /// <returns></returns>
        bool ValidateAuth(string controller, string action);
    }
}
