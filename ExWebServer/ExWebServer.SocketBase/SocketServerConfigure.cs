﻿using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;

using Common.Utility;

namespace ExWebServer.SocketBase
{
    public class SocketServerConfigure : ISocketServerConfigure
    {
        const int DEFAULT_PORT = 80;
        const int DEFAULT_BACKLOG = 200;
        const int DEFAULT_SENDTHREAD = 5;

        public int Port { get; set; }
        public int Backlog { get; set; }
        public int SendThreads { get; set; }
        public string ServerName { get; set; }
        public HashSet<long> TrustRemoteIP { get; set; }

        protected string _BasePath = "";
        //protected SubConfigure _ConfigFile = null;

        /// <summary>
        /// 打开xml格式的config文件
        /// </summary>
        /// <param name="configFileFullPath"></param>
        /// <returns></returns>
        private XmlDocument OpenConfigureFile(string fullPathName)
        {
            XmlDocument xmlDoc = null;
            try
            {
                if (File.Exists(fullPathName))
                {
                    xmlDoc = new XmlDocument();
                    xmlDoc.Load(fullPathName);
                }
            }
            catch
            {
                xmlDoc = null;
            }
            return xmlDoc;
        }

        public SocketServerConfigure()
        {
            //LoadConfig("");
        }

        protected string MakeBasePath()
        {
            if (string.IsNullOrEmpty(_BasePath))
                _BasePath = System.Environment.CurrentDirectory;

            return _BasePath;
        }

        protected int ParseTrustIPString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            if (TrustRemoteIP != null)
            {
                TrustRemoteIP.Clear();
                TrustRemoteIP = null;
            }

            try
            {
                string[] ips = value.Split(new char[] { ';', ',' });

                if (ips != null && ips.Length > 0)
                {
                    TrustRemoteIP = new HashSet<long>();
                    foreach (string ipAddr in ips)
                    {
                        long ip = IPUtility.GetIPIntValue(ipAddr);
                        TrustRemoteIP.Add(ip);
                    }
                }
            }
            catch { }

            return TrustRemoteIP != null ? TrustRemoteIP.Count : 0;
        }
    }
}
