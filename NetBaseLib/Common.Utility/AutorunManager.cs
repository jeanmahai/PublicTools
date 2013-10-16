using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace Common.Utility
{
    public static class AutorunManager
    {
        private class ExecuteItem
        {
            public Type ClassType { get; set; }
            public object Instance { get; set; }
            public List<ExecuteMethodItem> MethodList { get; set; }
        }

        private class ExecuteMethodItem
        {
            public string MethodName { get; set; }
            public string[] MethodArgs { get; set; }
            public bool IsStatic { get; set; }
        }

        private static readonly string s_XmlPath = GetAutorunXmlPath();

        private static void ExecuteMethod(Type type, object instance, string methodName, bool isStatic, string[] args)
        {
            BindingFlags bf = isStatic ? BindingFlags.Static : BindingFlags.Instance;
            MethodInfo[] infoArray = type.GetMethods();
            List<ParameterInfo[]> matched = new List<ParameterInfo[]>(infoArray.Length);
            List<MethodInfo> matched_m = new List<MethodInfo>(infoArray.Length);
            foreach (var con in infoArray)
            {
                if (con.Name != methodName)
                {
                    continue;
                }
                ParameterInfo[] pArray = con.GetParameters();
                if (pArray.Length == args.Length)
                {
                    matched.Add(pArray);
                    matched_m.Add(con);
                }
            }
            if (matched.Count <= 0)
            {
                throw new ApplicationException("Can't find the method '" + methodName + "' with " + args.Length + " parameter(s) of type '" + type.AssemblyQualifiedName + "'");
            }
            StringBuilder sb = new StringBuilder();
            for (int j=0; j< matched.Count; j++)
            {
                try
                {
                    object[] realArgs = new object[matched[j].Length];
                    for (int i = 0; i < matched[j].Length; i++)
                    {
                        realArgs[i] = DataConvertor.GetValueByType(matched[j][i].ParameterType, args[i], null, null);
                    }
                    matched_m[j].Invoke(isStatic ? null : instance, realArgs);
                    return;
                }
                catch (Exception ex)
                {
                    sb.AppendLine((j + 1) + ". " + ex.Message);
                }
            }
            throw new ApplicationException("There are " + matched.Count + " matched constructor(s), but all failed to construct instance for type '" + type.AssemblyQualifiedName + "', detail error message: \r\n" + sb);
        }

        private static void ExecuteClass(ExecuteItem item, bool isStart, Action<Exception> errorHandler)
        {
            if (isStart && item.Instance != null && item.Instance is IStartup)
            {
                try
                {
                    ((IStartup)item.Instance).Start();
                }
                catch (Exception ex)
                {
                    if (errorHandler != null)
                    {
                        errorHandler(ex);
                    }
                }
            }

            if (item.MethodList != null && item.MethodList.Count > 0)
            {
                foreach (var method in item.MethodList)
                {
                    try
                    {
                        ExecuteMethod(item.ClassType, item.Instance, method.MethodName, method.IsStatic, method.MethodArgs);
                    }
                    catch (Exception ex)
                    {
                        if (errorHandler != null)
                        {
                            errorHandler(ex);
                        }
                    }
                }
            }

            if (!isStart && item.Instance != null && item.Instance is IShutdown)
            {
                try
                {
                    ((IShutdown)item.Instance).Shut();
                }
                catch (Exception ex)
                {
                    if (errorHandler != null)
                    {
                        errorHandler(ex);
                    }
                }
            }
        }

        public static void Startup(Action<Exception> errorHandler)
        {
            List<ExecuteItem> list = GetModule(true, errorHandler);
            foreach (var m in list)
            {
                try
                {
                    ExecuteClass(m, true, errorHandler);
                }
                catch (Exception ex)
                {
                    if (errorHandler != null)
                    {
                        errorHandler(ex);
                    }
                }
            }
        }

        public static void Shutdown(Action<Exception> errorHandler)
        {
            List<ExecuteItem> list = GetModule(false, errorHandler);
            foreach (var m in list)
            {
                try
                {
                    ExecuteClass(m, false, errorHandler);
                }
                catch (Exception ex)
                {
                    if (errorHandler != null)
                    {
                        errorHandler(ex);
                    }
                }
            }
        }

        private static List<ExecuteItem> GetModule(bool isStart, Action<Exception> errorHandler)
        {
            if (!File.Exists(s_XmlPath))
            {
                return new List<ExecuteItem>(0);
            }
            string xpath = isStart ? @"//autorun/startup" : @"//autorun/shutdown";
            XmlDocument x = new XmlDocument();
            x.Load(s_XmlPath);
            XmlNode node = x.SelectSingleNode(xpath);
            if (node == null || node.ChildNodes == null || node.ChildNodes.Count <= 0)
            {
                return new List<ExecuteItem>(0);
            }
            XmlNode[] mList = GetChildrenNodes(node, "module");
            if (mList == null || mList.Length <= 0)
            {
                return new List<ExecuteItem>(0);
            }
            List<ExecuteItem> rstList = new List<ExecuteItem>(mList.Length * 2);
            foreach (var n in mList)
            {
                try
                {
                    string typeName = GetNodeAttribute(n, "type");
                    if (typeName == null || typeName.Trim().Length <= 0)
                    {
                        continue;
                    }
                    string[] argments;
                    XmlNodeList args = n.SelectNodes("constructor/arg");
                    if (args == null || args.Count <= 0)
                    {
                        argments = new string[0];
                    }
                    else
                    {
                        argments = new string[args.Count];
                        for (int i = 0; i < args.Count; i++)
                        {
                            string t = args[i].InnerText;
                            argments[i] = t == null ? null : t.Trim();
                        }
                    }
                    Type type = Type.GetType(typeName, true);
                    ExecuteItem item = new ExecuteItem();
                    rstList.Add(item);
                    item.ClassType = type;
                    if (type.IsAbstract)
                    {
                        item.Instance = null;
                    }
                    else
                    {
                        item.Instance = CreateInstance(type, argments);
                    }
                    XmlNode[] methodNodeList = GetChildrenNodes(n, "method");
                    if (methodNodeList == null || methodNodeList.Length <= 0)
                    {
                        item.MethodList = new List<ExecuteMethodItem>(0);
                    }
                    else
                    {
                        item.MethodList = new List<ExecuteMethodItem>(methodNodeList.Length);
                        foreach (XmlNode mn in methodNodeList)
                        {
                            try
                            {
                                string[] aa;
                                XmlNode[] argList = GetChildrenNodes(mn, "arg");
                                if (argList == null || argList.Length <= 0)
                                {
                                    aa = new string[0];
                                }
                                else
                                {
                                    aa = new string[argList.Length];
                                    for (int i = 0; i < argList.Length; i++)
                                    {
                                        string t = argList[i].InnerText;
                                        aa[i] = t == null ? null : t.Trim();
                                    }
                                }
                                string mType = GetNodeAttribute(mn, "type");
                                item.MethodList.Add(new ExecuteMethodItem
                                {
                                    MethodName = GetNodeAttribute(mn, "name"),
                                    IsStatic = (mType != null && mType.ToLower().Trim() == "static"),
                                    MethodArgs = aa
                                });
                            }
                            catch (Exception ex)
                            {
                                if (errorHandler != null)
                                {
                                    errorHandler(ex);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (errorHandler != null)
                    {
                        errorHandler(ex);
                    }
                }
            }
            return rstList;
        }

        private static string GetAutorunXmlPath()
        {
            string path = ConfigurationManager.AppSettings["AutorunConfigPath"];
            if (path == null || path.Trim().Length <= 0)
            {
                return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Configuration/Autorun.config");
            }
            string p = Path.GetPathRoot(path);
            if (p == null || p.Trim().Length <= 0) // 说明是相对路径
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
            }
            return path;
        }

        private static object CreateInstance(Type type, string[] args)
        {
            ConstructorInfo[] infoArray = type.GetConstructors();
            List<ParameterInfo[]> matched = new List<ParameterInfo[]>(infoArray.Length);
            foreach (var con in infoArray)
            {
                ParameterInfo[] pArray = con.GetParameters();
                if (pArray.Length == args.Length)
                {
                    matched.Add(pArray);
                }
            }
            if (matched.Count <= 0)
            {
                throw new ApplicationException("Can't find the constructor with " + args.Length + " parameter(s) of type '" + type.AssemblyQualifiedName + "'");
            }
            StringBuilder sb = new StringBuilder();
            int j = 1;
            foreach (var paramArray in matched)
            {
                try
                {
                    object[] realArgs = new object[paramArray.Length];
                    for (int i = 0; i < paramArray.Length; i++)
                    {
                        realArgs[i] = DataConvertor.GetValueByType(paramArray[i].ParameterType, args[i], null, null);
                    }
                    return Activator.CreateInstance(type, realArgs);
                }
                catch(Exception ex)
                {
                    sb.AppendLine(j + ". " + ex.Message);
                }
                j++;
            }
            throw new ApplicationException("There are " + matched.Count + " matched constructor(s), but all failed to construct instance for type '" + type.AssemblyQualifiedName + "', detail error message: \r\n" + sb);
        }

        private static XmlNode[] GetChildrenNodes(XmlNode node, string nodeName)
        {
            return GetChildrenNodes(node, delegate(XmlNode child)
            {
                return child.Name == nodeName;
            });
        }

        private static XmlNode[] GetChildrenNodes(XmlNode node, Predicate<XmlNode> match)
        {
            if (node == null || node.ChildNodes == null || node.ChildNodes.Count <= 0)
            {
                return new XmlNode[0];
            }
            List<XmlNode> nodeList = new List<XmlNode>(node.ChildNodes.Count);
            foreach (XmlNode child in node.ChildNodes)
            {
                if (match(child))
                {
                    nodeList.Add(child);
                }
            }
            return nodeList.ToArray();
        }

        private static string GetNodeAttribute(XmlNode node, string attributeName)
        {
            if (node.Attributes == null
                        || node.Attributes[attributeName] == null
                        || node.Attributes[attributeName].Value == null
                        || node.Attributes[attributeName].Value.Trim() == string.Empty)
            {
                return string.Empty;
            }
            return node.Attributes[attributeName].Value.Trim();
        }
    }

    public interface IStartup
    {
        void Start();
    }

    public interface IShutdown
    {
        void Shut();
    }
}
