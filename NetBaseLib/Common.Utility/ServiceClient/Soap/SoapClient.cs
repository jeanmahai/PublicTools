using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Caching;
using System.Net;
using System.IO;
using System.CodeDom;
using System.Web.Services.Description;
using System.Xml.Serialization;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace Common.Utility
{
    public static class SoapClient
    {
        public const string Namespace = "Common.Utility.ServiceClient.Soap";

        private static bool IsNullableType(this Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && type.GetGenericArguments() != null
                    && type.GetGenericArguments().Length == 1);
        }

        private static object[] GetArgs(object[] args, Assembly asy)
        {
            if (args == null)
            {
                return null;
            }
            List<object> list = new List<object>(args.Length * 2);
            for (int i = 0; i < args.Length; i++)
            {
                object x = SoapEntityMapping.ConvertToSoapObject(args[i], asy);
                list.Add(x);
                Type t = args[i].GetType();
                if (t.IsValueType)
                {
                    if (t.IsNullableType())
                    {
                        list.Add(args[i] != null);
                    }
                    else
                    {
                        list.Add(true);
                    }
                }
            }
            return list.ToArray();
        }

        public static void Invoke(string url, string methodName, params object[] args)
        {
            Invoke<object>(url, methodName, args);
        }

        public static T Invoke<T>(string url, string methodName, params object[] args)
        {
            object proxy = CreateServiceProxyInstance(url);
            object[] realArgs = GetArgs(args, proxy.GetType().Assembly);
            if (typeof(T).IsValueType)
            {
                T obj = default(T);
                bool hasValue = false;
                List<object> list = new List<object>(realArgs);
                list.Add(obj);
                list.Add(hasValue);
                object[] x = list.ToArray();
                MethodInfo me = proxy.GetType().GetMethod(methodName);
                me.Invoke(proxy, x);
                hasValue = (bool)x[x.Length - 1];
                if (hasValue)
                {
                    object tmp = x[x.Length - 2];
                    return (T)SoapEntityMapping.ConvertFromSoapObject(tmp, typeof(T));
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                object tmp = Invoker.MethodInvoke(proxy, methodName, realArgs);
                return (T)SoapEntityMapping.ConvertFromSoapObject(tmp, typeof(T));
            }
        }

        public static void InvokeAsync(string url, string methodName, Action callback, Action<Exception> errorHandler, params object[] args)
        {
            object proxy = CreateServiceProxyInstance(url);
            object[] realArgs = GetArgs(args, proxy.GetType().Assembly);
            string beginMethodName = "Begin" + methodName;
            string endMethodName = "End" + methodName;
            AsyncCallback cb = ar =>
            {
                bool hasError = false;
                try
                {
                    Invoker.MethodInvoke(ar.AsyncState, endMethodName, ar);
                }
                catch (Exception ex)
                {
                    hasError = true;
                    errorHandler(ex);
                }
                if (hasError == false && callback != null)
                {
                    callback();
                }
            };
            List<object> list = new List<object>(realArgs);
            list.Add(cb);
            list.Add(proxy);
            MethodInfo m = proxy.GetType().GetMethod(beginMethodName);
            Invoker.MethodInvoke(proxy, beginMethodName, list.ToArray());
        }

        public static void InvokeAsync<T>(string url, string methodName, Action<T> callback, Action<Exception> errorHandler, params object[] args)
        {
            object proxy = CreateServiceProxyInstance(url);
            object[] realArgs = GetArgs(args, proxy.GetType().Assembly);
            Type t = proxy.GetType();
            string beginMethodName = "Begin" + methodName;
            string endMethodName = "End" + methodName;
            //MethodInfo beginMethod = t.GetMethod("Begin" + methodName);
            MethodInfo endMethod = t.GetMethod(endMethodName);
            AsyncCallback cb = ar =>
            {
                T obj = default(T);
                bool hasError = false;
                try
                {
                    if (typeof(T).IsValueType)
                    {
                        bool hasValue = false;
                        object[] args1 = new object[] { ar, obj, hasValue };
                        endMethod.Invoke(ar.AsyncState, args1);
                        hasValue = (bool)args1[2];
                        if (hasValue)
                        {
                            obj = (T)SoapEntityMapping.ConvertFromSoapObject(args1[1], typeof(T));
                        }
                        else
                        {
                            obj = default(T);
                        }
                    }
                    else
                    {
                        object tmp = Invoker.MethodInvoke(ar.AsyncState, endMethodName, ar);
                        obj = (T)SoapEntityMapping.ConvertFromSoapObject(tmp, typeof(T));
                    }
                }
                catch (Exception ex)
                {
                    hasError = true;
                    if (errorHandler != null)
                    {
                        errorHandler(ex);
                    }
                }
                if (hasError == false && callback != null)
                {
                    callback(obj);
                }
            };
            List<object> list = new List<object>(realArgs);
            list.Add(cb);
            list.Add(proxy);
            Invoker.MethodInvoke(proxy, beginMethodName, list.ToArray());
        }

        public static object InitProxyInstance(string url)
        {
            return CreateServiceProxyInstance(url);
        }

        #region Private 

        private static object s_SyncObj_Instance = new object();
        private static object CreateServiceProxyInstance(string url)
        {
            string key = "SoapClient_" + url.ToUpper().Trim();
            object proxy = MemoryCache.Default.Get(key);
            if (proxy != null)
            {
                return proxy;
            }
            lock (key)
            {
                proxy = MemoryCache.Default.Get(key);
                if (proxy != null)
                {
                    return proxy;
                }
                Type t = CreateServiceProxyType(url);
                proxy = Activator.CreateInstance(t);
                MemoryCache.Default.Set(key, proxy, new CacheItemPolicy()
                {
                    AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes(5)),
                });
                return proxy;
            }
        }

        private static Type CreateServiceProxyType(string url)
        {
            string @namespace = Namespace;
            WebClient wc = new WebClient();
            ServiceDescription sd;
            try
            {
                using (Stream stream = wc.OpenRead(url))
                {
                    sd = ServiceDescription.Read(stream);
                }
            }
            catch
            {
                try
                {
                    using (Stream stream = wc.OpenRead(url + "?WSDL"))
                    {
                        sd = ServiceDescription.Read(stream);
                    }
                }
                catch
                {
                    using (Stream stream = wc.OpenRead(url + "?singleWsdl"))
                    {
                        sd = ServiceDescription.Read(stream);
                    }
                }
            }
            string classname = sd.Services[0].Name;

            //生成客户端代理类代码
            CodeNamespace cn = new CodeNamespace(@namespace);
            CodeCompileUnit ccu = new CodeCompileUnit();
            ccu.Namespaces.Add(cn);

            ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();
            sdi.ProtocolName = "Soap";
            sdi.Style = ServiceDescriptionImportStyle.Client;
            sdi.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateOldAsync;
            sdi.AddServiceDescription(sd, "", "");
            sdi.Import(cn, ccu);

            //设定编译参数
            CompilerParameters cplist = new CompilerParameters();
            cplist.GenerateExecutable = false;
            cplist.GenerateInMemory = true;
            cplist.ReferencedAssemblies.Add("System.dll");
            cplist.ReferencedAssemblies.Add("System.XML.dll");
            cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
            cplist.ReferencedAssemblies.Add("System.Data.dll");
            //cplist.ReferencedAssemblies.Add("Common.Utility.dll");

            //编译代理类
            CSharpCodeProvider csc = new CSharpCodeProvider();
            CompilerResults cr = csc.CompileAssemblyFromDom(cplist, ccu);
            if (true == cr.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("SoapServiceProxy为服务地址“" + url + "”生成代理类失败.\r\n");
                foreach (CompilerError ce in cr.Errors)
                {
                    sb.AppendLine(ce.ToString());
                }
                throw new ApplicationException(sb.ToString());
            }

            //生成代理实例
            Assembly assembly = cr.CompiledAssembly;
            return assembly.GetType(@namespace + "." + classname, true, true);
        }

        #endregion
    }
}
