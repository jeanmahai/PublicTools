using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Reflection;

namespace Common.Utility.WCF
{
    internal class RestWebServiceHostFactory : WebServiceHostFactory
    {
        private const int MAX_MSG_SIZE = 12000000;

        private string m_BizExceptionTypeName;
        private string m_ExceptionHandlerTypeName;
        private string m_ConverterTypeName;

        public RestWebServiceHostFactory(string bizExceptionTypeName, string exceptionHandlerTypeName, string converterTypeName)
        {
            m_BizExceptionTypeName = bizExceptionTypeName;
            m_ExceptionHandlerTypeName = exceptionHandlerTypeName;
            m_ConverterTypeName = converterTypeName;
        }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            ServiceHost host = base.CreateServiceHost(serviceType, baseAddresses);
            host.Opening += new EventHandler(Host_Opening);
            host.Opened += new EventHandler(Host_Opened);
            return host;
        }

        private void Host_Opened(object sender, EventArgs e)
        {
            
        }

        private void Host_Opening(object sender, EventArgs e)
        {
            ServiceHost host = sender as ServiceHost;
            if (host == null)
            {
                return;
            }
            RestServiceBehavior b = host.Description.Behaviors.Find<RestServiceBehavior>();
            if (b == null)
            {
                host.Description.Behaviors.Add(new RestServiceBehavior(m_BizExceptionTypeName, m_ExceptionHandlerTypeName));
            }
            ServiceBehaviorAttribute bb = host.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            if (bb == null)
            {
                bb = new ServiceBehaviorAttribute();
                host.Description.Behaviors.Add(bb);
            }
            bb.ConcurrencyMode = ConcurrencyMode.Multiple;
            bb.AddressFilterMode = AddressFilterMode.Any;
            bb.InstanceContextMode = InstanceContextMode.Single;
            bb.MaxItemsInObjectGraph = Int32.MaxValue;
            if (ServiceHostingEnvironment.AspNetCompatibilityEnabled)
            {
                AspNetCompatibilityRequirementsAttribute a = host.Description.Behaviors.Find<AspNetCompatibilityRequirementsAttribute>();
                if (a == null)
                {
                    host.Description.Behaviors.Add(new AspNetCompatibilityRequirementsAttribute() { RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed });
                }
                else
                {
                    a.RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed;
                }
            }
            //------- 设置 dataContractSerializer的 maxItemsInObjectGraph属性为int.MaxValue
            Type t = host.GetType();
            object obj = t.Assembly.CreateInstance("System.ServiceModel.Dispatcher.DataContractSerializerServiceBehavior", true, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { false, int.MaxValue }, null, null);
            IServiceBehavior myServiceBehavior = obj as IServiceBehavior;
            if (myServiceBehavior != null)
            {
                host.Description.Behaviors.Add(myServiceBehavior);
            }
            //-------
            foreach (var endpoint in host.Description.Endpoints)
            {
                WebHttpBinding binding = endpoint.Binding as WebHttpBinding;
                if (binding != null)
                {
                    binding.MaxReceivedMessageSize = MAX_MSG_SIZE;
                    binding.ReaderQuotas.MaxStringContentLength = MAX_MSG_SIZE;
                    binding.ReaderQuotas.MaxArrayLength = MAX_MSG_SIZE;
                    binding.ReaderQuotas.MaxBytesPerRead = MAX_MSG_SIZE;
                    binding.ReaderQuotas.MaxDepth = MAX_MSG_SIZE;
                    binding.ReaderQuotas.MaxNameTableCharCount = MAX_MSG_SIZE;
                }                
                WebHttpEndpoint p = endpoint as WebHttpEndpoint;
                if (p != null)
                {
                    p.HelpEnabled = true;
                    p.AutomaticFormatSelectionEnabled = true;
                }
                WebHttpBehavior b0 = endpoint.Behaviors.Find<WebHttpBehavior>();
                if (b0 == null)
                {
                    endpoint.Behaviors.Add(new WebHttpBehavior() { HelpEnabled = true, FaultExceptionEnabled = true, AutomaticFormatSelectionEnabled = true });
                }
                else
                {
                    b0.HelpEnabled = true;
                    b0.FaultExceptionEnabled = true;
                    b0.AutomaticFormatSelectionEnabled = true;
                }
                RestEndpointBehavior b1 = endpoint.Behaviors.Find<RestEndpointBehavior>();
                if (b1 == null)
                {
                    endpoint.Behaviors.Add(new RestEndpointBehavior());
                }
                foreach (var operation in endpoint.Contract.Operations)
                {
                    NEOperationBehavior b2 = operation.Behaviors.Find<NEOperationBehavior>();
                    if (b2 == null)
                    {
                        operation.Behaviors.Add(new NEOperationBehavior(m_ConverterTypeName, m_ExceptionHandlerTypeName));
                    }
                }
            }
        }
    }
}
