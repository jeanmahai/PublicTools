using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace NHibernate.Utility
{
    public class SqlManager
    {
        private static Dictionary<string, string> Sqls = new Dictionary<string, string>();
        private static string RuntimeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static string SqlFolder = ConfigurationManager.AppSettings["NHibernate.Configs.Sql"];

        static SqlManager()
        {
            var directory = new DirectoryInfo(Path.Combine(RuntimeDirectory
                , SqlFolder));
            var files = directory.GetFiles("*.xml");
            foreach (var fileInfo in files)
            {
                var xml = new XmlDocument();
                xml.Load(fileInfo.FullName);
                XmlNodeList sqlsNode = xml.SelectNodes("/Sqls/*");
                if (sqlsNode != null)
                {
                    foreach (XmlNode node in sqlsNode)
                    {
                        string key = "", val = "";
                        if (node.Attributes != null)
                        {
                            key = node.Attributes["name"].Value;
                        }
                        val = node.InnerText;
                        Sqls.Add(key, val);
                    }
                }
            }
        }

        public static string GetSqlText(string name)
        {
            return Sqls.SingleOrDefault(p=>p.Key==name).Value;
        }

        public static Dictionary<string, string> GetAllText()
        {
            return Sqls;
        }
    }
}
