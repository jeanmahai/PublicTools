using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Configuration;

namespace Common.Utility.WCF
{
    /// <summary>
    /// 在服务端的Endpoint级别注入新的IDispatchOperationSelector，以便根据客户端发起的HTTP Request的Method来修改WCF消息的HttpRequestMessageProperty的Method
    /// </summary>
    public class RestEndpointBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.OperationSelector = new RestServiceOperationSelector(endpointDispatcher.DispatchRuntime.OperationSelector);
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            
        }
    }
}
