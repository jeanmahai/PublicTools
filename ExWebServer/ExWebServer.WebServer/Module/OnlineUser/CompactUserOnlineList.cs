using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ExWebServer.WebServer.Module.OnlineUser
{
    [DataContract]
    public class CompactUserOnlineList
    {
        /// <summary>
        /// UserID
        /// </summary>
        [DataMember]
        public int U { get; set; }
        /// <summary>
        /// Online tag: 0 = offline;1 = online
        /// </summary>
        [DataMember]
        public int T { get; set; }
    }
}
