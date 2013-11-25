using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace ExWebServer.Logic.Auth
{
    /// <summary>
    /// 用户授权
    /// </summary>
    public class UserAuthLogic
    {
        /// <summary>
        /// 从请求的Cookie集合中解析用户ID
        /// </summary>
        /// <param name="cookies">请求的Cookie集合</param>
        /// <returns></returns>
        public static int AnalysisUserIDFromRequestCookies(NameValueCollection cookies)
        {
            int userID = 0;

            userID = 10001;

            return userID;
        }
    }
}
