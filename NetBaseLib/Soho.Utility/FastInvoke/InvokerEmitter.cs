using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.ComponentModel;

namespace Soho.Utility
{
    internal class InvokerEmitter : IMethodEmit
    {
        private Type m_InvokedType;

        private BindingFlags m_BindingFlags = BindingFlags.Public | BindingFlags.Instance;

        public InvokerEmitter(Type type)
        {
            m_InvokedType = type;
        }

        public void CreateMethod(ILGenerator ilg, MethodInfo methodInfo)
        {
            switch (methodInfo.Name)
            {
                case "CreateInstance":
                    Emit_CreateInstance(ilg);
                    break;
                case "MethodInvoke":
                    Emit_MethodInvoke(ilg);
                    break;
                case "PropertyGet":
                    Emit_PropertyGet(ilg);
                    break;
                case "PropertySet":
                    Emit_PropertySet(ilg);
                    break;
                case "GetPropertyType":
                    Emit_GetPropertyType(ilg);
                    break;
                case "GetPropertyNameIgnoreCase":
                    Emit_GetPropertyNameIgnoreCase(ilg);
                    break;
                case "ExistPropertyOrIndexerSet":
                    Emit_ExistPropertyOrIndexer(ilg, true);
                    break;
                case "ExistPropertyOrIndexerGet":
                    Emit_ExistPropertyOrIndexer(ilg, false);
                    break;
                case "GetIndexerType":
                    Emit_GetIndexerType(ilg);
                    break;
                case "IndexerGet":
                    Emit_IndexerGet(ilg);
                    break;
                case "IndexerSet":
                    Emit_IndexerSet(ilg);
                    break;
                default:
                    throw new ApplicationException("Not support to emit methd '" + methodInfo.Name + "'.");
            }
        }

        // void IndexerSet(object obj, object value, params object[] indexerParameters)
        private void Emit_IndexerSet(ILGenerator ilg)
        {
            PropertyInfo[] methodArray = GetIndexer(m_InvokedType, m_BindingFlags);
            foreach (PropertyInfo property in methodArray)
            {
                MethodInfo method = property.GetSetMethod();
                if (method == null)
                {
                    continue;
                }

                Label label1 = ilg.DefineLabel();

                ParameterInfo[] pInfoArray = method.GetParameters();
                ilg.Emit(OpCodes.Ldarg, 3);
                ilg.Emit(OpCodes.Ldlen);
                ilg.Emit(OpCodes.Conv_I4);
                ilg.Emit(OpCodes.Ldc_I4_S, pInfoArray.Length - 1);
                ilg.Emit(OpCodes.Bne_Un, label1);

                for (int i = 0; i < pInfoArray.Length - 1; i++)
                {
                    ilg.Emit(OpCodes.Ldarg, 3);
                    ilg.Emit(OpCodes.Ldc_I4_S, i);
                    ilg.Emit(OpCodes.Ldelem_Ref);
                    ilg.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetType", Type.EmptyTypes));
                    ilg.Emit(OpCodes.Ldtoken, pInfoArray[i].ParameterType);
                    ilg.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public));
                    ilg.Emit(OpCodes.Bne_Un, label1);
                }

                ilg.Emit(OpCodes.Ldarg, 1);
                ilg.Emit(OpCodes.Castclass, m_InvokedType);
                for (int i = 0; i < pInfoArray.Length - 1; i++)
                {
                    ilg.Emit(OpCodes.Ldarg, 3);
                    ilg.Emit(OpCodes.Ldc_I4_S, i);
                    ilg.Emit(OpCodes.Ldelem_Ref);
                    if (pInfoArray[i].ParameterType.IsValueType)
                    {
                        ilg.Emit(OpCodes.Unbox_Any, pInfoArray[i].ParameterType);
                    }
                    else
                    {
                        ilg.Emit(OpCodes.Castclass, pInfoArray[i].ParameterType);
                    }
                }
                ilg.Emit(OpCodes.Ldarg, 2);
                ilg.Emit(OpCodes.Ldnull);
                ilg.Emit(OpCodes.Ldstr, property.DeclaringType.FullName + "的索引器");
                ilg.Emit(OpCodes.Call, typeof(DataConvertor).GetMethod("GetValue", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(pInfoArray[pInfoArray.Length - 1].ParameterType));
                ilg.Emit(OpCodes.Callvirt, method);
                if (method.ReturnType != typeof(void))
                {
                    ilg.Emit(OpCodes.Pop);
                }
                ilg.Emit(OpCodes.Ret);
                ilg.MarkLabel(label1);
            }

