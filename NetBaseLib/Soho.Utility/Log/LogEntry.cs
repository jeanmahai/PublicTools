using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Soho.Utility
{
    [Serializable]
    [DataContract]
    public class LogEntry
    {
        [DataMember]
        public Guid LogID { get; set; }

        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public string Category { get; set; }

        [DataMember]
        public string RequestUrl { get; set; }

        [DataMember]
        public string UserHostName { get; set; }

        [DataMember]
        public string UserHostAddress { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public string ServerIP { get; set; }

        [DataMember]
        public string ServerName { get; set; }

        [DataMember]
        public DateTime ServerTime { get; set; }

        [DataMember]
        public string ReferenceKey { get; set; }

        [DataMember]
        public int ProcessID { get; set; }

        [DataMember]
        public string ProcessName { get; set; }

        [DataMember]
        public int ThreadID { get; set; }

        [DataMember]
        public List<ExtendedPropertyData> ExtendedProperties { get; set; }
    }

    [Serializable]
    [DataContract]
    public class ExtendedPropertyData
    {
        [DataMember]
        public string PropertyName { get; set; }

        [DataMember]
        public string PropertyValue { get; set; }
    }
}