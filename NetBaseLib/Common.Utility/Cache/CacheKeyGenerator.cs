using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Common.Utility
{
    internal static class CacheKeyGenerator
    {
        private static readonly Type s_IdentityType = Type.GetType("ECCentral.BizEntity.IIdentity, ECCentral.BizEntity");

        public static string GenerateKeyName(MethodBase method, object[] arguments, List<ParamCacheKey> keys)
        {
            string methodName;
            if (method.DeclaringType != null)
            {
                methodName = method.DeclaringType.FullName + "." + method.Name;
            }
            else
            {
                methodName = method.Name;
            }

            if ((keys == null || keys.Count <= 0) && (arguments == null || arguments.Length <= 0))
            {
                return methodName;
            }

            if (keys == null || keys.Count <= 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(methodName);
                foreach (object argument in arguments)
                {
                    sb.Append("-");
                    if (argument != null)
                    {
                        sb.Append(GetObjectValueAsKey(argument));
                    }
                }
                return sb.ToString();
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(methodName);
                foreach (ParamCacheKey key in keys)
                {
                    sb.Append("-");
                    if (key.ParamIndex < 0 || key.ParamIndex >= arguments.Length)
                    {
                        continue;
                    }
                    object argument = arguments[key.ParamIndex];
                    if (argument != null)
                    {
                        object v = (key.ParamProperty == null || key.ParamProperty.Length <= 0) ? argument
                            : DataMapper.GetFieldValueAllowNull(argument, key.ParamProperty);
                        if (v != null)
                        {
                            sb.Append(GetObjectValueAsKey(v));
                        }
                    }
                }
                return sb.ToString();
            }
        }

        public static string GenerateGroupName(MethodBase method)
        {
            string methodName;
            if (method.DeclaringType != null)
            {
                methodName = method.DeclaringType.FullName + "." + method.Name;
            }
            else
            {
                methodName = method.Name;
            }
            return "all-" + methodName;
        }

        private static string GetObjectValueAsKey(object obj)
        {
            Type type = obj.GetType();
            if (s_IdentityType.IsAssignableFrom(type))
            {
                return Invoker.PropertyGet(s_IdentityType, obj, "SysNo", false, true).ToString();
            }
            else if (CanAsKey(type))
            {
                return obj.ToString();
            }
            else
            {
                return SerializationUtility.BinarySerialize(obj);
            }
        }

        private static bool CanAsKey(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                            && type.GetGenericArguments() != null
                            && type.GetGenericArguments().Length == 1)
            {
                type = type.GetGenericArguments()[0];
            }
            TypeCode code = Type.GetTypeCode(type);
            if (code == TypeCode.DBNull || code == TypeCode.Empty || code == TypeCode.Object)
            {
                return false;
            }
            return true;
        }
    }
}
