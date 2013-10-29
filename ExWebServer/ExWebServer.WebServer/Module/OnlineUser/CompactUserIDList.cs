using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ExWebServer.WebServer.Module.OnlineUser
{
    [DataContract]
    public class CompactUserIDList
    {
        /// <summary>
        /// UserID
        /// </summary>
        [DataMember]
        public int U { get; set; }

        public CompactUserIDList() { }
        public CompactUserIDList(int uid)
        {
            U = uid;
        }
    }
}
