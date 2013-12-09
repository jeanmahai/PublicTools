using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Configuration;
using System.Configuration;

namespace Common.Utility.WCF
{
    public class RestServiceBehaviorExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(RestServiceBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new RestServiceBehavior(BizExceptionType, ExceptionHandlerType);
        }

        private ConfigurationPropertyCollection m_Properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (m_Properties == null)
                {
                    m_Properties = new ConfigurationPropertyCollection();
                    m_Properties.Add(new ConfigurationProperty("bizExceptionType", typeof(string), null, null, null, ConfigurationPropertyOptions.None));
                    m_Properties.Add(new ConfigurationProperty("exceptionHandlerType", typeof(string), null, null, null, ConfigurationPropertyOptions.None));
                }
                return base.Properties;
            }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            RestServiceBehaviorExtensionElement element = from as RestServiceBehaviorExtensionElement;
            if (element != null)
            {
                this.BizExceptionType = element.BizExceptionType;
                this.ExceptionHandlerType = element.ExceptionHandlerType;
            }
        }

        [ConfigurationProperty("bizExceptionType", IsRequired = false)]
        public string BizExceptionType
        {
            get { return (string)base["bizExceptionType"]; }
            set { base["bizExceptionType"] = value; }
        }

        [ConfigurationProperty("exceptionHandlerType", IsRequired = false)]
        public string ExceptionHandlerType
        {
            get { return (string)base["exceptionHandlerType"]; }
            set { base["exceptionHandlerType"] = value; }
        }
    }
}
