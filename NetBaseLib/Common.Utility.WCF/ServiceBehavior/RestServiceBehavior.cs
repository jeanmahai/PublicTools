using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Collections.ObjectModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;

namespace Common.Utility.WCF
{
    /// <summary>
    /// 1. 对异常信息的包装，将Service端未捕获的异常转化为JSON返还给客户端  --- RestServiceErrorHandler
    /// 2. 多语言的处理，将客户端Request信息里的语言信息读出，然后设置当前线程的Culture --- RestServiceParameterInspector
    /// 3. 使用url参数上的数据来修改Get方式时的http头 --- RestServiceMessageInspector
    /// </summary>
    public class RestServiceBehavior : IServiceBehavior
    {
        private Type[] m_BizExceptionTypeList = null;
        private Type m_ExceptionHandler = null;

        public RestServiceBehavior()
        {

        }

        public RestServiceBehavior(string customBizExceptionTypeName, string exceptionHandlerTypeName)
        {
            if(customBizExceptionTypeName != null && customBizExceptionTypeName.Trim().Length > 0)
            {
                string[] array = customBizExceptionTypeName.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                m_BizExceptionTypeList = new Type[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    m_BizExceptionTypeList[i] = Type.GetType(array[i].Trim(), true);
                }
            }
            if (exceptionHandlerTypeName != null && exceptionHandlerTypeName.Trim().Length > 0)
            {
                m_ExceptionHandler = Type.GetType(exceptionHandlerTypeName, true);
            }
        }

        public RestServiceBehavior(Type[] customBizExceptionTypeList, Type exceptionHandlerType)
        {
            m_BizExceptionTypeList = customBizExceptionTypeList;
            m_ExceptionHandler = exceptionHandlerType;
        }

        public RestServiceBehavior(Type customBizExceptionType, Type exceptionHandlerType)
            : this(new Type[] { customBizExceptionType }, exceptionHandlerType)
        {

        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                {                    
                    endpointDispatcher.DispatchRuntime.MessageInspectors.Insert(0, new RestServiceMessageInspector());
                    foreach (DispatchOperation dispatchOperation in endpointDispatcher.DispatchRuntime.Operations)
                    {
                        dispatchOperation.ParameterInspectors.Add(new RestServiceParameterInspector());
                    }
                }
                channelDispatcher.ErrorHandlers.Add(new RestServiceErrorHandler(m_BizExceptionTypeList, m_ExceptionHandler));
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            
        }
    }
}
