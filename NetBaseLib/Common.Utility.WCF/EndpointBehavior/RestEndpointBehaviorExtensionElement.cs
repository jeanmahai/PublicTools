using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Configuration;

namespace Common.Utility.WCF
{
    public class RestEndpointBehaviorExtensionElement : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new RestEndpointBehavior();
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(RestEndpointBehavior);
            }
        }
    }
}
