using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExWebServer.WebServer.Module.ConfigEntity
{
    public class ServerEntity
    {
        /// <summary>
        /// 允许方式
        /// </summary>
        public string RunWays { get; set; }
        /// <summary>
        /// 监听端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 信任的访问Host
        /// </summary>
        public List<string> HostList { get; set; }
        /// <summary>
        /// 下发消息的线程数
        /// </summary>
        public int SendThreads { get; set; }
        /// <summary>
        /// 允许的最大请求内容
        /// </summary>
        public long MaxPostLength { get; set; }
        /// <summary>
        /// 允许的请求头行数
        /// </summary>
        public int MaxHeaderLines { get; set; }
        /// <summary>
        /// 允许的最大请求队列
        /// </summary>
        public int MaxQueuedRequests { get; set; }
    }
}
