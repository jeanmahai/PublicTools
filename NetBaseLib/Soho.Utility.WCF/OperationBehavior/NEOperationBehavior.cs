using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.Xml;
using System.Net;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Soho.Utility.WCF
{
    /// <summary>
    /// 通过该Behavior来插入:
    /// 1. 一个自定义的MessageFormatter : DataTableSerializeMessageFormatter，用该Formatter来序列化DataTable和QueryResult对象
    /// 2. 一个OperationInvoker ： ESBMessageOperationInvoker，该OperationInvoker用来记录ESB消息的ID，从而保证service的幂等性
    /// </summary>
    public class NEOperationBehavior : IOperationBehavior
    {
        private IConvert m_Convertor = null;
        private IExceptionHandle m_ExceptionHandler = null;

        public NEOperationBehavior()
        {

        }

        public NEOperationBehavior(string converterTypeName, string exceptionHandlerTypeName)
            : this(converterTypeName != null && converterTypeName.Trim().Length > 0 ? Type.GetType(converterTypeName, true) : null,
            exceptionHandlerTypeName != null && exceptionHandlerTypeName.Trim().Length > 0 ? Type.GetType(exceptionHandlerTypeName, true) : null)
        {

        }

        public NEOperationBehavior(Type converterType, Type exceptionHandlerType)
        {
            if (converterType != null)
            {
                if (!typeof(IConvert).IsAssignableFrom(converterType))
                {
                    throw new ArgumentException("类型" + converterType.FullName + "没有实现接口" + typeof(IConvert).FullName, "type");
                }
                m_Convertor = Activator.CreateInstance(converterType) as IConvert;
            }
            if (exceptionHandlerType != null)
            {
                if (!typeof(IExceptionHandle).IsAssignableFrom(exceptionHandlerType))
                {
                    throw new ArgumentException("类型" + exceptionHandlerType.FullName + "没有实现接口" + typeof(IExceptionHandle).FullName, "type");
                }
                m_ExceptionHandler = Activator.CreateInstance(exceptionHandlerType) as IExceptionHandle;
            }
        }

        public NEOperationBehavior(IConvert convertor, IExceptionHandle exceptionHandler)
        {
            m_Convertor = convertor;
            m_ExceptionHandler = exceptionHandler;
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {

        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            dispatchOperation.Formatter = new DataTableSerializeMessageFormatter(operationDescription, dispatchOperation.Formatter, m_Convertor);
            dispatchOperation.Invoker = new ESBMessageOperationInvoker(dispatchOperation.Invoker, m_ExceptionHandler);
        }

        public void Validate(OperationDescription operationDescription)
        {

        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class NEOperationBehaviorAttribute : Attribute, IOperationBehavior
    {
        NEOperationBehavior m_OperationBehavior;

        public NEOperationBehaviorAttribute()
        {
            m_OperationBehavior = new NEOperationBehavior();
        }

        public NEOperationBehaviorAttribute(string convertorTypeName, string exceptionHandlerTypeName)
        {
            m_OperationBehavior = new NEOperationBehavior(convertorTypeName, exceptionHandlerTypeName);
        }

        public NEOperationBehaviorAttribute(Type convertorType, Type exceptionHandlerType)
        {
            m_OperationBehavior = new NEOperationBehavior(convertorType, exceptionHandlerType);
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
            m_OperationBehavior.AddBindingParameters(operationDescription, bindingParameters);
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            m_OperationBehavior.ApplyClientBehavior(operationDescription, clientOperation);
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            m_OperationBehavior.ApplyDispatchBehavior(operationDescription, dispatchOperation);
        }

        public void Validate(OperationDescription operationDescription)
        {
            m_OperationBehavior.Validate(operationDescription);
        }
    }
}
