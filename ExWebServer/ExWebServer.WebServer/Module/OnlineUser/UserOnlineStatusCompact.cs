using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ExWebServer.WebServer.Module.OnlineUser
{
    [DataContract]
    public class UserOnlineStatusCompact
    {
        /// <summary>
        /// UserID
        /// </summary>
        [DataMember]
        public int U { get; set; }
        /// <summary>
        /// 0 = offline; 1 = online; 2 = invisible
        /// </summary>
        [DataMember]
        public int L { get; set; }
        /// <summary>
        /// GameID/ServerID
        /// </summary>
        [DataMember]
        public int P { get; set; }

        public UserOnlineStatusCompact() { }
        public UserOnlineStatusCompact(int uid, int onlineCode, int position)
        {
            U = uid;
            L = onlineCode;
            P = position;
        }

        public static UserOnlineStatusCompact GetInstanceFromOnlineStatus(UserOnlineStatus onlineUser)
        {
            if (onlineUser == null)
                throw new Exception("UserOnlineStatus cannot be null.");

            return new UserOnlineStatusCompact(onlineUser.UserID, onlineUser.OnlineCode, onlineUser.Position);
        }
    }
}
