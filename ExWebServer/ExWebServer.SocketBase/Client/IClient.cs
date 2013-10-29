namespace ExWebServer.SocketBase.Client
{
    public interface IClient
    {
        /// <summary>
        /// 客户端ＩＤ
        /// </summary>
        int nSessionID { get; set; }
        /// <summary>
        /// 用户ＩＤ
        /// </summary>
        int nUID { get; set; }
        /// <summary>
        /// 信任连接
        /// </summary>
        bool IsTrustIP { get; set; }
        /// <summary>
        /// 经过用户身份验证,此时nUID真实有效
        /// </summary>
        bool IsAuthedUser { get; set; }
    }
}
