using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Soho.Utility.DataAccess.SearchEngine;
using System.Configuration;
using Soho.Utility.DataAccess.SearchEngine.Solr;

namespace Soho.Utility.DataAccess
{
    /// <summary>
    /// 搜索引擎检索管理工具，搜索引擎对外暴露的接口
    /// </summary>
    public  class SearchEngineManager
    {
        /// <summary>
        /// 根据查询条件检索结果
        /// </summary>
        /// <typeparam name="T">返回结果的数据类型</typeparam>
        /// <param name="condition">查询条件</param>
        /// <returns>查询结果</returns>
        public static T Query<T>(SearchCondition condition)
        {
            ISearchProvider provider = null;
            if (TryGetSearchProvider(typeof(T), out provider) && provider != null)
            {
                return provider.Query<T>(condition);
            }
            return default(T);
        }

        /// <summary>
        /// 取得检索提供程序
        /// </summary>
        /// <param name="tp">返回结果的数据类型</param>
        /// <param name="provider">检索提供程序 </param>
        /// <returns>是否争取取得检索提供程序 true：是；false：否</returns>
        private static bool TryGetSearchProvider(Type tp, out ISearchProvider provider)
        {
            return SearchEngineConfigManager.TryGetProvider(tp, out provider);
        }
    }
}
