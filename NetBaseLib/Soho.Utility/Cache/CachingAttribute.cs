using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;

using PostSharp.Aspects;
using PostSharp.Extensibility;

namespace Soho.Utility
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class CachingAttribute : OnMethodBoundaryAspect
    {
        private TimeSpan m_ExpireTime = new TimeSpan(1, 0, 0); // 60 minutes
        private ExpirationType m_Expiration = ExpirationType.AbsoluteTime;
        private List<ParamCacheKey> m_Keys;
        internal string CacheName
        {
            get;
            private set;
        }

        /// <summary>
        /// ext: "00:02:00"
        /// </summary>
        public string ExpireTime
        {
            get { return m_ExpireTime.Hours + ":" + m_ExpireTime.Minutes + ":" + m_ExpireTime.Seconds; }
            set { m_ExpireTime = TimeSpan.Parse(value); }
        }

        public ExpirationType ExpiryType
        {
            get { return m_Expiration; }
            set { m_Expiration = value; }
        }

        #region Constructor

        public CachingAttribute()
            : this(null, new string[0])
        {

        }

        public CachingAttribute(string[] keys)
            : this(null, keys)
        {

        }

        public CachingAttribute(string cacheName)
            : this(cacheName, new string[0])
        {

        }

        public CachingAttribute(string cacheName, string[] keys)
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
            CacheName = cacheName;
        }

        #endregion

        public override bool CompileTimeValidate(MethodBase method)
        {
            // Don't apply to constructors.
            if (method is ConstructorInfo)
            {
                Message.Write(SeverityType.Error, "ER0001", "Cannot cache constructors. Method : '" + method.DeclaringType.FullName + "." + method.Name + "'");
                return false;
            }

            MethodInfo methodInfo = (MethodInfo)method;

            // Don't apply to void methods.
            if (methodInfo.ReturnType.Name == "Void")
            {
                Message.Write(SeverityType.Error, "ER0002", "Cannot cache void methods. Method : '" + method.DeclaringType.FullName + "." + method.Name + "'");
                return false;
            }

            // Does not support out parameters.
            ParameterInfo[] parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].IsOut)
                {
                    Message.Write(SeverityType.Error, "ER0003", "Cannot cache methods with out parameters. Method : '" + method.DeclaringType.FullName + "." + method.Name + "'");
                    return false;
                }
            }

            // 没有指定Key，那么如果方法有参数，则必须要么第一个参数实现了IIdentity接口（参数可以为多个）,
            // 要么就只有一个参数且参数类型为string或.net primitive类型(包括了枚举)或DateTime(或这些类型所对应的Nullable泛型)
            //if ((m_Keys == null || m_Keys.Length <= 0) 
            //    && (parameters.Length > 0 && !typeof(IIdentity).IsAssignableFrom(parameters[0].ParameterType))
            //    )
            //{
            //    if (parameters.Length > 1)
            //    {
            //        Message.Write(SeverityType.Error, "ER0004", "Cannot cache methods which has more than one parameters but the first parameter is not implemented ECCentral.BizEntity.IIdentity.");
            //        return false;
            //    }
            //    else if (parameters.Length == 1 && !CanAsKey(parameters[0].ParameterType))
            //    {
            //        Message.Write(SeverityType.Error, "ER0005", "Cannot cache methods which has only one parameter and the type of parameter is neither .net primitive type nor datetime and string.");
            //        return false;
            //    }
            //}

            if (m_Keys != null && m_Keys.Count > 0)
            {
                List<ParameterInfo> parameterList = new List<ParameterInfo>(parameters);
                foreach (ParamCacheKey key in m_Keys)
                {
                    int index = parameterList.FindIndex(p => p.Name == key.ParamName);
                    ParameterInfo param = (index >= 0 && index < parameterList.Count) ? parameterList[index] : null;
                    if (param == null)
                    {
                        Message.Write(SeverityType.Error, "ER0006", "There is not a parameter named '" + key.ParamName + "' with the method '" + method.DeclaringType.FullName + "." + method.Name + "'");
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

        public override void OnEntry(MethodExecutionArgs eventArgs)
        {
            string key = CacheKeyGenerator.GenerateKeyName(eventArgs.Method, eventArgs.Arguments.ToArray(), m_Keys);
            object value = key != null ? CacheFactory.GetInstance(CacheName).Get(key) : null;
            if (value == null)
            {
                eventArgs.MethodExecutionTag = key;
            }
            else
            {
                eventArgs.ReturnValue = value;
                eventArgs.FlowBehavior = FlowBehavior.Return;
                CacheStatisticManager.Hit(eventArgs.Method, CacheName, GetGroupName(eventArgs.Method));
            }
        }

        public override void OnSuccess(MethodExecutionArgs eventArgs)
        {
            string key = eventArgs.MethodExecutionTag as string;
            string group = GetGroupName(eventArgs.Method);
            if (key == null)
            {
                return;
            }
            object returnValue = eventArgs.ReturnValue;
            if (returnValue != null)
            {
                if (m_Expiration == ExpirationType.AbsoluteTime)
                {
                    CacheFactory.GetInstance(CacheName).Set(key, returnValue, DateTime.Now.Add(m_ExpireTime), group);
                }
                else if (m_Expiration == ExpirationType.SlidingTime)
                {
                    CacheFactory.GetInstance(CacheName).Set(key, returnValue, m_ExpireTime, group);
                }
                else
                {
                    CacheFactory.GetInstance(CacheName).Set(key, returnValue, group);
                }
            }
            CacheStatisticManager.NotHit(eventArgs.Method, CacheName, group);
        }

        internal string GetGroupName(MethodBase method)
        {
            if ((m_Keys == null || m_Keys.Count <= 0) && method.GetParameters().Length <= 0)
            {
                return null;
            }
            return CacheKeyGenerator.GenerateGroupName(method);
        }
    }

    [Serializable]
    internal class ParamCacheKey
    {
        public string ParamName
        {
            get;
            set;
        }

        public int ParamIndex
        {
            get;
            set;
        }

        public string[] ParamProperty
        {
            get;
            set;
        }
    }

    [Serializable]
    public enum ExpirationType
    {
        SlidingTime,
        AbsoluteTime,
        Never
    }
}
