using System;
namespace Common.Utility.DataAccess.SearchEngine
{
    public class RangeFilter : FilterBase
    {
        #region Field

        private string from;
        private string to;
        private bool inclusive;

        #endregion Field

        #region Property

        /// <summary>
        /// 起始值
        /// </summary>
        public string From
        {
            get { return from; }
            set { from = value; }
        }

        /// <summary>
        /// 结束值
        /// </summary>
        public string To
        {
            get { return to; }
            set { to = value; }
        }

        /// <summary>
        /// 是否包含边界
        /// </summary>
        public bool Inclusive
        {
            get { return inclusive; }
            set { inclusive = value; }
        }

        #endregion Property

        #region Constructor

        /// <summary>
        /// 范围查询
        /// </summary>
        public RangeFilter()
        {
        }

        /// <summary>
        /// 范围查询
        /// </summary>
        /// <param name="field">字段名称</param>
        /// <param name="from">起始值</param>
        /// <param name="to">结束值</param>
        public RangeFilter(string field, string from, string to)
        {
            this.field = field;
            this.from = from;
            this.to = to;
            this.inclusive = true;
        }

        /// <summary>
        /// 范围查询
        /// </summary>
        /// <param name="field">字段名称</param>
        /// <param name="from">起始值</param>
        /// <param name="to">结束值</param>
        /// <param name="inclusive">是否包含边界</param>
        public RangeFilter(string field, string from, string to, bool inclusive)
        {
            this.field = field;
            this.from = from;
            this.to = to;
            this.inclusive = inclusive;
        }

        #endregion Constructor
    }
}