using System;
using System.Runtime.Serialization;

namespace Soho.Utility.DataAccess.SearchEngine
{
    public class FieldFilter : FilterBase
    {
        #region Field

        private string _value;

        #endregion Field

        #region Property

        /// <summary>
        /// 字段值
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        #endregion Property

        #region Constructor

        /// <summary>
        /// 字段查询
        /// </summary>
        public FieldFilter()
        {
        }

        /// <summary>
        /// 字段查询
        /// </summary>
        /// <param name="field">字段名称</param>
        /// <param name="value">字段值</param>
        public FieldFilter(string field, string value)
        {
            this.field = field;
            this._value = value;
        }

        #endregion Constructor
    }
}