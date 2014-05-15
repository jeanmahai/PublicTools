using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace Soho.Utility.WCF
{
    public class StandardExceptionBehavior : IServiceBehavior
    {
        private Type[] m_BizExceptionTypeList = null;
        private Type m_ExceptionHandler = null;

        public StandardExceptionBehavior()
        {

        }

        public StandardExceptionBehavior(string customBizExceptionTypeName, string exceptionHandlerTypeName)
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

        public StandardExceptionBehavior(Type[] customBizExceptionTypeList, Type exceptionHandlerType)
        {
            m_BizExceptionTypeList = customBizExceptionTypeList;
            m_ExceptionHandler = exceptionHandlerType;
        }

        public StandardExceptionBehavior(Type customBizExceptionType, Type exceptionHandlerType)
            : this(new Type[] { customBizExceptionType }, exceptionHandlerType)
        {

        }

        public void AddBindingParameters(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                channelDispatcher.ErrorHandlers.Add(new StandardServiceErrorHandler(m_BizExceptionTypeList, m_ExceptionHandler));
            }
        }

        public void Validate(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
        }
    }

    public class StandardServiceErrorHandler : IErrorHandler
    {
        private Type[] m_BizExceptionTypeList;
        private IExceptionHandle m_ExceptionHandler;

        public StandardServiceErrorHandler(Type[] typeList, Type handleType)
        {
            if (typeList != null && typeList.Length > 0)
            {
                foreach (var type in typeList)
                {
                    if (type != null && !type.IsSubclassOf(typeof(Exception)))
                    {
                        throw new ArgumentException("The type must derive from 'System.Exception'.", "type");
                    }
                }
            }
            m_BizExceptionTypeList = typeList;
            if (handleType != null)
            {
                if (!typeof(IExceptionHandle).IsAssignableFrom(handleType))
                {
                    throw new ArgumentException("The type must implement interface 'Nesoft.Utility.WCF.IExceptionHandle'.", "handleType");
                }
                m_ExceptionHandler = (IExceptionHandle)Activator.CreateInstance(handleType);
            }
            if (m_ExceptionHandler == null)
            {
                m_ExceptionHandler = new Soho.Utility.WCF.ExceptionHandler();
            }
        }

        private bool CheckIsBizException(Type type)
        {
            if (m_BizExceptionTypeList != null && m_BizExceptionTypeList.Length > 0)
            {
                foreach (var bizType in m_BizExceptionTypeList)
                {
                    if (bizType != null && bizType.IsAssignableFrom(type))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HandleError(Exception error)
        {
            if (m_ExceptionHandler != null)
            {
                if (error != null && CheckIsBizException(error.GetType())) // BizExcpetion不需要记录日志
                {
                    return true;
                }
                object requestParams;
                OperationContext.Current.OutgoingMessageProperties.TryGetValue("_RequestParams_", out requestParams);
                m_ExceptionHandler.Handle(error, requestParams as object[]);
            }
            return true;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (error == null)
            {
                return;
            }
            string msg;
            string codeName;
            if (CheckIsBizException(error.GetType()))
            {
                msg = error.Message;
                codeName = "1";
            }
            else
            {
                msg = error.ToString();
                codeName = "0";
            }
            FaultException e = new FaultException(msg, new FaultCode(codeName));
            MessageFault m = e.CreateMessageFault();
            fault = Message.CreateMessage(version, m, e.Action);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class StandardExceptionBehaviorAttribute : Attribute, IServiceBehavior
    {
        private StandardExceptionBehavior m_ServiceBehavior;

        public StandardExceptionBehaviorAttribute()
        {
            m_ServiceBehavior = new StandardExceptionBehavior();
        }

        public StandardExceptionBehaviorAttribute(string customBizExceptionTypeName, string exceptionHandlerTypeName)
        {
            m_ServiceBehavior = new StandardExceptionBehavior(customBizExceptionTypeName, exceptionHandlerTypeName);
        }

        public StandardExceptionBehaviorAttribute(Type customBizExceptionType, Type exceptionHandlerType)
        {
            m_ServiceBehavior = new StandardExceptionBehavior(customBizExceptionType, exceptionHandlerType);
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            m_ServiceBehavior.AddBindingParameters(serviceDescription, serviceHostBase, endpoints, bindingParameters);
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            m_ServiceBehavior.ApplyDispatchBehavior(serviceDescription, serviceHostBase);
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            m_ServiceBehavior.Validate(serviceDescription, serviceHostBase);
        }
    }
}
