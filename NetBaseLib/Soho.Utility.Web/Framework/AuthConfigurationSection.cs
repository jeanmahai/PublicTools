using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Soho.Utility.Web
{
    public class AuthConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("providers", IsRequired = false)]
        public AuthCollection Auths
        {
            get
            {
                return (AuthCollection)(this["providers"]);
            }
        }

        [ConfigurationProperty("default", IsRequired = true, IsKey = true)]
        public string Default
        {
            get
            {
                return this["default"].ToString();
            }
        }
    }

    public class AuthItem : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return this["name"].ToString();
            }
        }

        [ConfigurationProperty("type", IsRequired = true, IsKey = true)]
        public string Type
        {
            get
            {
                return this["type"].ToString();
            }
        }        
    }

    public class AuthCollection : ConfigurationElementCollection
    {
        public AuthItem this[int index]
        {
            get
            {
                return base.BaseGet(index) as AuthItem;
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
            return new AuthItem();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AuthItem)element).Name;
        }

        public AuthCollection()
        {
            this.AddElementName = "auth";
        }
    }
}
