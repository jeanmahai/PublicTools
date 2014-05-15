using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soho.Utility
{
    internal static class InvokerFactory
    {
        public static bool SaveDll
        {
            get;
            set;
        }

        private static Dictionary<Type, IInvoke> s_Cache = new Dictionary<Type, IInvoke>();
        private static object s_SyncObject = new object();

        public static IInvoke GetInvoker(Type type)
        {
            if (!s_Cache.ContainsKey(type))
            {
                lock (s_SyncObject)
                {
                    if (!s_Cache.ContainsKey(type))
                    {
                        Type implType = EmitHelper.CreateType(typeof(IInvoke), new InvokerEmitter(type), InvokerFactory.SaveDll, type.FullName+"_Invoker");
                        IInvoke invoker = (IInvoke)Activator.CreateInstance(implType);
                        s_Cache.Add(type, invoker);
                        return invoker;
                    }
                }
            }
            return s_Cache[type];
        }
    }
}
