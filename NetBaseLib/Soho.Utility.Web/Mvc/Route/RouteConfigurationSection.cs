using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;

namespace Soho.Utility.Web.Mvc
{
    public class RouteConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("defaultNamespace", IsRequired = false)]
        public DefaultNamespace DefaultNamespace
        {
            get
            {
                return (DefaultNamespace)(this["defaultNamespace"]);
            }
        }

        [ConfigurationProperty("ignore", IsRequired = false)]
        public IgnoreCollection Ignore
        {
            get
            {
                return (IgnoreCollection)(this["ignore"]);
            }
        }

        [ConfigurationProperty("areas", IsRequired = false)]
        public AreaCollection Areas
        {
            get
            {
                return (AreaCollection)(this["areas"]);
            }
        }

        [ConfigurationProperty("map", IsRequired = false)]
        public RoutingCollection Map
        {
            get
            {
                return (RoutingCollection)(this["map"]);
            }
        }       
    }
}
