using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Soho.Utility.Web.Mvc
{
    public class ConstraintCollection : ConfigurationElementCollection
    {
        public Constraint this[int index]
        {
            get
            {
                return base.BaseGet(index) as Constraint;
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
            return new Constraint();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Constraint)element).Name;
        }
    }
}
