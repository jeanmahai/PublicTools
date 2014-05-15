using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.MsmqIntegration;
using System.Text;

namespace Soho.Utility
{
    public static class ServiceFactory
    {
        private static string s_Url = string.Empty;
        public static string BaseUrl
        {
            get
            {
                return s_Url;
            }
            set
            {
                s_Url = value;
                if (s_Url != null)
                {
                    s_Url = s_Url.Trim().TrimEnd('\\').TrimEnd('/');
                }
            }
        }

        public static Func<Dictionary<string, string>> ContextSetting
        {
            get;
            set;
        }
    }

    public static class ServiceFactory<T> where T : class
    {
        private const int MAX_MSG_SIZE = int.MaxValue;
        //private static T s_Instance = null;
        //private static object s_Sync = new object();
        private static string s_Contract = typeof(T).FullName;
        public static T Instance
        {
            get
            {
                //if (s_Instance == null)
                //{
                //    lock (s_Sync)
                //    {
                //        if (s_Instance == null)
                //        {
                //            s_Instance = CreateInstance();
                //        }
                //    }
                //}
                //return s_Instance;
                return CreateInstance();
            }
        }

        private static T CreateInstance()
        {
            ServiceInfo info = null;
            List<ServiceInfo> list = ExportService.GetAllService();
            if (list != null && list.Count > 0)
            {
                info = list.Find(x => x.Contract == s_Contract);
            }
            if (info == null)
            {
                return null;
            }
            EndpointAddress endpointAddress = new EndpointAddress(ServiceFactory.BaseUrl + "/" + info.Address);
            ServiceEndpoint httpEndpoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(T)), FindBinding(info.Binding), endpointAddress);
            ChannelFactory<T> factory = new ChannelFactory<T>(httpEndpoint);
            if (info.Binding == BindingType.WebHttp)
            {
                WebHttpBehavior wh = factory.Endpoint.Behaviors.Find<WebHttpBehavior>();
                if (wh == null)
                {
                    factory.Endpoint.Behaviors.Add(new WebHttpBehavior());
                }
            }
            HeaderContextAttribute hc = factory.Endpoint.Behaviors.Find<HeaderContextAttribute>();
            if (hc == null)
            {
                factory.Endpoint.Behaviors.Add(new HeaderContextAttribute(info.Binding == BindingType.WebHttp));
            }
            foreach (OperationDescription op in factory.Endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                }
                else
                {
                    op.Behaviors.Add(new DataContractSerializerOperationBehavior(op) { MaxItemsInObjectGraph = int.MaxValue });
                }
            }
            return factory.CreateChannel();
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

    internal class ExportService
    {
        private static List<ServiceInfo> s_List = null;
        private static object s_Sync = new object();

        public static List<ServiceInfo> GetAllService()
        {
            if (s_List == null)
            {
                lock (s_Sync)
                {
                    if (s_List == null)
                    {
                        s_List = GetAllServiceFromRemote();
                    }
                }
            }
            return s_List;
        }

        private static List<ServiceInfo> GetAllServiceFromRemote()
        {
            IExportService service;
            ServiceEndpoint httpEndpoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(IExportService)), new BasicHttpBinding(), new EndpointAddress(ServiceFactory.BaseUrl + "/WCFExportService"));
            using (ChannelFactory<IExportService> factory =
                new ChannelFactory<IExportService>(httpEndpoint))
            {
                service = factory.CreateChannel();
                return service.GetAllService();
            }
        }
    }

    [ServiceContract(Namespace = "http://nesoft")]
    internal interface IExportService
    {
        [OperationContract]
        List<ServiceInfo> GetAllService();
    }

    [DataContract(Namespace = "http://nesoft")]
    internal class ServiceInfo
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
    internal enum BindingType
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

    internal class HeaderContextClientMessageInspector : IClientMessageInspector
    {
        private bool m_IsWebHttpBinding = false;
        public HeaderContextClientMessageInspector(bool isWebHttpBinding)
        {
            m_IsWebHttpBinding = isWebHttpBinding;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {

        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            Dictionary<string, string> dic = null;
            var handler = ServiceFactory.ContextSetting;
            if (handler != null)
            {
                dic = handler();
            }
            if (dic == null)
            {
                return null;
            }
            Dictionary<string, string>.Enumerator iterator = dic.GetEnumerator();
            HttpRequestMessageProperty hrmp = null;
            if (m_IsWebHttpBinding)
            {
                hrmp = request.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
                if (hrmp == null)
                {
                    hrmp = new HttpRequestMessageProperty();
                    request.Properties.Add(HttpRequestMessageProperty.Name, hrmp);
                }
            }
            while (iterator.MoveNext())
            {
                string val = iterator.Current.Value;
                if (val == null)
                {
                    continue;
                }
                val = HeaderContextHelper.EncryptAndSign(val);
                if (m_IsWebHttpBinding)
                {
                    hrmp.Headers.Add(HeaderContextHelper.BuildKey(iterator.Current.Key), val);
                }
                else
                {
                    MessageHeader clientHeader = MessageHeader.CreateHeader(HeaderContextHelper.BuildKey(iterator.Current.Key), string.Empty, val);
                    request.Headers.Add(clientHeader);
                }
            }
            return null;
        }        
    }

    internal class HeaderContextAttribute : Attribute, IEndpointBehavior, IOperationBehavior
    {
        private bool m_IsWebHttpBinding = false;
        public HeaderContextAttribute()
            : this(false)
        {

        }

        public HeaderContextAttribute(bool isWebHttpBinding)
        {
            m_IsWebHttpBinding = isWebHttpBinding;
        }

        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new HeaderContextClientMessageInspector(m_IsWebHttpBinding));
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {

        }

        public void Validate(ServiceEndpoint endpoint)
        {

        }

        #endregion

        #region IOperationBehavior Members

        public void AddBindingParameters(OperationDescription operationDescription, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(OperationDescription operationDescription, System.ServiceModel.Dispatcher.ClientOperation clientOperation)
        {
            clientOperation.Parent.MessageInspectors.Add(new HeaderContextClientMessageInspector(m_IsWebHttpBinding));
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, System.ServiceModel.Dispatcher.DispatchOperation dispatchOperation)
        {

        }

        public void Validate(OperationDescription operationDescription)
        {

        }

        #endregion
    }
}
