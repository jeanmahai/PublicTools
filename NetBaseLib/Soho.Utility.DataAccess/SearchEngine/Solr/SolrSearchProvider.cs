using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SolrNet;
using System.Xml.Linq;

namespace Soho.Utility.DataAccess.SearchEngine.Solr
{
    /// <summary>
    /// Solr检索提供程序实现类
    /// </summary>
    public class SolrSearchProvider : ISearchProvider
    {
        private Dictionary<Type, object> s_SearcherDic = new Dictionary<Type, object>();

        /// <summary>
        /// Solr搜索引擎对外暴露的唯一接口，根据传入的条件返回搜索结果
        /// </summary>
        /// <typeparam name="Result">搜索结果对象 </typeparam>
        /// <param name="condition">搜索条件</param>
        /// <returns></returns>
        public Result Query<Result>(SearchCondition condition)
        {
            Type tp = typeof(Result);

            object searchcore = null;
            if (!s_SearcherDic.TryGetValue(tp, out searchcore) || searchcore == null)
            {
                throw new ApplicationException(string.Format("没有为类型:{0} 配置对应的检索器", tp.FullName));
            }

            Searcher<Result> searchCore = searchcore as Searcher<Result>;
            if (searchCore == null)
            {
                throw new ApplicationException(string.Format("类型:{0} 对应的检索器错误,检索器必须是一个Searcher<>类型", tp.FullName));
            }

            return searchCore.Query(condition);
        }

        public static string ServiceBaseUrl;

        /// <summary>
        /// 解析Provider配置文件
        /// </summary>
        /// <param name="config"></param>
        public void ParseConfig(XElement config)
        {
            //try
            //{
                //Solr服务基地址
                ServiceBaseUrl = config.Element("baseUrl").Value.Trim();

                var searchersCfg = config.Element("searchers");
                if (searchersCfg != null)
                {
                    foreach (var core in searchersCfg.Descendants("searcher"))
                    {
                        Type resultType = Type.GetType(core.Attribute("result").Value.Trim(), true);
                        Type searcherType = Type.GetType(core.Attribute("type").Value.Trim(), true);

                        if (!s_SearcherDic.ContainsKey(resultType))
                        {
                            object o = Activator.CreateInstance(searcherType);
                            //searcherType.InvokeMember("ServiceBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty
                            //    , null, o, new object[] { baseUrl });
                            s_SearcherDic.Add(resultType, o);
                        }
                    }
                }
            //}
            //catch (Exception ex)
            //{
            //    //write log
            //    throw;
            //}
        }
    }
}