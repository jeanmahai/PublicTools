using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Soho.Utility.Web.Mvc
{
    public class AreaItem : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return this["name"].ToString();
            }
        }

        [ConfigurationProperty("map", IsRequired = true)]
        public RoutingCollection Map
        {
            get
            {
                return (RoutingCollection)(this["map"]);
            }
        }

        [ConfigurationProperty("namespaces", IsRequired = false)]
        public NamespacesCollection Namespaces
        {
            get
            {
                return (NamespacesCollection)(this["namespaces"]);
            }
        }
    }

    public class Namespace : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"].ToString();
            }
        }
    }

    public class NamespacesCollection : ConfigurationElementCollection
    {
        public NamespacesCollection()
        {
            this.AddElementName = "namespace";
        }

        public Namespace this[int index]
        {
            get
            {
                return base.BaseGet(index) as Namespace;
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
            return new Namespace();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Namespace)element).Name;
        }
    }
}
