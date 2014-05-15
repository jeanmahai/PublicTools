using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Soho.Utility.Web.Mvc
{
    public class RoutingCollection : ConfigurationElementCollection
    {
        public RoutingItem this[int index]
        {
            get
            {
                return base.BaseGet(index) as RoutingItem;
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
            return new RoutingItem();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RoutingItem)element).Name;
        }

        public RoutingCollection()
        {
            this.AddElementName = "route";
        }
    }
}
