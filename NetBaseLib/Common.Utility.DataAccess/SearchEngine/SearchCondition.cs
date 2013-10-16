using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Nesoft.SearchEngine;

namespace Common.Utility.DataAccess.SearchEngine
{
    /// <summary>
    /// 搜索条件
    /// </summary>
    public class SearchCondition
    {
        #region Field

        private string keyWord;

        private PagingInfo pagingInfo;
        private List<SortItem> sortItems;

        private List<FilterBase> filters;
        private Expression filterExpression;

        private bool isGroupQuery;

        #endregion Field

        #region Property

        /// <summary>
        /// 搜索关键字
        /// </summary>
        public string KeyWord
        {
            get { return keyWord; }
            set { keyWord = value; }
        }

        /// <summary>
        /// 分页信息
        /// </summary>
        public PagingInfo PagingInfo
        {
            get { return this.pagingInfo; }
            set { this.pagingInfo = value; }
        }

        /// <summary>
        /// 排序方式
        /// </summary>
        public List<SortItem> SortItems
        {
            get { return this.sortItems; }
            set { this.sortItems = value; }
        }

        /// <summary>
        /// 过滤条件列表（各项之间为“AND”关系）
        /// </summary>
        public List<FilterBase> Filters
        {
            get { return this.filters; }
            set { this.filters = value; }
        }

        /// <summary>
        /// 查询条件表达式（支持复杂表达式）
        /// </summary>
        public Expression FilterExpression
        {
            get { return this.filterExpression; }
            set { this.filterExpression = value; }
        }

        /// <summary>
        /// 是否分组查询
        /// </summary>
        public bool IsGroupQuery
        {
            get { return this.isGroupQuery; }
            set { this.isGroupQuery = value; }
        }

        #endregion Property
    }
}