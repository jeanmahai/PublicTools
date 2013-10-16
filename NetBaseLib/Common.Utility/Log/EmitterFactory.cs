using System;
using System.Configuration;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility
{
    internal static class EmitterFactory
    {
        private static List<ILogEmitter> s_Emitters = null;
        private static object s_SyncObj = new object();

        public static List<ILogEmitter> Create()
        {
            if (s_Emitters == null)
            {
                lock (s_SyncObj)
                {
                    if (s_Emitters == null)
                    {
                        LogSetting s = LogSection.GetSetting();
                        List<ILogEmitter> list = new List<ILogEmitter>();

                        if (s.Emitters != null)
                        {
                            s.Emitters.ForEach(p =>
                            {
                                ILogEmitter e;
                                switch (p.Type)
                                {
                                    case "eventLog":
                                        e = new EventLogEmitter();
                                        break;
                                    case "text":
                                        e = new TextEmitter();
                                        break;
                                    case "soap":
                                        e = new SoapEmitter();
                                        break;
                                    case "restful":
                                        e = new RestfulEmitter();
                                        break;
                                    case "sqlDb":
                                        e = new SqlDbEmitter();
                                        break;
                                    case "queue":
                                        e = new QueueEmitter();
                                        break;
                                    default:
                                        Type type = Type.GetType(p.Type, true);
                                        e = (ILogEmitter)Activator.CreateInstance(type);
                                        break;
                                }
                                e.Init(p.Parameters);
                                list.Add(e);
                            });
                        }
                      
                        s_Emitters = list;
                    }
                }
            }
            return s_Emitters;
        }
    }
}