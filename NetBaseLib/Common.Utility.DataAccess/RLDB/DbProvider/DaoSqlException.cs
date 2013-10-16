using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Runtime.Serialization;

namespace Common.Utility.DataAccess.Database.DbProvider
{
    [Serializable]
    public class DataAccessException : ApplicationException
    {
        public DataAccessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public DataAccessException(Exception innerException, string connectionStr, string sqlText,
            params DbParameter[] commandParameters)
            : base(BuilderMessage(innerException.Message, connectionStr, sqlText, commandParameters), innerException)
        {
        }

        private static string BuilderMessage(string errorMsg, string connectionStr, string sqlText,
            params DbParameter[] commandParameters)
        {
            StringBuilder msg = new StringBuilder();
            msg.AppendFormat("{0}\r\n", errorMsg);
            msg.AppendFormat("<<Connection String>> : {0}\r\n", connectionStr);
            msg.AppendFormat("<<SQL Script>> : {0}\r\n", sqlText);
            if (commandParameters != null && commandParameters.Length > 0)
            {
                msg.Append("<<SQL Parameter(s)>> :\r\n");
                foreach (DbParameter param in commandParameters)
                {
                    msg.AppendFormat("{0} [{1}] : {2} [{3}]\r\n", param.ParameterName, param.DbType, param.Value, param.Value == null ? "" : param.Value.GetType().ToString());
                }
            }
            return msg.ToString();
        }
    }
}
