using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Common.Utility.Web.Mvc
{
    public class RoutingItem:ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired=true, IsKey=true)]
        public string Name
        {
            get
            {
                return this["name"].ToString();
            }
        }

        [ConfigurationProperty("url", IsRequired=true, IsKey=true)]
        public string Url
        {
            get
            {
                return this["url"].ToString();
            }
        }

        [ConfigurationProperty("controller", IsRequired=true)]
        public string Controller
        {
            get
            {
                return this["controller"].ToString();
            }
        }

        [ConfigurationProperty("action", IsRequired=true)]
        public string Action
        {
            get
            {
                return this["action"].ToString();
            }
        }

        [ConfigurationProperty("parameters", IsRequired = false)]
        public ParameterCollection Paramaters
        {
            get
            {
                return this["parameters"] as ParameterCollection;
            }
        }

    }
}
