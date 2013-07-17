using System;

namespace WebSocket.WebSocket.Server
{
    [Serializable]
    public class WebSocketResponse
    {
        /// <summary>
        /// 客户端执行的脚本id
        /// </summary>
        public string Handler { get; set; }
        public string Data { get; set; }
        /// <summary>
        /// 服务端直接返回脚本执行
        /// </summary>
        public string RemoteHandler { get; set; }
    }
}
