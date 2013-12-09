using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.MsmqIntegration;
using System.ServiceModel.Web;
using System.Text;
using System.Web;

namespace Common.Utility.WCF
{
    public class StandardServiceFactory : ServiceHostFactory
    {
        private const int MAX_MSG_SIZE = int.MaxValue;
        private BindingType m_BindingType;
        private string m_BizExceptionTypeName;
        private string m_ExceptionHandlerTypeName;

        public StandardServiceFactory(string bizExceptionTypeName, string exceptionHandlerTypeName)
            : this(BindingType.BasicHttp, bizExceptionTypeName, exceptionHandlerTypeName)
        {

        }

        public StandardServiceFactory(BindingType bindingType, string bizExceptionTypeName, string exceptionHandlerTypeName)
        {
            m_BindingType = bindingType;
            m_BizExceptionTypeName = bizExceptionTypeName;
            m_ExceptionHandlerTypeName = exceptionHandlerTypeName;
        }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            ServiceHost host = new ServiceHost(serviceType, baseAddresses);  //base.CreateServiceHost(serviceType, baseAddresses);
            Type contractType = FindServiceContractInterface(serviceType);
            host.AddServiceEndpoint(contractType, FindBinding(m_BindingType), "");
            ServiceMetadataBehavior b1 = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (b1 == null)
            {
                b1 = new ServiceMetadataBehavior() { HttpGetEnabled = true };
                host.Description.Behaviors.Add(b1);
            }
            else
            {
                b1.HttpGetEnabled = true;
            }
            StandardExceptionBehavior b = host.Description.Behaviors.Find<StandardExceptionBehavior>();
            if (b == null)
            {
                host.Description.Behaviors.Add(new StandardExceptionBehavior(m_BizExceptionTypeName, m_ExceptionHandlerTypeName));
            }
            //if (url.IndexOf("http://") < 0)
            //{
            //    b1.HttpGetUrl = new Uri(string.Format("http://{0}{1}{2}", hostIP, (service.Port > 0 ? (":" + service.Port) : string.Empty), url));
            //}
            ServiceDebugBehavior b2 = host.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (b2 == null)
            {
                host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
            }
            else
            {
                b2.IncludeExceptionDetailInFaults = true;
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

            return host;
        }

        private static bool CheckHasDataContractAttribute(Type type)
        {
            object[] tmp = type.GetCustomAttributes(typeof(ServiceContractAttribute), false);
            return tmp != null && tmp.Length > 0 && tmp[0] is ServiceContractAttribute;
        }

        public static Type FindServiceContractInterface(Type type)
        {
            if (CheckHasDataContractAttribute(type))
            {
                return type;
            }
            Type[] interTypes = type.GetInterfaces();
            if (interTypes == null || interTypes.Length <= 0)
            {
                throw new ApplicationException("Can't find the WCF service contract for type '" + type.FullName + "'");
            }
            foreach (var t in interTypes)
            {
                if (CheckHasDataContractAttribute(t))
                {
                    return t;
                }
            }
            throw new ApplicationException("Can't find the WCF service contract for type '" + type.FullName + "'");
        }

