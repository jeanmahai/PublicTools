using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Configuration;

namespace Common.Utility
{
    [XmlRoot("typeVersionMaps")]
    public class TypeVersionConfig
    {
        [XmlIgnore]
        public const string VERSION_DEFAULT = "1.0";
        private static readonly string Config_File_Path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Configuration\\TypeVersion.config");

        private static TypeVersionConfig s_Instance;
        private static object s_SyncObj = new object();

        private static Dictionary<string, string> s_VersionMap = new Dictionary<string, string>();

        private static void LoadConfig()
        {
            string tmpPath = ConfigurationManager.AppSettings["TypeVersionConfigFilePath"];
            if (tmpPath == null || tmpPath.Trim().Length <= 0)
            {
                tmpPath = Config_File_Path;
            }
            else if (!tmpPath.Contains(':')) // 说明配得相对路径
            {
                tmpPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, tmpPath);
            }
            if (File.Exists(tmpPath))
            {
                s_Instance = LoadFromFile<TypeVersionConfig>(tmpPath);
            }
            else
            {
                s_Instance = new TypeVersionConfig() { AssemblyFolder = string.Empty, GlobalDefaultVersion = string.Empty, Maps = new TypeVersionMap[0] };
            }
            if (s_Instance != null && s_Instance.Maps != null && s_Instance.Maps.Length > 0)
            {
                foreach (var map in s_Instance.Maps)
                {
                    if (map.Type == null || map.Type.Trim().Length <= 0)
                    {
                        throw new ArgumentException("AssemblyQualifiedName of type can't be blank in type version config file.", "typeAssemblyQualifiedName");
                    }
                    if (map.Version == null || map.Version.Trim().Length <= 0)
                    {
                        throw new ArgumentException("Version can't be blank for type '" + map.Type.Trim() + "' in type version config file.", "typeAssemblyQualifiedName");
                    }
                    string[] tmpArray = map.Type.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tmpArray == null || tmpArray.Length < 2)
                    {
                        throw new ArgumentException("Error format for assemblyQualifiedName ['" + map.Type.Trim() + "'] of type in type version config file.", "typeAssemblyQualifiedName");
                    }
                    string key = tmpArray[0].Trim() + "," + tmpArray[1].Trim();
                    if (s_VersionMap.ContainsKey(key))
                    {
                        throw new ArgumentException("Duplicated assemblyQualifiedName of type '" + key + "' in type version config file.", "typeAssemblyQualifiedName");
                    }
                    s_VersionMap.Add(key, map.Version);
                }
            }
        }

        internal static TypeVersionConfig Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    lock (s_SyncObj)
                    {
                        if (s_Instance == null)
                        {
                            LoadConfig();
                        }
                    }
                }
                return s_Instance;
            }
        }

        internal static bool ExistVersionMap(Type type)
        {
            return ExistVersionMap(type.AssemblyQualifiedName);
        }

        internal static bool ExistVersionMap(string typeAssemblyQualifiedName)
        {
            string[] tmpArray = typeAssemblyQualifiedName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return s_VersionMap.ContainsKey(tmpArray[0].Trim() + "," + tmpArray[1].Trim());
        }

        internal static string GetVersion(Type type)
        {
            return GetVersion(type.AssemblyQualifiedName);
        }

        internal static string GetVersion(string typeAssemblyQualifiedName)
        {
            string[] tmpArray = typeAssemblyQualifiedName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return s_VersionMap[tmpArray[0].Trim() + "," + tmpArray[1].Trim()];
        }

        private static T LoadFromFile<T>(string filePath) where T : class
        {
            FileStream fs = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return (T)serializer.Deserialize(fs);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        [XmlElement("map")]
        public TypeVersionMap[] Maps
        {
            get;
            set;
        }

        [XmlAttribute("globalDefaultVersion")]
        public string GlobalDefaultVersion
        {
            get;
            set;
        }

        [XmlAttribute("assemblyFolder")]
        public string AssemblyFolder
        {
            get;
            set;
        }
    }
}
