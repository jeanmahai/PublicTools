using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Data;

namespace Soho.Utility.Json
{
    public static class JsonHelper
    {
        public static string _strErrMsg;
        public static string ObjToJson(this IJson source)
        {
            return ObjToJson(source, source.GetType());
        }

        public static string TbObjToJson(this IJson[] source)
        {
            return TbObjToJson(source, source.GetType());
        }

        public static string ObjToJson<T>(T obj)
        {
            try
            {
                DataContractJsonSerializer json = new DataContractJsonSerializer(obj.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    json.WriteObject(stream, obj);
                    string szJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                    return szJson;
                }
            }
            catch (Exception ex) { _strErrMsg = ex.Message; return ""; }
            finally { }
        }

        public static string ObjToJson(this IJson source, Type type)
        {
            DataContractJsonSerializer serilializer = null;
            Stream stream = null;
            StreamReader reader = null;

            try
            {
                serilializer = new DataContractJsonSerializer(type);
                stream = new MemoryStream();
                serilializer.WriteObject(stream, source);
                stream.Flush();
                stream.Position = 0;
                reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                _strErrMsg = ex.Message;
                return "";
            }
            finally
            {
                if (reader != null)
                    try { reader.Close(); }
                    catch { }
                    finally { reader = null; }
                if (stream != null)
                    try { stream.Close(); }
                    catch { }
                    finally { stream = null; }
                serilializer = null;
            }
        }

        public static T JsonToObj<T>(this string str)
        {
            using (MemoryStream ms = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(str)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(ms);
            }
        }

        public static string TbObjToJson(this IJson[] source, Type type)
        {
            DataContractJsonSerializer serilializer = new DataContractJsonSerializer(type);
            System.Text.ASCIIEncoding converter = new System.Text.ASCIIEncoding();

            using (Stream stream = new MemoryStream())
            {
                serilializer.WriteObject(stream, source);
                stream.Flush();
                stream.Position = 0;
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        public static string DataTableToJson(DataTable dt)
        {
            StringBuilder jsonBuilder = null;
            if (dt == null)
                return "";
            try
            {
                jsonBuilder = new StringBuilder();
                jsonBuilder.Append("{\"");
                jsonBuilder.Append(dt.TableName.ToString());
                jsonBuilder.Append("\":[");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    jsonBuilder.Append("{");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        jsonBuilder.Append("\"");
                        jsonBuilder.Append(dt.Columns[j].ColumnName);
                        jsonBuilder.Append("\":\"");
                        jsonBuilder.Append(dt.Rows[i][j].ToString());
                        jsonBuilder.Append("\",");
                    }
                    jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                    jsonBuilder.Append("},");
                }
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("]");
                jsonBuilder.Append("}");
            }
            catch { }
            finally
            {
                jsonBuilder = null;
            }



            return jsonBuilder != null ? jsonBuilder.ToString() : "";
        }

        // 通过JSON序列化深表Copy对象
        public static T DeepClone<T>(this IJson Source) where T : IJson
        {
            string jsonString = Source.ObjToJson();
            return jsonString.JsonToObj<T>();
        }

        public static T DeepClone<T>(T obj)
        {
            string jsonString = ObjToJson<T>(obj);
            return JsonToObj<T>(jsonString);
        }
    }
}
