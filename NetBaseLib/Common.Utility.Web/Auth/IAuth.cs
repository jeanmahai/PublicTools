using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utility.Web
{
    public interface IAuth
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <param name="verifyCode"></param>
        /// <returns></returns>
        bool Login(string userName, string pwd, string verifyCode);        

        /// <summary>
        /// 验证客户登录有效性
        /// </summary>       
        /// <returns></returns>
        bool ValidateAuth();
    }
}
