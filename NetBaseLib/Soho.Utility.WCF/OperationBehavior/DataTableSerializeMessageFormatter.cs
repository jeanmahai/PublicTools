using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;
using System.Web;
using System.Runtime.Serialization;

namespace Soho.Utility.WCF
{
    public class DataTableSerializeMessageFormatter : IDispatchMessageFormatter
    {
        private OperationDescription m_Operation;
        private IDispatchMessageFormatter m_Formatter;
        private IConvert m_Convertor;

        public DataTableSerializeMessageFormatter(OperationDescription operation, IDispatchMessageFormatter formatter)
            : this(operation, formatter, null)
        {

        }

        public DataTableSerializeMessageFormatter(OperationDescription operation, IDispatchMessageFormatter formatter, IConvert convertor)
        {
            m_Operation = operation;
            m_Formatter = formatter;
            m_Convertor = convertor;
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            m_Formatter.DeserializeRequest(message, parameters);
        }

        private string ToXml(DataTable dt)
        {
            if (dt == null || dt.Rows == null || dt.Rows.Count <= 0)
            {
                return string.Empty;
            }
            StringBuilder jsonBuilder = new StringBuilder();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                jsonBuilder.Append("<item type=\"Object\">");
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    if (dt.Columns[j].ColumnName.EndsWith("_ECCentral_Auto_Removed_820319"))
                    {
                        continue;
                    }
                    Type realType;
                    object v;
                    if (m_Convertor != null)
                    {
                        v = m_Convertor.Convert(dt.Rows[i][j], dt.Columns[j].DataType, i, j);
                        realType = v.GetType();
                    }
                    else
                    {
                        v = dt.Rows[i][j];
                        realType = dt.Columns[j].DataType;
                    }
                    string realValue;
                    string typeStr;
                    if (realType.IsEnum)
                    {
                        realValue = (v == null || v == DBNull.Value) ? string.Empty : ((int)v).ToString();
                        typeStr = realType.AssemblyQualifiedName;
                    }
                    else
                    {
                        TypeCode code = Type.GetTypeCode(realType);
                        switch (code)
                        {
                            case TypeCode.Boolean:
                                realValue = v.ToString().ToLower();
                                break;
                            case TypeCode.DBNull:
                            case TypeCode.Empty:
                                realValue = string.Empty;
                                break;
                            default:
                                realValue = v.ToString();
                                break;
                        }
                        if (code == TypeCode.Object)
                        {
                            code = TypeCode.String;
                        }
                        typeStr = code.ToString();
                    }
                    if (realType == typeof(string))
                    {
                        realValue = HttpUtility.HtmlEncode(realValue);
                    }
                    jsonBuilder.AppendFormat("<{0} type=\"{1}\"><![CDATA[{2}]]></{0}>", dt.Columns[j].ColumnName,
                        typeStr, realValue);
                }
                jsonBuilder.Append("</item>");
            }
            return jsonBuilder.ToString();
        }

        private Message BuildMessage(string jsonStr, MessageVersion messageVersion, object[] parameters, object result)
        {
            byte[] body = Encoding.UTF8.GetBytes(jsonStr);
            Message replyMessage = Message.CreateMessage(messageVersion, m_Operation.Messages[1].Action, new RawBodyWriter(body));
            replyMessage.Properties.Add(WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(WebContentFormat.Raw));
            HttpResponseMessageProperty respProp = new HttpResponseMessageProperty();
            respProp.Headers[HttpResponseHeader.ContentType] = "application/json";
            replyMessage.Properties.Add(HttpResponseMessageProperty.Name, respProp);
            return replyMessage;
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            if (result != null && result is DataTable)
            {
                return BuildMessage(string.Format("<root type=\"Enumerable\">{0}</root>", ToXml((DataTable)result)), messageVersion,
                    parameters, result);
            }
            else if (result != null && result is QueryResult)
            {
                QueryResult rst = (QueryResult)result;
                string content = string.Format("<root type=\"Object\"><TotalCount type=\"Int32\">{0}</TotalCount><Rows type=\"Enumerable\">{1}</Rows></root>",
                    rst.TotalCount, ToXml(rst.Data));
                return BuildMessage(content, messageVersion, parameters, result);
            }
            else if (result != null && result is IEnumerable<QueryResult>)
            {
                IEnumerable<QueryResult> rstList = (IEnumerable<QueryResult>)result;
                StringBuilder sb = new StringBuilder();
                sb.Append("<root type=\"Enumerable\">");
                foreach (QueryResult rst in rstList)
                {
                    sb.AppendFormat("<item type=\"Object\"><TotalCount type=\"Int32\">{0}</TotalCount><Rows type=\"Enumerable\">{1}</Rows></item>",
                        rst.TotalCount, ToXml(rst.Data));
                }
                sb.Append("</root>");
                return BuildMessage(sb.ToString(), messageVersion, parameters, result);
            }
            else
            {
                return m_Formatter.SerializeReply(messageVersion, parameters, result);
            }
        }

        private class RawBodyWriter : BodyWriter
        {
            private byte[] m_Content;
            public RawBodyWriter(byte[] content)
                : base(true)
            {
                this.m_Content = content;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                writer.WriteStartElement("Binary");
                writer.WriteBase64(m_Content, 0, m_Content.Length);
                writer.WriteEndElement();
            }
        }
    }

    public interface IConvert
    {
        object Convert(object data, Type dataType, int rowIndex, int colsIndex);
    }

    [DataContract]
    public class QueryResult
    {
        [DataMember]
        public DataTable Data
        {
            get;
            set;
        }

        [DataMember]
        public int TotalCount
        {
            get;
            set;
        }
    }

    [DataContract]
    public class QueryResultList : List<QueryResult>
    {

    }
}
