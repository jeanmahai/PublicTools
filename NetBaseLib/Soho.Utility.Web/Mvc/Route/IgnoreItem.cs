using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Soho.Utility.Web.Mvc
{
    public class IgnoreItem : ConfigurationElement
    {
        [ConfigurationProperty("url", IsRequired=true, IsKey=true)]
        public string Url
        {
            get
            {
                return this["url"].ToString();
            }
        }

        [ConfigurationProperty("constraints", IsRequired = false)]
        public ConstraintCollection Constraints
        {
            get
            {
                return this["constraints"] as ConstraintCollection;
            }
        }
    }
}
