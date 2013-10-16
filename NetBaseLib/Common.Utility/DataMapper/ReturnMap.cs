using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Common.Utility
{
    public class ReturnMap<T> where T : class
    {
        private List<PropertyColumn> m_List = new List<PropertyColumn>();

        internal ReturnMap(Expression<Func<T, object>> expression, string columName)
        {
            this.Add(expression, columName);
        }

        public ReturnMap<T> Add(Expression<Func<T, object>> expression, string columName)
        {
            string proCombineStr = Analyst.GetPropertyCombineStr(expression.Body);
            if (m_List.Exists(p => p.PropertyCombineStr == proCombineStr))
            {
                throw new ApplicationException("属性" + proCombineStr + "重复设置");
            }
            PropertyColumn pc = new PropertyColumn();
            pc.PropertyCombineStr = proCombineStr;
            pc.Column = (columName == null || columName.Trim().Length <= 0) ? proCombineStr : columName;
            m_List.Add(pc);
            return this;
        }

        public ReturnMap<T> Add(Expression<Func<T, object>> expression)
        {
            return Add(expression, null);
        }

        internal List<PropertyColumn> GetMaps()
        {
            return m_List;
        }
    }

    internal class PropertyColumn
    {
        public List<string> PropertyList
        {
            get;
            private set;
        }

        private string m_PropertyCombineStr;
        public string PropertyCombineStr
        {
            get
            {
                return m_PropertyCombineStr;
            }
            set
            {
                m_PropertyCombineStr = value;
                PropertyList = new List<string>(m_PropertyCombineStr.Split('.'));
            }
        }

        public string Column
        {
            get;
            set;
        }
    }
}
