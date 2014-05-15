using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Soho.Utility.DataAccess.SearchEngine
{
    public interface ISearchProvider
    {
        /// <summary>
        /// 根据查询条件检索结果
        /// </summary>
        /// <typeparam name="T">返回结果的数据类型</typeparam>
        /// <param name="condition">查询条件</param>
        /// <returns>查询结果</returns>
        Result Query<Result>(SearchCondition condition);

        /// <summary>
        /// 解析检索提供程序配置
        /// </summary>
        /// <param name="config"></param>
        void ParseConfig(XElement config);
    }
}
