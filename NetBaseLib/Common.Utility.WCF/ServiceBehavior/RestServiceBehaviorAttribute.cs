using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;

namespace Common.Utility.WCF
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RestServiceBehaviorAttribute : Attribute, IServiceBehavior
    {
        private RestServiceBehavior m_RestServiceBehavior;

        public RestServiceBehaviorAttribute()
        {
            m_RestServiceBehavior = new RestServiceBehavior();
        }

        public RestServiceBehaviorAttribute(string customBizExceptionTypeName, string exceptionHandlerTypeName)
        {
            m_RestServiceBehavior = new RestServiceBehavior(customBizExceptionTypeName, exceptionHandlerTypeName);
        }

        public RestServiceBehaviorAttribute(Type customBizExceptionType, Type exceptionHandlerType)
        {
            m_RestServiceBehavior = new RestServiceBehavior(customBizExceptionType, exceptionHandlerType);
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            m_RestServiceBehavior.AddBindingParameters(serviceDescription, serviceHostBase, endpoints, bindingParameters);
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            m_RestServiceBehavior.ApplyDispatchBehavior(serviceDescription, serviceHostBase);
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            m_RestServiceBehavior.Validate(serviceDescription, serviceHostBase);
        }
    }
}