            LocalBuilder local_sb = ilg.DeclareLocal(typeof(StringBuilder));
            LocalBuilder local_index = ilg.DeclareLocal(typeof(int));
            LocalBuilder local_parm = ilg.DeclareLocal(typeof(object));
            Label labelFor = ilg.DefineLabel();
            Label labelForBegin = ilg.DefineLabel();
            Label labelTmp = ilg.DefineLabel();
            ilg.Emit(OpCodes.Newobj, typeof(StringBuilder).GetConstructor(Type.EmptyTypes));
            ilg.Emit(OpCodes.Stloc_S, local_sb);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, "无法找到类型'" + m_InvokedType.FullName + "'的public的可写的索引器[");
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
            ilg.Emit(OpCodes.Pop);

            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Stloc_S, local_index);

            ilg.Emit(OpCodes.Br, labelFor);
            ilg.MarkLabel(labelForBegin);

            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Ble, labelTmp);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, ", ");
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
            ilg.Emit(OpCodes.Pop);

            ilg.MarkLabel(labelTmp);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, "{0} parm{1}");
            ilg.Emit(OpCodes.Ldarg, 3);
            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldelem_Ref);
            ilg.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetType", Type.EmptyTypes));
            ilg.Emit(OpCodes.Callvirt, typeof(MemberInfo).GetMethod("get_Name", Type.EmptyTypes));
            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Box, typeof(int));
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("AppendFormat", new Type[] { typeof(string), typeof(object), typeof(object) }));
            ilg.Emit(OpCodes.Pop);

            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldc_I4_1);
            ilg.Emit(OpCodes.Add);
            ilg.Emit(OpCodes.Stloc_S, local_index);

            ilg.MarkLabel(labelFor);
            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldarg, 3);
            ilg.Emit(OpCodes.Ldlen);
            ilg.Emit(OpCodes.Conv_I4);
            ilg.Emit(OpCodes.Blt, labelForBegin);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, "]");
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
            ilg.Emit(OpCodes.Pop);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString", Type.EmptyTypes));
            ilg.Emit(OpCodes.Newobj, typeof(ApplicationException).GetConstructor(new Type[1] { typeof(string) }));
            ilg.Emit(OpCodes.Throw);
        }

        // object IndexerGet(object obj, params object[] indexerParameters)
        private void Emit_IndexerGet(ILGenerator ilg)
        {
            PropertyInfo[] methodArray = GetIndexer(m_InvokedType, m_BindingFlags);
            foreach (PropertyInfo property in methodArray)
            {
                MethodInfo method = property.GetGetMethod();
                if (method == null)
                {
                    continue;
                }

                Label label1 = ilg.DefineLabel();

                ParameterInfo[] pInfoArray = method.GetParameters();
                ilg.Emit(OpCodes.Ldarg, 2);
                ilg.Emit(OpCodes.Ldlen);
                ilg.Emit(OpCodes.Conv_I4);
                ilg.Emit(OpCodes.Ldc_I4_S, pInfoArray.Length);
                ilg.Emit(OpCodes.Bne_Un, label1);

                for (int i = 0; i < pInfoArray.Length; i++)
                {
                    ilg.Emit(OpCodes.Ldarg, 2);
                    ilg.Emit(OpCodes.Ldc_I4_S, i);
                    ilg.Emit(OpCodes.Ldelem_Ref);
                    ilg.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetType", Type.EmptyTypes));
                    ilg.Emit(OpCodes.Ldtoken, pInfoArray[i].ParameterType);
                    ilg.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public));
                    ilg.Emit(OpCodes.Bne_Un, label1);
                }

                ilg.Emit(OpCodes.Ldarg, 1);
                ilg.Emit(OpCodes.Castclass, m_InvokedType);
                for (int i = 0; i < pInfoArray.Length; i++)
                {
                    ilg.Emit(OpCodes.Ldarg, 2);
                    ilg.Emit(OpCodes.Ldc_I4_S, i);
                    ilg.Emit(OpCodes.Ldelem_Ref);
                    if (pInfoArray[i].ParameterType.IsValueType)
                    {
                        ilg.Emit(OpCodes.Unbox_Any, pInfoArray[i].ParameterType);
                    }
                    else
                    {
                        ilg.Emit(OpCodes.Castclass, pInfoArray[i].ParameterType);
                    }
                }
                ilg.Emit(OpCodes.Callvirt, method);
                if (method.ReturnType == typeof(void))
                {
                    ilg.Emit(OpCodes.Ldnull);
                }
                else if (method.ReturnType.IsValueType)
                {
                    ilg.Emit(OpCodes.Box, method.ReturnType);
                }
                ilg.Emit(OpCodes.Ret);
                ilg.MarkLabel(label1);
            }
            LocalBuilder local_sb = ilg.DeclareLocal(typeof(StringBuilder));
            LocalBuilder local_index = ilg.DeclareLocal(typeof(int));
            LocalBuilder local_parm = ilg.DeclareLocal(typeof(object));
            Label labelFor = ilg.DefineLabel();
            Label labelForBegin = ilg.DefineLabel();
            Label labelTmp = ilg.DefineLabel();
            ilg.Emit(OpCodes.Newobj, typeof(StringBuilder).GetConstructor(Type.EmptyTypes));
            ilg.Emit(OpCodes.Stloc_S, local_sb);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, "无法找到类型'" + m_InvokedType.FullName + "'的public的可读的索引器[");
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
            ilg.Emit(OpCodes.Pop);

            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Stloc_S, local_index);

            ilg.Emit(OpCodes.Br, labelFor);
            ilg.MarkLabel(labelForBegin);

            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Ble, labelTmp);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, ", ");
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
            ilg.Emit(OpCodes.Pop);

            ilg.MarkLabel(labelTmp);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, "{0} parm{1}");
            ilg.Emit(OpCodes.Ldarg, 2);
            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldelem_Ref);
            ilg.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetType", Type.EmptyTypes));
            ilg.Emit(OpCodes.Callvirt, typeof(MemberInfo).GetMethod("get_Name", Type.EmptyTypes));
            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Box, typeof(int));
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("AppendFormat", new Type[] { typeof(string), typeof(object), typeof(object) }));
            ilg.Emit(OpCodes.Pop);

            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldc_I4_1);
            ilg.Emit(OpCodes.Add);
            ilg.Emit(OpCodes.Stloc_S, local_index);

            ilg.MarkLabel(labelFor);
            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldarg, 2);
            ilg.Emit(OpCodes.Ldlen);
            ilg.Emit(OpCodes.Conv_I4);
            ilg.Emit(OpCodes.Blt, labelForBegin);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, "]");
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
            ilg.Emit(OpCodes.Pop);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString", Type.EmptyTypes));
            ilg.Emit(OpCodes.Newobj, typeof(ApplicationException).GetConstructor(new Type[1] { typeof(string) }));
            ilg.Emit(OpCodes.Throw);

        }

        // Type GetIndexerType(params Type[] parameterTypeList)
        private void Emit_GetIndexerType(ILGenerator ilg)
        {
            PropertyInfo[] methodArray = GetIndexer(m_InvokedType, m_BindingFlags);
            foreach (PropertyInfo property in methodArray)
            {
                List<ParameterInfo> paramList;
                MethodInfo method = property.GetGetMethod();
                if (method != null)
                {
                    paramList = new List<ParameterInfo>(method.GetParameters());
                }
                else
                {
                    method = property.GetSetMethod();
                    if (method == null)
                    {
                        continue;
                    }
                    // Set方法会多一个参数，即属性的返回值会作为Set方法的最后一个参数，所以需要把最后一个参数忽略掉
                    ParameterInfo[] tmp = method.GetParameters();
                    paramList = new List<ParameterInfo>(tmp.Length - 1);
                    for (int i = 0; i < tmp.Length - 1; i++)
                    {
                        paramList.Add(tmp[i]);
                    }
                }
                Label label1 = ilg.DefineLabel();

                // 1. 比较索引器的参数数量
                ilg.Emit(OpCodes.Ldarg_1);
                ilg.Emit(OpCodes.Ldlen);
                ilg.Emit(OpCodes.Conv_I4);
                ilg.Emit(OpCodes.Ldc_I4_S, paramList.Count);
                ilg.Emit(OpCodes.Bne_Un, label1);

                // 2. 比较索引器的每个参数的类型是否匹配
                for (int i = 0; i < paramList.Count; i++)
                {
                    ParameterInfo p = paramList[i];
                    ilg.Emit(OpCodes.Ldarg_1);
                    ilg.Emit(OpCodes.Ldc_I4_S, i);
                    ilg.Emit(OpCodes.Ldelem_Ref);
                    ilg.Emit(OpCodes.Ldtoken, p.ParameterType);
                    ilg.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public));
                    ilg.Emit(OpCodes.Bne_Un, label1);
                }
                ilg.Emit(OpCodes.Ldtoken, property.PropertyType);
                ilg.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public));
                ilg.Emit(OpCodes.Ret);
                ilg.MarkLabel(label1);
            }
            ilg.Emit(OpCodes.Ldnull);
            ilg.Emit(OpCodes.Ret);
        }

        // isSet = true --- bool ExistPropertySet(string propertyName, params Type[] parameterTypeList)
        // isSet = false --- bool ExistPropertyGet(string propertyName, params Type[] parameterTypeList)
        private void Emit_ExistPropertyOrIndexer(ILGenerator ilg, bool isSet)
        {
            PropertyInfo[] pInfoArray = m_InvokedType.GetProperties(m_BindingFlags);
            if (pInfoArray == null || pInfoArray.Length <= 0)
            {
                ilg.Emit(OpCodes.Ldc_I4_0);
                ilg.Emit(OpCodes.Ret);
                return;
            }

            foreach (PropertyInfo property in pInfoArray)
            {
                MethodInfo method = isSet ? property.GetSetMethod() : property.GetGetMethod();
                if (method == null)
                {
                    continue;
                }
                ParameterInfo[] paramInfoArray = method.GetParameters();
                List<ParameterInfo> paramList;
                if (isSet) // Set方法会多一个参数，即属性的返回值会作为Set方法的最后一个参数，所以需要把最后一个参数忽略掉
                {
                    paramList = new List<ParameterInfo>(paramInfoArray.Length - 1);
                    for (int i = 0; i < paramInfoArray.Length - 1; i++)
                    {
                        paramList.Add(paramInfoArray[i]);
                    }
                }
                else
                {
                    paramList = new List<ParameterInfo>(paramInfoArray);
                }
                Label label1 = ilg.DefineLabel();

                // 1. 比较属性名字
                ilg.Emit(OpCodes.Ldarg_1);
                ilg.Emit(OpCodes.Ldstr, property.Name);
                ilg.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) }));
                ilg.Emit(OpCodes.Brfalse, label1);

                // 2. 比较Set或Get方法的参数数量
                ilg.Emit(OpCodes.Ldarg_2);
                ilg.Emit(OpCodes.Ldlen);
                ilg.Emit(OpCodes.Conv_I4);
                ilg.Emit(OpCodes.Ldc_I4_S, paramList.Count);
                ilg.Emit(OpCodes.Bne_Un, label1);

                // 3. 比较Set或Get方法的每个参数的类型是否匹配
                for (int i = 0; i < paramList.Count; i++)
                {
                    ParameterInfo p = paramList[i];
                    ilg.Emit(OpCodes.Ldarg_2);
                    ilg.Emit(OpCodes.Ldc_I4_S, i);
                    ilg.Emit(OpCodes.Ldelem_Ref);
                    ilg.Emit(OpCodes.Ldtoken, p.ParameterType);
                    ilg.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public));
                    ilg.Emit(OpCodes.Bne_Un, label1);
                }
                ilg.Emit(OpCodes.Ldc_I4_1);
                ilg.Emit(OpCodes.Ret);
                ilg.MarkLabel(label1);
            }
            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Ret);
        }

        // string GetPropertyNameIgnoreCase(string propertyName)
        private void Emit_GetPropertyNameIgnoreCase(ILGenerator ilg)
        {
            PropertyInfo[] methodArray = GetPropertyExcludeIndexer(m_InvokedType, m_BindingFlags);
            if (methodArray == null || methodArray.Length <= 0)
            {
                ilg.Emit(OpCodes.Ldnull);
                ilg.Emit(OpCodes.Ret);
                return;
            }

            foreach (PropertyInfo property in methodArray)
            {
                Label label1 = ilg.DefineLabel();
                ilg.Emit(OpCodes.Ldarg, 1);
                ilg.Emit(OpCodes.Callvirt, typeof(string).GetMethod("ToUpper", Type.EmptyTypes));
                ilg.Emit(OpCodes.Ldstr, property.Name.ToUpper());
                ilg.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) }));
                ilg.Emit(OpCodes.Brfalse, label1);
                ilg.Emit(OpCodes.Ldstr, property.Name);
                ilg.Emit(OpCodes.Ret);
                ilg.MarkLabel(label1);
            }
            ilg.Emit(OpCodes.Ldnull);
            ilg.Emit(OpCodes.Ret);
        }

        // Type GetPropertyType(string propertyName);
        private void Emit_GetPropertyType(ILGenerator ilg)
        {
            PropertyInfo[] methodArray = GetPropertyExcludeIndexer(m_InvokedType, m_BindingFlags);
            if (methodArray == null || methodArray.Length <= 0)
            {
                //ilg.Emit(OpCodes.Ldstr, "类型'" + m_InvokedType.FullName + "'没有public的实例属性.");
                //ilg.Emit(OpCodes.Newobj, typeof(ApplicationException).GetConstructor(new Type[1] { typeof(string) }));
                //ilg.Emit(OpCodes.Throw);
                ilg.Emit(OpCodes.Ldnull);
                ilg.Emit(OpCodes.Ret);
                return;
            }

            foreach (PropertyInfo property in methodArray)
            {
                Label label1 = ilg.DefineLabel();
                ilg.Emit(OpCodes.Ldarg, 1);
                ilg.Emit(OpCodes.Ldstr, property.Name);
                ilg.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) }));
                ilg.Emit(OpCodes.Brfalse, label1);
                ilg.Emit(OpCodes.Ldtoken, property.PropertyType);
                ilg.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public));
                ilg.Emit(OpCodes.Ret);
                ilg.MarkLabel(label1);
            }
            //ilg.Emit(OpCodes.Ldstr, "无法找到类型'" + m_InvokedType.FullName + "'的公开实例属性{0}.");
            //ilg.Emit(OpCodes.Ldarg, 1);
            //ilg.Emit(OpCodes.Call, typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object) }));
            //ilg.Emit(OpCodes.Newobj, typeof(ApplicationException).GetConstructor(new Type[1] { typeof(string) }));
            //ilg.Emit(OpCodes.Throw);
            ilg.Emit(OpCodes.Ldnull);
            ilg.Emit(OpCodes.Ret);
        }

        // PropertySet(object obj, string propertyName, object value);
        private void Emit_PropertySet(ILGenerator ilg)
        {
            PropertyInfo[] methodArray = GetPropertyExcludeIndexer(m_InvokedType, m_BindingFlags);
            if (methodArray == null || methodArray.Length <= 0)
            {
                ilg.Emit(OpCodes.Ldstr, "类型'" + m_InvokedType.FullName + "'没有public的实例属性.");
                ilg.Emit(OpCodes.Newobj, typeof(ApplicationException).GetConstructor(new Type[1] { typeof(string) }));
                ilg.Emit(OpCodes.Throw);
                return;
            }

            foreach (PropertyInfo property in methodArray)
            {
                MethodInfo setMethod = property.GetSetMethod();
                if (setMethod == null)
                {
                    continue;
                }
                Label label1 = ilg.DefineLabel();
                ilg.Emit(OpCodes.Ldarg, 2);
                ilg.Emit(OpCodes.Ldstr, property.Name);
                ilg.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) }));
                ilg.Emit(OpCodes.Brfalse, label1);

                ilg.Emit(OpCodes.Ldarg, 1);
                ilg.Emit(OpCodes.Castclass, m_InvokedType);
                ilg.Emit(OpCodes.Ldarg, 3);

                ilg.Emit(OpCodes.Ldnull);
                ilg.Emit(OpCodes.Ldstr, property.DeclaringType.FullName + "." + property.Name);
                ilg.Emit(OpCodes.Call, typeof(DataConvertor).GetMethod("GetValue", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(property.PropertyType));
                
                ilg.Emit(OpCodes.Callvirt, setMethod);
                if (setMethod.ReturnType != typeof(void))
                {
                    ilg.Emit(OpCodes.Pop);
                }
                ilg.Emit(OpCodes.Ret);
                ilg.MarkLabel(label1);
            }
            ilg.Emit(OpCodes.Ldstr, "无法找到类型'" + m_InvokedType.FullName + "'的公开实例属性{0}或该属性只读.");
            ilg.Emit(OpCodes.Ldarg, 2);
            ilg.Emit(OpCodes.Call, typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object) }));
            ilg.Emit(OpCodes.Newobj, typeof(ApplicationException).GetConstructor(new Type[1] { typeof(string) }));
            ilg.Emit(OpCodes.Throw);
        }

        // object PropertyGet(object obj, string propertyName)
        private void Emit_PropertyGet(ILGenerator ilg)
        {
            PropertyInfo[] methodArray = GetPropertyExcludeIndexer(m_InvokedType, m_BindingFlags);
            if (methodArray == null || methodArray.Length <= 0)
            {
                ilg.Emit(OpCodes.Ldstr, "类型'" + m_InvokedType.FullName + "'没有public的实例属性.");
                ilg.Emit(OpCodes.Newobj, typeof(ApplicationException).GetConstructor(new Type[1] { typeof(string) }));
                ilg.Emit(OpCodes.Throw);
                return;
            }

            foreach (PropertyInfo property in methodArray)
            {
                MethodInfo getMethod = property.GetGetMethod();
                if (getMethod == null)
                {
                    continue;
                }
                Label label1 = ilg.DefineLabel();
                ilg.Emit(OpCodes.Ldarg, 2);
                ilg.Emit(OpCodes.Ldstr, property.Name);
                ilg.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) }));
                ilg.Emit(OpCodes.Brfalse, label1);

                ilg.Emit(OpCodes.Ldarg, 1);
                ilg.Emit(OpCodes.Castclass, m_InvokedType);
                
                ilg.Emit(OpCodes.Callvirt, getMethod);
                if (getMethod.ReturnType == typeof(void))
                {
                    ilg.Emit(OpCodes.Ldnull);
                }
                else if (getMethod.ReturnType.IsValueType)
                {
                    ilg.Emit(OpCodes.Box, getMethod.ReturnType);
                }
                ilg.Emit(OpCodes.Ret);
                ilg.MarkLabel(label1);
            }
            ilg.Emit(OpCodes.Ldstr, "无法找到类型'" + m_InvokedType.FullName + "'的公开实例属性{0}或该属性只写.");
            ilg.Emit(OpCodes.Ldarg, 2);
            ilg.Emit(OpCodes.Call, typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object) }));
            ilg.Emit(OpCodes.Newobj, typeof(ApplicationException).GetConstructor(new Type[1] { typeof(string) }));
            ilg.Emit(OpCodes.Throw);
        }

        // object MethodInvoke(object obj, string methodName, params object[] parameters)
        private void Emit_MethodInvoke(ILGenerator ilg)
        {
            MethodInfo[] methodArray = GetMethodsExcludePropertyMethodAndOpenGenericTypeMethod(m_InvokedType, m_BindingFlags);
            if (methodArray == null || methodArray.Length <= 0)
            {
                ilg.Emit(OpCodes.Ldstr, "类型'" + m_InvokedType.FullName + "'没有public的实例方法.");
                ilg.Emit(OpCodes.Newobj, typeof(ApplicationException).GetConstructor(new Type[1] { typeof(string) }));
                ilg.Emit(OpCodes.Throw);
                return;
            }

            foreach (MethodInfo method in methodArray)
            {
                Label label1 = ilg.DefineLabel();
                ParameterInfo[] pInfoArray = method.GetParameters();
                bool hasRefOrOut = false;
                foreach (ParameterInfo p in pInfoArray)
                {
                    if (p.IsOut || p.ParameterType.IsByRef)
                    {
                        hasRefOrOut = true;
                        break;
                    }
                }
                if (hasRefOrOut)
                {
                    continue;
                }
                ilg.Emit(OpCodes.Ldarg, 2);
                ilg.Emit(OpCodes.Ldstr, method.Name);
                ilg.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) }));
                ilg.Emit(OpCodes.Brfalse, label1);

                ilg.Emit(OpCodes.Ldarg, 3);
                ilg.Emit(OpCodes.Ldlen);
                ilg.Emit(OpCodes.Conv_I4);
                ilg.Emit(OpCodes.Ldc_I4_S, pInfoArray.Length);
                ilg.Emit(OpCodes.Bne_Un, label1);

                for (int i = 0; i < pInfoArray.Length; i++)
                {
                    ilg.Emit(OpCodes.Ldarg, 3);
                    ilg.Emit(OpCodes.Ldc_I4_S, i);
                    ilg.Emit(OpCodes.Ldelem_Ref);
                    ilg.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetType", Type.EmptyTypes));
                    ilg.Emit(OpCodes.Ldtoken, pInfoArray[i].ParameterType);
                    ilg.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public));
                    ilg.Emit(OpCodes.Call, typeof(DataConvertor).GetMethod("CompareType", BindingFlags.Static | BindingFlags.Public));
                    ilg.Emit(OpCodes.Brfalse, label1);
                }

                ilg.Emit(OpCodes.Ldarg, 1);
                ilg.Emit(OpCodes.Castclass, m_InvokedType);
                for (int i = 0; i < pInfoArray.Length; i++)
                {
                    ilg.Emit(OpCodes.Ldarg, 3);
                    ilg.Emit(OpCodes.Ldc_I4_S, i);
                    ilg.Emit(OpCodes.Ldelem_Ref);
                    if (pInfoArray[i].ParameterType.IsValueType)
                    {
                        ilg.Emit(OpCodes.Unbox_Any, pInfoArray[i].ParameterType);
                    }
                    else
                    {
                        ilg.Emit(OpCodes.Castclass, pInfoArray[i].ParameterType);
                    }
                }
                ilg.Emit(OpCodes.Callvirt, method);
                if (method.ReturnType == typeof(void))
                {
                    ilg.Emit(OpCodes.Ldnull);
                }
                else if (method.ReturnType.IsValueType)
                {
                    ilg.Emit(OpCodes.Box, method.ReturnType);
                }
                ilg.Emit(OpCodes.Ret);
                ilg.MarkLabel(label1);
            }
            LocalBuilder local_sb = ilg.DeclareLocal(typeof(StringBuilder));
            LocalBuilder local_index = ilg.DeclareLocal(typeof(int));
            LocalBuilder local_parm = ilg.DeclareLocal(typeof(object));
            Label labelFor = ilg.DefineLabel();
            Label labelForBegin = ilg.DefineLabel();
            Label labelTmp = ilg.DefineLabel();
            ilg.Emit(OpCodes.Newobj, typeof(StringBuilder).GetConstructor(Type.EmptyTypes));
            ilg.Emit(OpCodes.Stloc_S, local_sb);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, "无法找到类型'" + m_InvokedType.FullName + "'的公开实例方法{0}(");
            ilg.Emit(OpCodes.Ldarg, 2);
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("AppendFormat", new Type[] { typeof(string), typeof(object) }));
            ilg.Emit(OpCodes.Pop);

            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Stloc_S, local_index);

            ilg.Emit(OpCodes.Br, labelFor);
            ilg.MarkLabel(labelForBegin);

            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Ble, labelTmp);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, ", ");
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
            ilg.Emit(OpCodes.Pop);

            ilg.MarkLabel(labelTmp);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, "{0} parm{1}");
            ilg.Emit(OpCodes.Ldarg, 3);
            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldelem_Ref);
            ilg.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetType", Type.EmptyTypes));
            ilg.Emit(OpCodes.Callvirt, typeof(MemberInfo).GetMethod("get_Name", Type.EmptyTypes));
            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Box, typeof(int));
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("AppendFormat", new Type[] { typeof(string), typeof(object), typeof(object) }));
            ilg.Emit(OpCodes.Pop);

            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldc_I4_1);
            ilg.Emit(OpCodes.Add);
            ilg.Emit(OpCodes.Stloc_S, local_index);

            ilg.MarkLabel(labelFor);
            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldarg, 3);
            ilg.Emit(OpCodes.Ldlen);
            ilg.Emit(OpCodes.Conv_I4);
            ilg.Emit(OpCodes.Blt, labelForBegin);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, ")");
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
            ilg.Emit(OpCodes.Pop);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString", Type.EmptyTypes));
            ilg.Emit(OpCodes.Newobj, typeof(ApplicationException).GetConstructor(new Type[1] { typeof(string) }));
            ilg.Emit(OpCodes.Throw);
        }

        // object CreateInstance(params object[] parameters)
        private void Emit_CreateInstance(ILGenerator ilg)
        {
            ConstructorInfo[] cInfoArray = m_InvokedType.GetConstructors(m_BindingFlags);
            if (cInfoArray == null || cInfoArray.Length <= 0)
            {
                ilg.Emit(OpCodes.Ldstr, "类型'" + m_InvokedType.FullName + "'没有public的实例构造函数.");
                ilg.Emit(OpCodes.Newobj, typeof(ApplicationException).GetConstructor(new Type[1] { typeof(string) }));
                ilg.Emit(OpCodes.Throw);
                return;
            }
            foreach (ConstructorInfo cInfo in cInfoArray)
            {
                Label label1 = ilg.DefineLabel();
                ParameterInfo[] pInfoArray = cInfo.GetParameters();
                ilg.Emit(OpCodes.Ldarg, 1);
                ilg.Emit(OpCodes.Ldlen);
                ilg.Emit(OpCodes.Conv_I4);
                ilg.Emit(OpCodes.Ldc_I4_S, pInfoArray.Length);
                ilg.Emit(OpCodes.Bne_Un, label1);
                for (int i = 0; i < pInfoArray.Length; i++)
                {
                    ilg.Emit(OpCodes.Ldarg, 1);
                    ilg.Emit(OpCodes.Ldc_I4_S, i);
                    ilg.Emit(OpCodes.Ldelem_Ref);
                    ilg.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetType", Type.EmptyTypes));
                    ilg.Emit(OpCodes.Ldtoken, pInfoArray[i].ParameterType);
                    ilg.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public));
                    ilg.Emit(OpCodes.Bne_Un, label1);
                }
                for (int i = 0; i < pInfoArray.Length; i++)
                {
                    ilg.Emit(OpCodes.Ldarg, 1);
                    ilg.Emit(OpCodes.Ldc_I4_S, i);
                    ilg.Emit(OpCodes.Ldelem_Ref);
                    if (pInfoArray[i].ParameterType.IsValueType)
                    {
                        ilg.Emit(OpCodes.Unbox_Any, pInfoArray[i].ParameterType);
                    }
                    else
                    {
                        ilg.Emit(OpCodes.Castclass, pInfoArray[i].ParameterType);
                    }
                }
                ilg.Emit(OpCodes.Newobj, cInfo);
                ilg.Emit(OpCodes.Ret);
                ilg.MarkLabel(label1);
            }
            //ilg.Emit(OpCodes.Ldnull);

            LocalBuilder local_sb = ilg.DeclareLocal(typeof(StringBuilder));
            LocalBuilder local_index = ilg.DeclareLocal(typeof(int));
            LocalBuilder local_parm = ilg.DeclareLocal(typeof(object));
            Label labelFor = ilg.DefineLabel();
            Label labelForBegin = ilg.DefineLabel();
            Label labelTmp = ilg.DefineLabel();
            ilg.Emit(OpCodes.Newobj, typeof(StringBuilder).GetConstructor(Type.EmptyTypes));
            ilg.Emit(OpCodes.Stloc_S, local_sb);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, "无法找到类型'" + m_InvokedType.FullName + "'的公开实例构造方法" + m_InvokedType.Name + "(");
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
            ilg.Emit(OpCodes.Pop);

            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Stloc_S, local_index);

            ilg.Emit(OpCodes.Br, labelFor);
            ilg.MarkLabel(labelForBegin);

            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Ble, labelTmp);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, ", ");
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
            ilg.Emit(OpCodes.Pop);

            ilg.MarkLabel(labelTmp);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, "{0} parm{1}");
            ilg.Emit(OpCodes.Ldarg, 1);
            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldelem_Ref);
            ilg.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetType", Type.EmptyTypes));
            ilg.Emit(OpCodes.Callvirt, typeof(MemberInfo).GetMethod("get_Name", Type.EmptyTypes));
            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Box, typeof(int));
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("AppendFormat", new Type[] { typeof(string), typeof(object), typeof(object) }));
            ilg.Emit(OpCodes.Pop);

            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldc_I4_1);
            ilg.Emit(OpCodes.Add);
            ilg.Emit(OpCodes.Stloc_S, local_index);

            ilg.MarkLabel(labelFor);
            ilg.Emit(OpCodes.Ldloc_S, local_index);
            ilg.Emit(OpCodes.Ldarg, 1);
            ilg.Emit(OpCodes.Ldlen);
            ilg.Emit(OpCodes.Conv_I4);
            ilg.Emit(OpCodes.Blt, labelForBegin);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Ldstr, ")");
            ilg.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
            ilg.Emit(OpCodes.Pop);

            ilg.Emit(OpCodes.Ldloc_S, local_sb);
            ilg.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString", Type.EmptyTypes));
            ilg.Emit(OpCodes.Newobj, typeof(ApplicationException).GetConstructor(new Type[1] { typeof(string) }));
            ilg.Emit(OpCodes.Throw);
        }

        #region Helper Method

        // 排除掉属性的get、set方法，以及具有泛型参数的方法
        private MethodInfo[] GetMethodsExcludePropertyMethodAndOpenGenericTypeMethod(Type type, BindingFlags bindingAttr)
        {
            MethodInfo[] methodArray = type.GetMethods(bindingAttr);
            PropertyInfo[] pArray = type.GetProperties(bindingAttr);
            // 获取所有的属性的set和get方法，放到列表mList中
            List<MethodInfo> mList;
            if (pArray != null && pArray.Length > 0)
            {
                mList = new List<MethodInfo>(pArray.Length * 2);
                foreach (PropertyInfo p in pArray)
                {
                    MethodInfo method = p.GetGetMethod();
                    if (method != null)
                    {
                        mList.Add(method);
                    } 
                    method = p.GetSetMethod();
                    if (method != null)
                    {
                        mList.Add(method);
                    }
                }
            }
            else
            {
                mList = new List<MethodInfo>(0);
            }

            // 过滤掉所有的属性的set和get方法
            List<MethodInfo> rst = new List<MethodInfo>(methodArray.Length);
            foreach (MethodInfo method in methodArray)
            {
                if (method.ContainsGenericParameters) // 带有泛型参数
                {
                    continue;
                }
                if (mList.Contains(method))
                {
                    continue;
                }
                rst.Add(method);
            }
            return rst.ToArray();
        }

        private PropertyInfo[] GetPropertyExcludeIndexer(Type type, BindingFlags bindingAttr)
        {
            PropertyInfo[] proArray = m_InvokedType.GetProperties(bindingAttr);
            if (proArray == null || proArray.Length <= 0)
            {
                return new PropertyInfo[0];
            }
            List<PropertyInfo> list = new List<PropertyInfo>(proArray);
            return list.FindAll(p => p.Name != "Item").ToArray();
        }

        private PropertyInfo[] GetIndexer(Type type, BindingFlags bindingAttr)
        {
            PropertyInfo[] proArray = m_InvokedType.GetProperties(bindingAttr);
            if (proArray == null || proArray.Length <= 0)
            {
                return new PropertyInfo[0];
            }
            List<PropertyInfo> list = new List<PropertyInfo>(proArray);
            return list.FindAll(p => p.Name == "Item").ToArray();
        }

        #endregion
    }
}
