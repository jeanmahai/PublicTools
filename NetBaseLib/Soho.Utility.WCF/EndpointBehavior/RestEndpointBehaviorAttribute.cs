using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Soho.Utility.WCF
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RestEndpointBehaviorAttribute : Attribute, IEndpointBehavior
    {
        private RestEndpointBehavior m_RestEndpointBehavior = new RestEndpointBehavior();

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            m_RestEndpointBehavior.AddBindingParameters(endpoint, bindingParameters);
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            m_RestEndpointBehavior.ApplyClientBehavior(endpoint, clientRuntime);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            m_RestEndpointBehavior.ApplyDispatchBehavior(endpoint, endpointDispatcher);
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            m_RestEndpointBehavior.Validate(endpoint);
        }
    }
}
