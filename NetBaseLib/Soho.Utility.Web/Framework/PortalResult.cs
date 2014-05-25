using System;
using System.Runtime.Serialization;

namespace Soho.Utility.Web.Framework
{
    [Serializable]
    [DataContract]
    public class PortalResult
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public object Data { get; set; }
        [DataMember]
        public int Code { get; set; }
    }

    [Serializable]
    [DataContract]
    public class MobilePortalResult : PortalResult
    {
        public MobilePortalResult(PortalResult result)
        {
            this.Success = result.Success;
            this.Message = result.Message;
            this.Data = result.Data;
            this.Code = result.Code;
        }

        [DataMember]
        public string Cookie { get; set; }
    }
}
