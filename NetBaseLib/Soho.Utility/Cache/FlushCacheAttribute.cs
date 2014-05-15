using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using PostSharp.Aspects;
using PostSharp.Extensibility;

namespace Soho.Utility
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class FlushCacheAttribute : OnMethodBoundaryAspect
    {
        private Type m_Type;

        private string m_MethodName;

        private List<ParamCacheKey> m_Keys;

        private string m_CacheName;

        private string m_CacheGroupName;

        #region Overload Constructor

        public FlushCacheAttribute(string typeFullName, string methodName)
            : this(typeFullName, methodName, new string[0])
        {

        }

        public FlushCacheAttribute(Type type, string methodName)
            : this(type, methodName, new string[0])
        {

        }

        public FlushCacheAttribute(string typeFullName, string methodName, string key)
            : this(typeFullName, methodName, new string[1] { key })
        {

        }

        public FlushCacheAttribute(Type type, string methodName, string key)
            : this(type, methodName, new string[1] { key })
        {

        }

        public FlushCacheAttribute(string typeFullName, string methodName, string[] keys)
            : this(Type.GetType(typeFullName), methodName, keys)
        {

        }

        #endregion

        public FlushCacheAttribute(Type type, string methodName, string[] keys)
        {
            List<ParamCacheKey> keyList = new List<ParamCacheKey>(keys.Length);
            List<string> duplicatedCheck = new List<string>();
            foreach (string key in keys)
            {
                string tmp = key.Trim();
                if (duplicatedCheck.Contains(tmp))
                {
                    throw new ArgumentException("Duplicated key '" + tmp + "' in argument <string[] keys>.", "keys");
                }
                duplicatedCheck.Add(tmp);
                string[] array = key.Split('.');
                string[] propertyArray = new string[array.Length - 1];
                for (int i = 1; i < array.Length; i++)
                {
                    propertyArray[i - 1] = array[i];
                }
                keyList.Add(new ParamCacheKey { ParamName = array[0], ParamProperty = propertyArray });
            }
            m_Keys = keyList;
            m_Type = type;
            m_MethodName = methodName;
        }

        public override bool CompileTimeValidate(MethodBase method)
        {
            // Don't apply to constructors.
            //if (method is ConstructorInfo)
            //{
            //    Message.Write(SeverityType.Error, "ER0001", "Cannot cache constructors.");
            //    return false;
            //}
            if (m_Type == null)
            {
                Message.Write(SeverityType.Error, "ER0008", "The type in FlushCacheAttribute is null with the method : '" + method.DeclaringType.FullName + "." + method.Name + "'.");
                return false;
            }
            MethodInfo cacheMethod = m_Type.GetMethod(m_MethodName);
            if (cacheMethod == null)
            {
                Message.Write(SeverityType.Error, "ER0009", "The method '" + m_MethodName + "' of type '" + m_Type.FullName + "' in FlushCacheAttribute is not found with the method : '" + method.DeclaringType.FullName + "." + method.Name + "'.");
                return false;
            }
            object[] attrArray = cacheMethod.GetCustomAttributes(typeof(CachingAttribute), false);
            if (attrArray == null || attrArray.Length <= 0 || attrArray[0] == null || !(attrArray[0] is CachingAttribute))
            {
                Message.Write(SeverityType.Error, "ER0010", "The method '" + m_MethodName + "' of type '" + m_Type.FullName + "' in FlushCacheAttribute is not marked by CachingAttribute with the method : '" + method.DeclaringType.FullName + "." + method.Name + "'.");
                return false;
            }
            CachingAttribute cache = (CachingAttribute)attrArray[0];
            m_CacheName = cache.CacheName;
            m_CacheGroupName = cache.GetGroupName(cacheMethod);
            if (m_Keys != null && m_Keys.Count > 0)
            {
                MethodInfo methodInfo = (MethodInfo)method;
                ParameterInfo[] parameters = method.GetParameters();
                List<ParameterInfo> parameterList = new List<ParameterInfo>(parameters);
                foreach (ParamCacheKey key in m_Keys)
                {
                    ParameterInfo param = parameterList.Find(p => p.Name == key.ParamName);
                    if (param == null)
                    {
                        Message.Write(SeverityType.Error, "ER0006", "There is not a parameter named '" + key.ParamName + "' with the method '" + method.DeclaringType.FullName + "." + method.Name + "'.");
                        return false;
                    }
                    if (key.ParamProperty != null && key.ParamProperty.Length > 0)
                    {
                        Type tmpType = param.ParameterType;
                        for (int i = 0; i < key.ParamProperty.Length; i++)
                        {
                            if (!Invoker.ExistPropertyGet(tmpType, key.ParamProperty[i], false))
                            {
                                Message.Write(SeverityType.Error, "ER0007", "Error with the Keys : there is no property '" + key.ParamProperty[i] + "' can be read in type '" + tmpType.AssemblyQualifiedName + "'. Method : '" + method.DeclaringType.FullName + "." + method.Name + "'");
                                return false;
                            }
                            if (i < key.ParamProperty.Length - 1)
                            {
                                tmpType = Invoker.GetPropertyType(tmpType, key.ParamProperty[i], false, true);
                            }
                        }
                    }
                }
            }
            return true;
        }

        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            if (m_Keys != null && m_Keys.Count > 0)
            {
                List<ParameterInfo> parameterList = new List<ParameterInfo>(method.GetParameters());
                foreach (ParamCacheKey key in m_Keys)
                {
                    key.ParamIndex = parameterList.FindIndex(p => p.Name == key.ParamName);
                }
            }
        }

        public override void OnExit(MethodExecutionArgs eventArgs)
        {
            if ((m_Keys != null && m_Keys.Count > 0))
            {
                MethodInfo cacheMethod = m_Type.GetMethod(m_MethodName);
                string key;
                if (m_CacheGroupName == null) // 缓存的方法没有指定Key的，整个方法就一个Key
                {
                    key = CacheKeyGenerator.GenerateKeyName(cacheMethod, eventArgs.Arguments.ToArray(), null);
                }
                else
                {
                    key = CacheKeyGenerator.GenerateKeyName(cacheMethod, eventArgs.Arguments.ToArray(), m_Keys);
                }
                if (key != null)
                {
                    CacheFactory.GetInstance(m_CacheName).Remove(key);
                }
            }
            else // 刷新缓存的方法没有指定Key
            {
                if (m_CacheGroupName == null)  // 缓存的方法没有指定Key的，整个方法就一个Key
                {
                    MethodInfo cacheMethod = m_Type.GetMethod(m_MethodName);
                    string key = CacheKeyGenerator.GenerateKeyName(cacheMethod, eventArgs.Arguments.ToArray(), null);
                    if (key != null)
                    {
                        CacheFactory.GetInstance(m_CacheName).Remove(key);
                    }
                }
                else
                {
                    CacheFactory.GetInstance(m_CacheName).RemoveByGroup(m_CacheGroupName);
                }
            }
        }
    }
}
