using System.Runtime.Serialization;

using ExWebServer.SocketBase.Client;

namespace ExWebServer.WebServer.Comet
{
    [DataContract]
    public class CometCommand
    {
        public const int PERMISSION_ANONYMOUS = 1;
        public const int PERMISSION_TRUSTIP = 2;
        [DataMember]
        /// <summary>
        /// 命令
        /// </summary>
        public CometCommandID CommandID { get; set; }
        [DataMember]
        /// <summary>
        /// 访问权限：bit0 = 匿名/用户；bit1 = trustIP
        /// </summary>
        public int Permissions { get; set; }
        [DataMember]
        public bool RequireKeepAlive { get; set; }

        public bool AllowAnonymous { get { return (Permissions & PERMISSION_ANONYMOUS) == PERMISSION_ANONYMOUS ? true : false; } }
        public bool AllowUntrustIP { get { return (Permissions & PERMISSION_TRUSTIP) == PERMISSION_TRUSTIP ? false : true; } }


        public CometCommand()
        {
            CommandID = CometCommandID.NA;
            Permissions = 1;
            RequireKeepAlive = false;
        }

        public CometCommand(CometCommandID commandID)
        {
            CommandID = commandID;
            Permissions = 1;
            RequireKeepAlive = false;
        }

        public static CometCommand ParseFromString(string command)
        {
            if (string.IsNullOrEmpty(command))
                return new CometCommand();

            command = command.Trim().ToLower();

            if (command.CompareTo("default") == 0)
                return new CometCommand(CometCommandID.Default);
            if (command.CompareTo("validationuserkey") == 0)
                return new CometCommand(CometCommandID.ValidationUserKey);

            return new CometCommand();
        }

        public static CometCommand ParseFromRequest(HttpLib.HttpRequest request)
        {
            if (request == null || request.PathUri.Segments.Length < 2)
                return new CometCommand();

            string command = request.PathUri.Segments[1];

            return ParseFromString(command);
        }

        /// <summary>
        /// 检验该客户端是否被允许访问此命令
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool IsClientAllowed(IClient client)
        {
            if (client == null)
                return false;

            //  如果要求验证用户，而客户端未匿名用户则ｆａｌｓｅ
            if (!AllowAnonymous && !client.IsAuthedUser)
                return false;

            if (!AllowUntrustIP && !client.IsTrustIP)
                return false;

            return true;
        }

        public override string ToString()
        {
            if (CommandID == CometCommandID.NA)
                return "";
            return CommandID.ToString();
        }
    }
}