        private static Binding FindBinding(BindingType bType)
        {
            switch (bType)
            {
                case BindingType.WebHttp:
                    WebHttpBinding webHttpBinding = new WebHttpBinding();
                    webHttpBinding.UseDefaultWebProxy = false;
                    webHttpBinding.Security.Mode = WebHttpSecurityMode.None;
                    webHttpBinding.MaxReceivedMessageSize = MAX_MSG_SIZE;
                    webHttpBinding.ReaderQuotas.MaxStringContentLength = MAX_MSG_SIZE;
                    webHttpBinding.ReaderQuotas.MaxArrayLength = MAX_MSG_SIZE;
                    webHttpBinding.ReaderQuotas.MaxBytesPerRead = MAX_MSG_SIZE;
                    webHttpBinding.ReaderQuotas.MaxDepth = MAX_MSG_SIZE;
                    webHttpBinding.ReaderQuotas.MaxNameTableCharCount = MAX_MSG_SIZE;
                    return webHttpBinding;
                case BindingType.BasicHttp:
                    BasicHttpBinding basicHttpBinding = new BasicHttpBinding();
                    basicHttpBinding.UseDefaultWebProxy = false;
                    basicHttpBinding.Security.Mode = BasicHttpSecurityMode.None;
                    basicHttpBinding.MaxReceivedMessageSize = MAX_MSG_SIZE;
                    basicHttpBinding.ReaderQuotas.MaxStringContentLength = MAX_MSG_SIZE;
                    basicHttpBinding.ReaderQuotas.MaxArrayLength = MAX_MSG_SIZE;
                    basicHttpBinding.ReaderQuotas.MaxBytesPerRead = MAX_MSG_SIZE;
                    basicHttpBinding.ReaderQuotas.MaxDepth = MAX_MSG_SIZE;
                    basicHttpBinding.ReaderQuotas.MaxNameTableCharCount = MAX_MSG_SIZE;
                    return basicHttpBinding;
                case BindingType.MsmqIntegration:
                    MsmqIntegrationBinding msmqIntegrationBinding = new MsmqIntegrationBinding();
                    msmqIntegrationBinding.ExactlyOnce = false;
                    msmqIntegrationBinding.UseSourceJournal = true;
                    msmqIntegrationBinding.Security.Mode = MsmqIntegrationSecurityMode.None;
                    msmqIntegrationBinding.MaxReceivedMessageSize = MAX_MSG_SIZE;
                    return msmqIntegrationBinding;
                case BindingType.MsmqIntegrationForPubsub:
                    MsmqIntegrationBinding msmqIntegrationForPubsubBinding = new MsmqIntegrationBinding();
                    msmqIntegrationForPubsubBinding.ExactlyOnce = false;
                    msmqIntegrationForPubsubBinding.UseSourceJournal = true;
                    msmqIntegrationForPubsubBinding.Security.Mode = MsmqIntegrationSecurityMode.Transport;
                    msmqIntegrationForPubsubBinding.Security.Transport.MsmqAuthenticationMode = MsmqAuthenticationMode.WindowsDomain;
                    msmqIntegrationForPubsubBinding.MaxReceivedMessageSize = MAX_MSG_SIZE;
                    return msmqIntegrationForPubsubBinding;
                case BindingType.NetMsmq:
                    NetMsmqBinding netMsmqBinding = new NetMsmqBinding();
                    //netMsmqBinding.ReaderQuotas.MaxStringContentLength = serviceClient.ReaderQuotasMaxStringContentLength;
                    netMsmqBinding.ExactlyOnce = false;
                    netMsmqBinding.UseSourceJournal = true;
                    netMsmqBinding.MaxReceivedMessageSize = MAX_MSG_SIZE;
                    netMsmqBinding.ReaderQuotas.MaxStringContentLength = MAX_MSG_SIZE;
                    netMsmqBinding.ReaderQuotas.MaxArrayLength = MAX_MSG_SIZE;
                    netMsmqBinding.ReaderQuotas.MaxBytesPerRead = MAX_MSG_SIZE;
                    netMsmqBinding.ReaderQuotas.MaxDepth = MAX_MSG_SIZE;
                    netMsmqBinding.ReaderQuotas.MaxNameTableCharCount = MAX_MSG_SIZE;
                    //if (credentialType.HasValue)
                    //{
                    //    netMsmqBinding.Security.Mode = NetMsmqSecurityMode.Message;
                    //    netMsmqBinding.Security.Message.ClientCredentialType = GetMessageCredentialType(credentialType.Value);
                    //    netMsmqBinding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Default;
                    //}
                    return netMsmqBinding;
                case BindingType.NetTcp:
                    NetTcpBinding netTcpBinding = new NetTcpBinding();
                    //netTcpBinding.ReaderQuotas.MaxStringContentLength = serviceClient.ReaderQuotasMaxStringContentLength;
                    //netTcpBinding.ReliableSession.Enabled = serviceClient.ReliableSessionEnabled;
                    //netTcpBinding.ReliableSession.Ordered = serviceClient.ReliableSessionOrdered;
                    netTcpBinding.MaxReceivedMessageSize = MAX_MSG_SIZE;
                    netTcpBinding.ReaderQuotas.MaxStringContentLength = MAX_MSG_SIZE;
                    netTcpBinding.ReaderQuotas.MaxArrayLength = MAX_MSG_SIZE;
                    netTcpBinding.ReaderQuotas.MaxBytesPerRead = MAX_MSG_SIZE;
                    netTcpBinding.ReaderQuotas.MaxDepth = MAX_MSG_SIZE;
                    netTcpBinding.ReaderQuotas.MaxNameTableCharCount = MAX_MSG_SIZE;
                    //if (credentialType.HasValue)
                    //{
                    //    netTcpBinding.Security.Mode = SecurityMode.Message;
                    //    netTcpBinding.Security.Message.ClientCredentialType = GetMessageCredentialType(credentialType.Value);
                    //    netTcpBinding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Default;
                    //}
                    return netTcpBinding;
                case BindingType.NetNamedPipe:
                    NetNamedPipeBinding netNamedPipeBinding = new NetNamedPipeBinding();
                    netNamedPipeBinding.Security.Mode = NetNamedPipeSecurityMode.None;
                    netNamedPipeBinding.TransactionFlow = false;
                    //netNamedPipeBinding.MaxConnections = 100;
                    netNamedPipeBinding.MaxReceivedMessageSize = MAX_MSG_SIZE;
                    netNamedPipeBinding.ReaderQuotas.MaxStringContentLength = MAX_MSG_SIZE;
                    netNamedPipeBinding.ReaderQuotas.MaxArrayLength = MAX_MSG_SIZE;
                    netNamedPipeBinding.ReaderQuotas.MaxBytesPerRead = MAX_MSG_SIZE;
                    netNamedPipeBinding.ReaderQuotas.MaxDepth = MAX_MSG_SIZE;
                    netNamedPipeBinding.ReaderQuotas.MaxNameTableCharCount = MAX_MSG_SIZE;
                    return netNamedPipeBinding;
                case BindingType.WSDualHttp:
                    WSDualHttpBinding wsDualHttpBinding = new WSDualHttpBinding();
                    //wsDualHttpBinding.ReaderQuotas.MaxStringContentLength = serviceClient.ReaderQuotasMaxStringContentLength;
                    wsDualHttpBinding.UseDefaultWebProxy = false;
                    //wsDualHttpBinding.ReliableSession.Ordered = serviceClient.ReliableSessionOrdered;
                    wsDualHttpBinding.MaxReceivedMessageSize = MAX_MSG_SIZE;
                    wsDualHttpBinding.ReaderQuotas.MaxStringContentLength = MAX_MSG_SIZE;
                    wsDualHttpBinding.ReaderQuotas.MaxArrayLength = MAX_MSG_SIZE;
                    wsDualHttpBinding.ReaderQuotas.MaxBytesPerRead = MAX_MSG_SIZE;
                    wsDualHttpBinding.ReaderQuotas.MaxDepth = MAX_MSG_SIZE;
                    wsDualHttpBinding.ReaderQuotas.MaxNameTableCharCount = MAX_MSG_SIZE;
                    //if (credentialType.HasValue)
                    //{
                    //    wsDualHttpBinding.Security.Mode = WSDualHttpSecurityMode.Message;
                    //    wsDualHttpBinding.Security.Message.ClientCredentialType = GetMessageCredentialType(credentialType.Value);
                    //    wsDualHttpBinding.Security.Message.NegotiateServiceCredential = true;
                    //    wsDualHttpBinding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Default;
                    //}
                    return wsDualHttpBinding;
                default:
                    WSHttpBinding wsHttpBinding = new WSHttpBinding();
                    wsHttpBinding.UseDefaultWebProxy = false;
                    //wsHttpBinding.ReliableSession.Enabled = serviceClient.ReliableSessionEnabled;
                    //wsHttpBinding.ReliableSession.Ordered = serviceClient.ReliableSessionOrdered;
                    wsHttpBinding.MaxReceivedMessageSize = MAX_MSG_SIZE;
                    wsHttpBinding.ReaderQuotas.MaxStringContentLength = MAX_MSG_SIZE;
                    wsHttpBinding.ReaderQuotas.MaxArrayLength = MAX_MSG_SIZE;
                    wsHttpBinding.ReaderQuotas.MaxBytesPerRead = MAX_MSG_SIZE;
                    wsHttpBinding.ReaderQuotas.MaxDepth = MAX_MSG_SIZE;
                    wsHttpBinding.ReaderQuotas.MaxNameTableCharCount = MAX_MSG_SIZE;
                    wsHttpBinding.ReceiveTimeout = new TimeSpan(23, 59, 59);
                    //if (credentialType.HasValue)
                    //{
                    //    wsHttpBinding.Security.Mode = SecurityMode.Message;
                    //    wsHttpBinding.Security.Message.ClientCredentialType = GetMessageCredentialType(credentialType.Value);
                    //    wsHttpBinding.Security.Message.NegotiateServiceCredential = true;
                    //    wsHttpBinding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Default;
                    //    wsHttpBinding.Security.Message.EstablishSecurityContext = true;
                    //}
                    return wsHttpBinding;
            }
        }
    }

