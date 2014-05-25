using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace Soho.Utility
{
    public static class SerializationUtility
    {
        public static T LoadFromXml<T>(string filePath)
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

        public static void SaveToXml(string filePath, object data)
        {
            FileStream fs = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(data.GetType());
                fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                serializer.Serialize(fs, data);
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

        #region Xml序列化

        public static string ToXmlString(this object serialObject, bool removeDataRootXmlNode = false)
        {
            return XmlSerialize(serialObject, removeDataRootXmlNode);
        }

        public static string XmlSerialize(object serialObject, bool removeDataRootXmlNode = false)
        {
            StringBuilder sb = new StringBuilder();
            XmlSerializer ser = new XmlSerializer(serialObject.GetType());
            using (TextWriter writer = new StringWriter(sb))
            {
                ser.Serialize(writer, serialObject);
                string xmlData = writer.ToString();
                if (removeDataRootXmlNode)
                {
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.LoadXml(xmlData);
                    xmlData = doc.LastChild.InnerXml;
                }
                return xmlData;
            }
        }

        public static object XmlDeserialize(string str, Type type)
        {
            XmlSerializer mySerializer = new XmlSerializer(type);
            using (TextReader reader = new StringReader(str))
            {
                return mySerializer.Deserialize(reader);
            }
        }

        public static T XmlDeserialize<T>(string str)
        {
            return (T)XmlDeserialize(str, typeof(T));
        }

        #endregion Xml序列化

        #region 二进制序列化

        public static string ToBinaryBase64String(this object serialObject)
        {
            return BinarySerialize(serialObject);
        }

        public static string BinarySerialize(object serialObject)
        {
            if (serialObject == null)
            {
                return string.Empty;
            }
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, serialObject);
                ms.Position = 0;
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static object BinaryDeserialize(string str)
        {
            if (str == null || str.Trim().Length <= 0)
            {
                return null;
            }
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(str)))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(ms);
            }
        }

        public static T BinaryDeserialize<T>(string str)
        {
            return (T)BinaryDeserialize(str);
        }

        #endregion 二进制序列化

        public static string ToJsonStringCommon(this object serialObject)
        {
            return JsonSerializeCommon(serialObject);
        }

        public static string JsonSerializeCommon(object obj)
        {
            if (obj.GetType().Name.StartsWith("<>f__AnonymousType"))
            {
                return SerializationUtility.JsonSerialize2(obj);
            }
            return SerializationUtility.JsonSerialize(obj);
        }

        #region Json序列化 - DataContractJsonSerializer，不支持对匿名类的序列化

        /// <summary>
        /// 通过DataContractJsonSerializer，不支持对匿名类的序列化
        /// </summary>
        /// <param name="serialObject"></param>
        /// <returns></returns>
        public static string ToJsonString(this object serialObject)
        {
            return JsonSerialize(serialObject);
        }

        /// <summary>
        /// 通过DataContractJsonSerializer，不支持对匿名类的序列化
        /// </summary>
        /// <param name="serialObject"></param>
        /// <returns></returns>
        public static string JsonSerialize(object serialObject)
        {
            if (serialObject == null)
            {
                return string.Empty;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(serialObject.GetType());
                serializer.WriteObject(stream, serialObject);
                byte[] s = stream.ToArray();
                return Encoding.UTF8.GetString(s, 0, s.Length);
            }
        }

        /// <summary>
        /// 通过DataContractJsonSerializer
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object JsonDeserialize(string str, Type type)
        {
            if (str == null || str.Trim().Length <= 0)
            {
                return null;
            }
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(type);
                return serializer.ReadObject(stream);
            }
        }

        /// <summary>
        /// 通过DataContractJsonSerializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T JsonDeserialize<T>(string str)
        {
            return (T)JsonDeserialize(str, typeof(T));
        }

        #endregion Json序列化

        #region Json序列化 - JavaScriptSerializer，可支持对匿名类的序列化

        /// <summary>
        /// 通过JavaScriptSerializer，可支持对匿名类的序列化
        /// </summary>
        /// <param name="serialObject"></param>
        /// <returns></returns>
        public static string ToJsonString2(this object serialObject)
        {
            return JsonSerialize2(serialObject);
        }

        /// <summary>
        /// 通过JavaScriptSerializer，可支持对匿名类的序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string JsonSerialize2(object obj)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(obj);
        }

        /// <summary>
        /// 通过JavaScriptSerializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static T JsonDeserialize2<T>(string input)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(input);
        }

        #endregion

        public static T DeepClone<T>(T t)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, t);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }

        #region
        /// <summary>  
        /// JSON序列化  
        /// </summary>  
        public static string JsonSerialize3<T>(T t)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string jsonString = serializer.Serialize(t);
            return jsonString;
        }
        /// <summary>  
        /// JSON反序列化  
        /// </summary>  
        public static T JsonDeserialize3<T>(string jsonString)
        {
            jsonString = Regex.Replace(jsonString, @"\\/Date\((\d+)\)\\/", match =>
            {
                DateTime dt = new DateTime(1970, 1, 1);
                dt = dt.AddMilliseconds(long.Parse(match.Groups[1].Value));
                dt = dt.ToLocalTime();
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            });

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(jsonString);
        }
        #endregion
    }
}