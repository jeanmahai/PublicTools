using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utility.MSMQ
{
    public class MSMQDefine
    {
        /// <summary>
        /// 默认BUS名
        /// </summary>
        public const string DEFAULT_BUSNAME = "bus0";
        /// <summary>
        /// 默认的MSMQ配置文件相对于应用程序启动目录的配置目录路径
        /// </summary>
        public const string DEFAULT_FOLDER = "Configuration";
        /// <summary>
        /// MSMQ配置文件名称
        /// </summary>
        public const string CONFIG_FILE = "MSMQConfig.xml";
    }
}
