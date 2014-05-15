using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Soho.Utility
{
    [DataContract]
    public class MailEntity
    {
        private List<string> m_ToAddressList;
        private List<string> m_CcAddressList;
        private List<string> m_BccAddressList;
        private MailTemplateEntity m_TemplateInfo;

        [DataMember]
        public string FromAddress { get; set; }
        [DataMember]
        public string FromDisplay { get; set; }
        [DataMember]
        public List<string> ToAddressList
        {
            get
            {
                if (m_ToAddressList == null)
                {
                    m_ToAddressList = new List<string>();
                }
                return m_ToAddressList;
            }
            set
            {
                m_ToAddressList = value;
            }
        }
        [DataMember]
        public List<string> CcAddressList
        {
            get
            {
                if (m_CcAddressList == null)
                {
                    m_CcAddressList = new List<string>();
                }
                return m_CcAddressList;
            }
            set
            {
                m_CcAddressList = value;
            }
        }
        [DataMember]
        public List<string> BccAddressList
        {
            get
            {
                if (m_BccAddressList == null)
                {
                    m_BccAddressList = new List<string>();
                }
                return m_BccAddressList;
            }
            set
            {
                m_BccAddressList = value;
            }
        }
        [DataMember]
        public string Subject { get; set; }
        [DataMember]
        public string Body { get; set; }
        [DataMember]
        public Encoding Charset { get; set; }
        [DataMember]
        public bool IsBodyHtml { get; set; }
        [DataMember]
        public MailPriority Priority { get; set; }
        [DataMember]
        public MailTemplateEntity TemplateInfo
        {
            get
            {
                if (m_TemplateInfo == null)
                {
                    m_TemplateInfo = new MailTemplateEntity();
                }
                return m_TemplateInfo;
            }
            set
            {
                m_TemplateInfo = value;
            }
        }
    }

    [DataContract]
    public enum MailPriority
    {
        [EnumMember]
        Normal = 0,

        [EnumMember]
        Low = 1,

        [EnumMember]
        High = 2
    }

    [DataContract]
    public class MailTemplateEntity
    {
        private List<VariableTable> m_KeyTableVariables;
        private List<Variable> m_KeyValueVariables;

        [DataMember]
        public string TemplateID { get; set; }
        [DataMember]
        public List<VariableTable> TableVariables
        {
            get
            {
                if (m_KeyTableVariables == null)
                {
                    m_KeyTableVariables = new List<VariableTable>();
                }
                return m_KeyTableVariables;
            }
            set
            {
                m_KeyTableVariables = value;
            }
        }
        [DataMember]
        public List<Variable> Variables
        {
            get
            {
                if (m_KeyValueVariables == null)
                {
                    m_KeyValueVariables = new List<Variable>();
                }
                return m_KeyValueVariables;
            }
            set
            {
                m_KeyValueVariables = value;
            }
        }
    }

    [DataContract]
    public class VariableTable
    {
        public VariableTable()
        {

        }

        public VariableTable(string name, params string[] cList)
        {
            Name = name;
            Columns = cList;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string[] Columns { get; set; }

        [DataMember]
        public List<string[]> Rows { get; set; }
    }

    [DataContract]
    public class Variable
    {
        public Variable()
        {

        }

        public Variable(string name, string value)
        {
            Name = name;
            Value = value;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }
    }

    public static class VariableTableExtends
    {
        public static VariableTable SetColumns(this VariableTable vt, params string[] cList)
        {
            vt.Columns = cList;
            vt.Rows = null;
            return vt;
        }

        public static VariableTable InsertRow(this VariableTable vt, int rowIndex, params string[] data)
        {
            if (vt.Columns == null)
            {
                throw new ApplicationException("Please call 'SetColumns' method for VariableTable object first.");
            }
            if (data.Length > vt.Columns.Length)
            {
                throw new ApplicationException("There are " + vt.Columns.Length + " columns of VariableTable' schema, but " + data.Length + " columns in new row to add, more than VariableTable' schema.");
            }
            if (vt.Rows == null)
            {
                vt.Rows = new List<string[]>();
            }
            vt.Rows.Insert(rowIndex, data);
            return vt;
        }

        public static VariableTable AddRow(this VariableTable vt, params string[] data)
        {
            if (vt.Columns == null)
            {
                throw new ApplicationException("Please call 'SetColumns' method for VariableTable object first.");
            }
            if (data.Length > vt.Columns.Length)
            {
                throw new ApplicationException("There are " + vt.Columns.Length + " columns of VariableTable' schema, but " + data.Length + " columns in new row to add, more than VariableTable' schema.");
            }
            if (vt.Rows == null)
            {
                vt.Rows = new List<string[]>();
            }
            vt.Rows.Add(data);
            return vt;
        }

        public static VariableTable AddRows(this VariableTable vt, string[][] data)
        {
            if (vt.Columns == null)
            {
                throw new ApplicationException("Please call 'SetColumns' method for VariableTable object first.");
            }
            if (data == null || data.Length <= 0)
            {
                return vt;
            }
            foreach (var row in data)
            {
                if (row.Length > vt.Columns.Length)
                {
                    throw new ApplicationException("There are " + vt.Columns.Length + " columns of VariableTable' schema, but " + row.Length + " columns in new row to add, more than VariableTable' schema.");
                }
            }
            if (vt.Rows == null)
            {
                vt.Rows = new List<string[]>();
            }
            vt.Rows.AddRange(data);
            return vt;
        }

        public static VariableTable RemoveRow(this VariableTable vt, int rowIndex)
        {
            vt.Rows.RemoveAt(rowIndex);
            return vt;
        }

        public static string[] Get(this VariableTable vt, int rowIndex)
        {
            return vt.Rows[rowIndex];
        }

        public static string Get(this VariableTable vt, int rowIndex, int columnIndex)
        {
            string[] tmp = Get(vt, rowIndex);
            if (columnIndex < 0 || columnIndex >= tmp.Length)
            {
                return null;
            }
            return tmp[columnIndex];
        }

        public static string Get(this VariableTable vt, int rowIndex, string columnName, bool ignoreCase = false)
        {
            if (vt.Columns == null)
            {
                return null;
            }
            int cIndex = -1;
            for (int i = 0; i < vt.Columns.Length; i++)
            {
                if (string.Compare(columnName, vt.Columns[i], ignoreCase) == 0)
                {
                    cIndex = i;
                    break;
                }
            }
            if (cIndex == -1)
            {
                return null;
            }
            return Get(vt, rowIndex, cIndex);
        }
    }
}
