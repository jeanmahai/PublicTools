using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Linq.Expressions;

namespace Soho.Utility
{
    public class InputMap<T> where T : class
    {
        private List<ParameterProperty<T>> m_List = new List<ParameterProperty<T>>();

        internal InputMap(string parameterName, Expression<Func<T, object>> expression)
        {
            this.Add(parameterName, expression);
        }

        public InputMap<T> Add(string parameterName, Expression<Func<T, object>> expression)
        {
            if (m_List.Exists(p => p.Parameter == parameterName))
            {
                throw new ApplicationException("SQL参数" + parameterName + "重复设置");
            }

            ParameterProperty<T> pc = new ParameterProperty<T>();
            pc.Parameter = parameterName;
            pc.PropertyCombineStr = Analyst.GetPropertyCombineStr(expression.Body);
            //pc.PropertyGetter = expression.Compile();
            m_List.Add(pc);
            return this;
        }

        public List<ParameterProperty<T>> GetMaps()
        {
            return m_List;
        }
    }

    public class ParameterProperty<T>
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

        //public Func<T, object> PropertyGetter
        //{
        //    get;
        //    set;
        //}

        public string Parameter
        {
            get;
            set;
        }
    }
}
