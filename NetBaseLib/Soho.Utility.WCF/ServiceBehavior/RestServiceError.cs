using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Soho.Utility.WCF
{
    [DataContract(Name = "ServiceError", Namespace = "")]
    public class RestServiceError
    {
        [DataMember]
        public int StatusCode { get; set; }

        [DataMember]
        public string StatusDescription { get; set; }

        [DataMember]
        public List<Error> Faults { get; set; }

        public RestServiceError()
        {
            Faults = new List<Error>();
        }
    }

    [DataContract(Name = "Error", Namespace = "")]
    public class Error
    {
        [DataMember]
        public string ErrorCode { get; set; }

        [DataMember]
        public string ErrorDescription { get; set; }
    }
}
