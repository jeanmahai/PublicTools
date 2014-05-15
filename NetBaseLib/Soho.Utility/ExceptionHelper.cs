using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soho.Utility
{
    internal static class ExceptionHelper
    {
        private const string DEFAULT_CATEGORY = "ExceptionLog";

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

        public static string HandleException(Exception ex)
        {
            return HandleException(ex, null, DEFAULT_CATEGORY, null);
        }

        public static string HandleException(Exception ex, object[] methodArguments)
        {
            return HandleException(ex, null, DEFAULT_CATEGORY, methodArguments);
        }

        public static string HandleException(Exception ex, string message, object[] methodArguments)
        {
            return HandleException(ex, message, DEFAULT_CATEGORY, methodArguments);
        }

        public static string HandleException(Exception ex, string message, string category, object[] methodArguments)
        {
            return HandleException(ex, message, category, methodArguments, null);
        }

        public static string HandleException(Exception ex, string message, string category, object[] methodArguments, string referenceKey)
        {
            List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();
            if (message != null)
            {
                list.Add(new KeyValuePair<string, object>("Error Message", message));
            }
            list.Add(new KeyValuePair<string, object>("Method Arguments Type", GetMethodArgumentType(methodArguments)));
            list.Add(new KeyValuePair<string, object>("Method Arguments Value", GetMethodArgumentValue(methodArguments)));
            return HandleException(ex, category, list, referenceKey);
        }

        public static string HandleException(Exception ex, string category, List<KeyValuePair<string, object>> extendedProperties)
        {
            return HandleException(ex, category, extendedProperties, null);
        }

        public static string HandleException(Exception ex, string category, List<KeyValuePair<string, object>> extendedProperties, string referenceKey)
        {
            if (string.IsNullOrEmpty(category))
            {
                category = DEFAULT_CATEGORY;
            }
            return Logger.WriteLog(ex.ToString(), category, referenceKey, extendedProperties);
        }
    }
}