    [ServiceContract(Namespace = "http://nesoft")]
    public interface IExportService
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "/", Method = "*")]
        List<ServiceInfo> GetAllService();
    }

    [DataContract(Namespace = "http://nesoft")]
    public class ServiceInfo
    {
        [DataMember]
        public string Address
        {
            get;
            set;
        }

        [DataMember]
        public BindingType Binding
        {
            get;
            set;
        }

        [DataMember]
        public string Contract
        {
            get;
            set;
        }
    }

    [DataContract(Namespace = "http://nesoft")]
    public enum BindingType
    {
        [EnumMember]
        WebHttp,
        [EnumMember]
        BasicHttp,
        [EnumMember]
        WSHttp,
        [EnumMember]
        WSDualHttp,
        [EnumMember]
        NetTcp,
        [EnumMember]
        MsmqIntegration,
        [EnumMember]
        MsmqIntegrationForPubsub,
        [EnumMember]
        NetMsmq,
        [EnumMember]
        NetNamedPipe
    }

    public class ExportService : IExportService
    {
        private static List<ServiceInfo> s_List = new List<ServiceInfo>();

        public static void AddServiceInfo(string adress, BindingType bType, string contract)
        {
            s_List.Add(new ServiceInfo { Address = adress, Binding = bType, Contract = contract });
        }

        public List<ServiceInfo> GetAllService()
        {
            return s_List;
        }
    }
}
