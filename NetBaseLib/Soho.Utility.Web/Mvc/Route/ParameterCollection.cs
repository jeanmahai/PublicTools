using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Soho.Utility.Web.Mvc
{
    public class ParameterCollection : ConfigurationElementCollection
    {
        public Parameter this[int index]
        {
            get
            {
                return base.BaseGet(index) as Parameter;
            }

            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Parameter();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Parameter)element).Name;
        }
    }
}
