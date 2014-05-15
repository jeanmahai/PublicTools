using System;

namespace Soho.Utility.DataAccess.SearchEngine
{
    /// <summary>
    /// 检索器抽象基类，对外提供Query检索方法
    /// </summary>
    /// <typeparam name="Result">检索结果返回类型</typeparam>
    public abstract class Searcher<Result>
    {
        public Searcher()
        {
            Init();
        }

        /// <summary>
        /// 真正的检索方法，需要派生类进行实现
        /// </summary>
        /// <param name="condition">查询条件</param>
        /// <returns>检索结果</returns>
        protected abstract Result GetSearchResult(SearchCondition condition);

        /// <summary>
        /// 检索器初始化，派生类根据需要进行重写
        /// </summary>
        protected virtual void Init() { }

        /// <summary>
        /// 对外提供的检索接口
        /// </summary>
        /// <param name="condition">查询条件</param>
        /// <returns>检索结果</returns>
        public Result Query(SearchCondition condition)
        {
            return GetSearchResult(condition);
        }
    }
}