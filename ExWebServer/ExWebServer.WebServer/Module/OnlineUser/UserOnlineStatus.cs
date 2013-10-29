using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ExWebServer.WebServer.Module.OnlineUser
{
    [DataContract]
    public class UserOnlineStatus
    {
        [DataMember]
        public int UserID { get; set; }
        /// <summary>
        /// 0 = offline; 1 = online; 2 = invisible
        /// </summary>
        [DataMember]
        public int OnlineCode { get; set; }
        /// <summary>
        /// GameID/ServerID
        /// </summary>
        [DataMember]
        public int Position { get; set; }

        /// <summary>
        /// true = 在线，不包括隐身
        /// </summary>
        public bool IsOnline
        {
            get
            {
                if (OnlineCode == 1)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// true = 隐身
        /// </summary>
        public bool IsInvisible
        {
            get
            {
                if (OnlineCode == 2)
                    return true;
                return false;
            }
        }

        public UserOnlineStatus() { }
        public UserOnlineStatus(int uid, int onlineCode, int position)
        {
            UserID = uid;
            OnlineCode = onlineCode;
            Position = position;
        }

        public static UserOnlineStatus GetInstanceFromCompactStatus(UserOnlineStatusCompact compactStatus)
        {
            if (compactStatus == null)
                throw new Exception("UserOnlineStatus cannot be null.");

            return new UserOnlineStatus(compactStatus.U, compactStatus.L, compactStatus.P);
        }
    }
}
