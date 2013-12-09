using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utility.WCF
{
    public interface IExceptionHandle
    {
        void Handle(Exception error, object[] methodArguments);
    }

    internal class ExceptionHandler : IExceptionHandle
    {
        public void Handle(Exception error, object[] methodArguments)
        {
            List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();
            list.Add(new KeyValuePair<string, object>("Method Arguments Type", GetMethodArgumentType(methodArguments)));
            list.Add(new KeyValuePair<string, object>("Method Arguments Value", GetMethodArgumentValue(methodArguments)));
            Logger.WriteLog(error.ToString(), "ExceptionLog", null, list);
        }

        private static string GetMethodArgumentType(object[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
            {
                return "N/A";
            }
            string result = string.Empty;

            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] == null)
                {
                    result += "NULL";
                }
                else
                {
                    result += arguments[i].GetType().ToString();
                }

                if (i != arguments.Length - 1)
                {
                    result += ", ";
                }
            }
            return result;
        }

        private static string GetMethodArgumentValue(object[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
            {
                return "N/A";
            }
            string result = string.Empty;
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] != null)
                {
                    if (arguments[i] is string)
                    {
                        result += Convert.ToString(arguments[i]);
                    }
                    else if (arguments[i].GetType().IsPrimitive)
                    {
                        result += arguments[i].ToString();
                    }
                    else
                    {
                        try
                        {
                            result += SerializationUtility.XmlSerialize(arguments[i]);
                        }
                        catch
                        {
                            result += "[Xml Serialize Error]";
                        }
                    }
                    if (i != arguments.Length - 1)
                    {
                        result += ", ";
                    }
                }
            }
            return result;
        }
    }
}
