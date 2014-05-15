using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Soho.Utility.Web.Mvc
{
    public class AreaCollection : ConfigurationElementCollection
    {
        public AreaItem this[int index]
        {
            get
            {
                return base.BaseGet(index) as AreaItem;
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
            return new AreaItem();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AreaItem)element).Name;
        }

        public AreaCollection()
        {
            this.AddElementName = "area";
        }
    }
}
