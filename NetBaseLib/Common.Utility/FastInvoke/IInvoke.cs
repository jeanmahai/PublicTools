using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utility
{
    public interface IInvoke
    {
        object CreateInstance(params object[] parameters);

        object MethodInvoke(object obj, string methodName, params object[] parameters);

        object PropertyGet(object obj, string propertyName);

        void PropertySet(object obj, string propertyName, object value);

        string GetPropertyNameIgnoreCase(string propertyName);

        Type GetPropertyType(string propertyName);

        bool ExistPropertyOrIndexerSet(string propertyName, params Type[] parameterTypeList);

        bool ExistPropertyOrIndexerGet(string propertyName, params Type[] parameterTypeList);

        Type GetIndexerType(params Type[] parameterTypeList);

        object IndexerGet(object obj, params object[] indexerParameters);

        void IndexerSet(object obj, object value, params object[] indexerParameters);
    }
}
