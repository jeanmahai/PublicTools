using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Common.Utility
{
    public static class DataMapper
    {
        // ------------- Auto Map Return Entity ------------

        #region 从IDataReader获取数据

        /// <summary>
        /// 循环遍历IDataReader来获取数据，填充返回对象实例的集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <typeparam name="C">实体类型的泛型集合类型，需要继承自ICollection<T></typeparam>
        /// <param name="reader">读取数据库获得的数据读取器</param>
        /// <returns>对象实例的集合，如果没有数据则返回Count=0的集合</returns>
        public static C GetEntityList<T, C>(IDataReader reader, Action<IDataReader, T> manualMapper = null)
            where T : class, new()
            where C : ICollection<T>, new()
        {
            return GetEntityList<T, C>(reader, true, true, manualMapper);
        }

        /// <summary>
        /// 循环遍历IDataReader来获取数据，填充返回对象实例的集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <typeparam name="C">实体类型的泛型集合类型，需要继承自ICollection<T></typeparam>
        /// <param name="reader">读取数据库获得的数据读取器</param>
        /// <param name="propertyNameIgnoreCase">根据所获取的SQL结果集中各个列的名字去寻找实体类型中同名属性时，是否区分大小写，true则不区分大小写，默认为true。</param>
        /// <param name="skipNotExistProperty">当SQL结果集中某个列的名字在实体类型中找不到对应的同名属性时，是否自动跳过，true则自动跳过，否则抛出异常，默认为true。</param>
        /// <returns>对象实例的集合，如果没有数据则返回Count=0的集合</returns>
        public static C GetEntityList<T, C>(IDataReader reader, bool propertyNameIgnoreCase, bool skipNotExistProperty, Action<IDataReader, T> manualMapper = null)
            where T : class, new()
            where C : ICollection<T>, new()
        {
            return GetEntityList<T, C>(reader, propertyNameIgnoreCase, skipNotExistProperty, '.', manualMapper);
        }

        /// <summary>
        /// 循环遍历IDataReader来获取数据，填充返回对象实例的集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <typeparam name="C">实体类型的泛型集合类型，需要继承自ICollection<T></typeparam>
        /// <param name="reader">读取数据库获得的数据读取器</param>
        /// <param name="propertyNameIgnoreCase">根据所获取的SQL结果集中各个列的名字去寻找实体类型中同名属性时，是否区分大小写，true则不区分大小写，默认为true。</param>
        /// <param name="skipNotExistProperty">当SQL结果集中某个列的名字在实体类型中找不到对应的同名属性时，是否自动跳过，true则自动跳过，否则抛出异常，默认为true。</param>
        /// <param name="splitter">SQL结果集中某个列的名字上区分对象属性级别的分隔符，默认为'.'</param>
        /// <returns>对象实例的集合，如果没有数据则返回Count=0的集合</returns>
        public static C GetEntityList<T, C>(IDataReader reader, bool propertyNameIgnoreCase, bool skipNotExistProperty, char splitter, Action<IDataReader, T> manualMapper = null)
            where T : class, new()
            where C : ICollection<T>, new()
        {
            C list = new C();
            bool hasData;
            do
            {
                T entity;
                hasData = TryGetEntity<T>(reader, out entity, propertyNameIgnoreCase, skipNotExistProperty, splitter, manualMapper);
                if (hasData && entity != null)
                {
                    list.Add(entity);
                }
            } while (hasData);
            return list;
        }

        /// <summary>
        /// 根据IDataReader来获取泛型参数T所指定的类型的对象实例，并将IDataReader中的数据填充到该对象实例中；请注意：方法内部会自动调用IDataReader reader.Read()方法
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="reader">读取数据库获得的数据读取器</param>
        /// <param name="entity">泛型参数T所指定的类型的对象实例</param>
        /// <returns>IDataReader中是否有数据，等同于IDataReader reader.Read()方法的返回值，如果为false，则out参数entity为null</returns>
        public static bool TryGetEntity<T>(IDataReader reader, out T entity, Action<IDataReader, T> manualMapper = null) where T : class, new()
        {
            return TryGetEntity<T>(reader, out entity, true, true, manualMapper);
        }

        /// <summary>
        /// 根据IDataReader来获取泛型参数T所指定的类型的对象实例，并将IDataReader中的数据填充到该对象实例中；请注意：方法内部会自动调用IDataReader reader.Read()方法
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="reader">读取数据库获得的数据读取器</param>
        /// <param name="entity">泛型参数T所指定的类型的对象实例</param>
        /// <param name="propertyNameIgnoreCase">根据所获取的SQL结果集中各个列的名字去寻找实体类型中同名属性时，是否区分大小写，true则不区分大小写，默认为true。</param>
        /// <param name="skipNotExistProperty">当SQL结果集中某个列的名字在实体类型中找不到对应的同名属性时，是否自动跳过，true则自动跳过，否则抛出异常，默认为true。</param>
        /// <returns>泛型参数T所指定的类型的对象实例</returns>
        public static bool TryGetEntity<T>(IDataReader reader, out T entity, bool propertyNameIgnoreCase, bool skipNotExistProperty, Action<IDataReader, T> manualMapper = null) where T : class, new()
        {
            return TryGetEntity<T>(reader, out entity, propertyNameIgnoreCase, skipNotExistProperty, '.', manualMapper);
        }

        /// <summary>
        /// 根据IDataReader来获取泛型参数T所指定的类型的对象实例，并将IDataReader中的数据填充到该对象实例中；请注意：方法内部会自动调用IDataReader reader.Read()方法
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="reader">读取数据库获得的数据读取器</param>
        /// <param name="entity">泛型参数T所指定的类型的对象实例</param>
        /// <param name="propertyNameIgnoreCase">根据所获取的SQL结果集中各个列的名字去寻找实体类型中同名属性时，是否区分大小写，true则不区分大小写，默认为true。</param>
        /// <param name="skipNotExistProperty">当SQL结果集中某个列的名字在实体类型中找不到对应的同名属性时，是否自动跳过，true则自动跳过，否则抛出异常，默认为true。</param>
        /// <param name="splitter">SQL结果集中某个列的名字上区分对象属性级别的分隔符，默认为'.'</param>
        /// <returns>泛型参数T所指定的类型的对象实例</returns>
        public static bool TryGetEntity<T>(IDataReader reader, out T entity, bool propertyNameIgnoreCase, bool skipNotExistProperty, char splitter, Action<IDataReader, T> manualMapper = null) where T : class, new()
        {
            if (reader.Read() == false)
            {
                entity = null;
                return false;
            }
            entity = new T();
            NoReadAutoMap(entity, reader, propertyNameIgnoreCase, skipNotExistProperty, splitter, manualMapper);
            return true;
        }

        /// <summary>
        /// 请注意：方法内部会自动调用IDataReader reader.Read()方法
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="obj">待填充的实体对象实例</param>
        /// <param name="reader">读取数据库获得的数据读取器</param>
        /// <returns>等同于IDataReader reader.Read()方法的返回值，表示是否有数据填充进obj对象实例</returns>
        public static bool AutoMap<T>(T obj, IDataReader reader, Action<IDataReader, T> manualMapper = null) where T : class
        {
            return AutoMap<T>(obj, reader, true, true, manualMapper);
        }

        /// <summary>
        /// 请注意：方法内部会自动调用IDataReader reader.Read()方法
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="obj">待填充的实体对象实例</param>
        /// <param name="reader">读取数据库获得的数据读取器</param>
        /// <param name="propertyNameIgnoreCase">根据所获取的SQL结果集中各个列的名字去寻找实体类型中同名属性时，是否区分大小写，true则不区分大小写，默认为true。</param>
        /// <param name="skipNotExistProperty">当SQL结果集中某个列的名字在实体类型中找不到对应的同名属性时，是否自动跳过，true则自动跳过，否则抛出异常，默认为true。</param>
        /// <returns>等同于IDataReader reader.Read()方法的返回值，表示是否有数据填充进obj对象实例</returns>
        public static bool AutoMap<T>(T obj, IDataReader reader, bool propertyNameIgnoreCase, bool skipNotExistProperty, Action<IDataReader, T> manualMapper = null) where T : class
        {
            return AutoMap<T>(obj, reader, propertyNameIgnoreCase, skipNotExistProperty, '.', manualMapper);
        }

        /// <summary>
        /// 请注意：方法内部会自动调用IDataReader reader.Read()方法
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="obj">待填充的实体对象实例</param>
        /// <param name="reader">读取数据库获得的数据读取器</param>
        /// <param name="propertyNameIgnoreCase">根据所获取的SQL结果集中各个列的名字去寻找实体类型中同名属性时，是否区分大小写，true则不区分大小写，默认为true。</param>
        /// <param name="skipNotExistProperty">当SQL结果集中某个列的名字在实体类型中找不到对应的同名属性时，是否自动跳过，true则自动跳过，否则抛出异常，默认为true。</param>
        /// <param name="splitter">SQL结果集中某个列的名字上区分对象属性级别的分隔符，默认为'.'</param>
        /// <returns>等同于IDataReader reader.Read()方法的返回值，表示是否有数据填充进obj对象实例</returns>
        public static bool AutoMap<T>(T obj, IDataReader reader, bool propertyNameIgnoreCase, bool skipNotExistProperty, char splitter, Action<IDataReader, T> manualMapper = null) where T : class
        {
            if (reader.Read() == false)
            {
                return false;
            }
            NoReadAutoMap<T>(obj, reader, propertyNameIgnoreCase, skipNotExistProperty, splitter, manualMapper);
            return true;
        }

        /// <summary>
        /// 使用这个方法，需要在方法外先调用IDataReader reader的Read()方法后再将reader传入方法
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="obj">待填充的实体对象实例</param>
        /// <param name="reader">读取数据库获得的数据读取器</param>
        /// <param name="propertyNameIgnoreCase">根据所获取的SQL结果集中各个列的名字去寻找实体类型中同名属性时，是否区分大小写，true则不区分大小写，默认为true。</param>
        /// <param name="skipNotExistProperty">当SQL结果集中某个列的名字在实体类型中找不到对应的同名属性时，是否自动跳过，true则自动跳过，否则抛出异常，默认为true。</param>
        /// <param name="splitter">SQL结果集中某个列的名字上区分对象属性级别的分隔符，默认为'.'</param>
        public static void NoReadAutoMap<T>(T obj, IDataReader reader, bool propertyNameIgnoreCase,
            bool skipNotExistProperty, char splitter, Action<IDataReader, T> manualMapper = null) where T : class
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string[] pNames = reader.GetName(i).Split(new char[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
                object val = DataMapper.ConvertIfEnum(reader[i], typeof(T), pNames, propertyNameIgnoreCase, skipNotExistProperty);
                FillFieldValue(obj, pNames, val, propertyNameIgnoreCase, skipNotExistProperty);
            }
            if (manualMapper != null)
            {
                manualMapper(reader, obj);
            }
        }

        #endregion

        #region 从DataRow获取数据

        /// <summary>
        /// 循环遍历DataRowCollection来获取数据，填充返回对象实例的集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <typeparam name="C">实体类型的泛型集合类型，需要继承自ICollection<T></typeparam>
        /// <param name="rows">读取数据库获得的数据读取器</param>
        /// <returns>对象实例的集合，如果没有数据则返回Count=0的集合</returns>
        public static C GetEntityList<T, C>(DataRowCollection rows, Action<DataRow, T> manualMapper = null)
            where T : class, new()
            where C : ICollection<T>, new()
        {
            return GetEntityList<T, C>(rows, true, true, manualMapper);
        }

        /// <summary>
        /// 循环遍历DataRowCollection来获取数据，填充返回对象实例的集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <typeparam name="C">实体类型的泛型集合类型，需要继承自ICollection<T></typeparam>
        /// <param name="rows">读取数据库获得的数据读取器</param>
        /// <param name="propertyNameIgnoreCase">根据所获取的DataRow数据集中各个列的名字去寻找实体类型中同名属性时，是否区分大小写，true则不区分大小写，默认为true。</param>
        /// <param name="skipNotExistProperty">当DataRow数据集中某个列的名字在实体类型中找不到对应的同名属性时，是否自动跳过，true则自动跳过，否则抛出异常，默认为true。</param>
        /// <returns>对象实例的集合，如果没有数据则返回Count=0的集合</returns>
        public static C GetEntityList<T, C>(DataRowCollection rows, bool propertyNameIgnoreCase, bool skipNotExistProperty, Action<DataRow, T> manualMapper = null)
            where T : class, new()
            where C : ICollection<T>, new()
        {
            return GetEntityList<T, C>(rows, propertyNameIgnoreCase, skipNotExistProperty, '.', manualMapper);
        }

        /// <summary>
        /// 循环遍历DataRowCollection来获取数据，填充返回对象实例的集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <typeparam name="C">实体类型的泛型集合类型，需要继承自ICollection<T></typeparam>
        /// <param name="rows">读取数据库获得的数据读取器</param>
        /// <param name="propertyNameIgnoreCase">根据所获取的DataRow数据集中各个列的名字去寻找实体类型中同名属性时，是否区分大小写，true则不区分大小写，默认为true。</param>
        /// <param name="skipNotExistProperty">当DataRow数据集中某个列的名字在实体类型中找不到对应的同名属性时，是否自动跳过，true则自动跳过，否则抛出异常，默认为true。</param>
        /// <param name="splitter">SQL结果集中某个列的名字上区分对象属性级别的分隔符，默认为'.'</param>
        /// <returns>对象实例的集合，如果没有数据则返回Count=0的集合</returns>
        public static C GetEntityList<T, C>(DataRowCollection rows, bool propertyNameIgnoreCase, bool skipNotExistProperty, char splitter, Action<DataRow, T> manualMapper = null)
            where T : class, new()
            where C : ICollection<T>, new()
        {
            C list = new C();
            if (rows == null || rows.Count <= 0)
            {
                return list;
            }
            foreach (DataRow row in rows)
            {
                list.Add(GetEntity<T>(row, propertyNameIgnoreCase, skipNotExistProperty, splitter, manualMapper));
            }
            return list;
        }

        /// <summary>
        /// 根据DataRow来获取泛型参数T所指定的类型的对象实例，并将DataRow中的数据填充到该对象实例中
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="row">DataRow类型的数据来源</param>
        /// <returns>实体的对象实例</returns>
        public static T GetEntity<T>(DataRow row, Action<DataRow, T> manualMapper = null) where T : class, new()
        {
            return GetEntity<T>(row, true, true, manualMapper);
        }

        /// <summary>
        /// 根据DataRow来获取泛型参数T所指定的类型的对象实例，并将DataRow中的数据填充到该对象实例中
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="row">DataRow类型的数据来源</param>
        /// <param name="propertyNameIgnoreCase">根据所获取的SQL结果集中各个列的名字去寻找实体类型中同名属性时，是否区分大小写，true则不区分大小写，默认为true。</param>
        /// <param name="skipNotExistProperty">当SQL结果集中某个列的名字在实体类型中找不到对应的同名属性时，是否自动跳过，true则自动跳过，否则抛出异常，默认为true。</param>
        /// <returns>实体的对象实例</returns>
        public static T GetEntity<T>(DataRow row, bool propertyNameIgnoreCase, bool skipNotExistProperty, Action<DataRow, T> manualMapper = null) where T : class, new()
        {
            return GetEntity<T>(row, propertyNameIgnoreCase, skipNotExistProperty, '.', manualMapper);
        }

        /// <summary>
        /// 根据DataRow来获取泛型参数T所指定的类型的对象实例，并将DataRow中的数据填充到该对象实例中
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="row">DataRow类型的数据来源</param>
        /// <param name="propertyNameIgnoreCase">根据所获取的SQL结果集中各个列的名字去寻找实体类型中同名属性时，是否区分大小写，true则不区分大小写，默认为true。</param>
        /// <param name="skipNotExistProperty">当SQL结果集中某个列的名字在实体类型中找不到对应的同名属性时，是否自动跳过，true则自动跳过，否则抛出异常，默认为true。</param>
        /// <param name="splitter">SQL结果集中某个列的名字上区分对象属性级别的分隔符，默认为'.'</param>
        /// <returns>实体的对象实例</returns>
        public static T GetEntity<T>(DataRow row, bool propertyNameIgnoreCase, bool skipNotExistProperty, char splitter, Action<DataRow, T> manualMapper = null) where T : class, new()
        {
            T rst = new T();
            AutoMap<T>(rst, row, propertyNameIgnoreCase, skipNotExistProperty, splitter, manualMapper);
            return rst;
        }

        /// <summary>
        /// 根据DataRow的数据来填充实体属性
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="obj">待填充的实体对象实例</param>
        /// <param name="row">读取数据库获得的数据读取器</param>
        /// <param name="propertyNameIgnoreCase">根据所获取的DataRow数据集中各个列的名字去寻找实体类型中同名属性时，是否区分大小写，true则不区分大小写，默认为true。</param>
        /// <param name="skipNotExistProperty">当DataRow数据集中某个列的名字在实体类型中找不到对应的同名属性时，是否自动跳过，true则自动跳过，否则抛出异常，默认为true。</param>
        public static void AutoMap<T>(T obj, DataRow row, Action<DataRow, T> manualMapper = null) where T : class
        {
            AutoMap<T>(obj, row, true, true, '.', manualMapper);
        }

        /// <summary>
        /// 根据DataRow的数据来填充实体属性
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="obj">待填充的实体对象实例</param>
        /// <param name="row">读取数据库获得的数据读取器</param>
        /// <param name="propertyNameIgnoreCase">根据所获取的DataRow数据集中各个列的名字去寻找实体类型中同名属性时，是否区分大小写，true则不区分大小写，默认为true。</param>
        /// <param name="skipNotExistProperty">当DataRow数据集中某个列的名字在实体类型中找不到对应的同名属性时，是否自动跳过，true则自动跳过，否则抛出异常，默认为true。</param>
        public static void AutoMap<T>(T obj, DataRow row, bool propertyNameIgnoreCase, bool skipNotExistProperty, Action<DataRow, T> manualMapper = null) where T : class
        {
            AutoMap<T>(obj, row, propertyNameIgnoreCase, skipNotExistProperty, '.', manualMapper);
        }

        /// <summary>
        /// 根据DataRow的数据来填充实体属性
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="obj">待填充的实体对象实例</param>
        /// <param name="row">读取数据库获得的数据读取器</param>
        /// <param name="propertyNameIgnoreCase">根据所获取的DataRow数据集中各个列的名字去寻找实体类型中同名属性时，是否区分大小写，true则不区分大小写，默认为true。</param>
        /// <param name="skipNotExistProperty">当DataRow数据集中某个列的名字在实体类型中找不到对应的同名属性时，是否自动跳过，true则自动跳过，否则抛出异常，默认为true。</param>
        /// <param name="splitter">SQL结果集中某个列的名字上区分对象属性级别的分隔符，默认为'.'</param>
        public static void AutoMap<T>(T obj, DataRow row, bool propertyNameIgnoreCase, bool skipNotExistProperty, char splitter, Action<DataRow, T> manualMapper = null) where T : class
        {
            for (int i = 0; i < row.Table.Columns.Count; i++)
            {
                string[] pNames = row.Table.Columns[i].ColumnName.Split(new char[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
                object val = DataMapper.ConvertIfEnum(row[i], typeof(T), pNames, propertyNameIgnoreCase, skipNotExistProperty);
                FillFieldValue(obj, pNames, val, propertyNameIgnoreCase, skipNotExistProperty);
            }
            if (manualMapper != null)
            {
                manualMapper(row, obj);
            }
        }

        #endregion

        // ------------- Assigned Map Return Entity ------------

        public static void AssignedMap<T>(T obj, IDataReader reader, ReturnMap<T> rMap) where T : class
        {
            AssignedMap<T>(obj, reader, rMap, true, true);
        }

        public static void AssignedMap<T>(T obj, IDataReader reader, ReturnMap<T> rMap, bool propertyNameIgnoreCase, bool skipNotExistProperty) where T : class
        {
            if (rMap == null)
            {
                return;
            }
            List<PropertyColumn> maps = rMap.GetMaps();
            if (maps == null || maps.Count <= 0)
            {
                return;
            }
            foreach (PropertyColumn map in maps)
            {
                object val = reader[map.Column];
                if (map.PropertyList == null || map.PropertyList.Count <= 0)
                {
                    continue;
                }
                else
                {
                    string[] pNames = map.PropertyList.ToArray();
                    val = DataMapper.ConvertIfEnum(val, typeof(T), pNames, propertyNameIgnoreCase, skipNotExistProperty);
                    FillFieldValue(obj, pNames, val, propertyNameIgnoreCase, skipNotExistProperty);
                }
            }
        }

        public static void AssignedMap<T>(T obj, DataRow row, ReturnMap<T> rMap) where T : class
        {
            AssignedMap<T>(obj, row, rMap, true, true);
        }

        public static void AssignedMap<T>(T obj, DataRow row, ReturnMap<T> rMap, bool propertyNameIgnoreCase, bool skipNotExistProperty) where T : class
        {
            if (rMap == null)
            {
                return;
            }
            List<PropertyColumn> maps = rMap.GetMaps();
            if (maps == null || maps.Count <= 0)
            {
                return;
            }
            foreach (PropertyColumn map in maps)
            {
                object val = row[map.Column];
                if (map.PropertyList == null || map.PropertyList.Count <= 0)
                {
                    continue;
                }
                else
                {
                    string[] pNames = map.PropertyList.ToArray();
                    val = DataMapper.ConvertIfEnum(val, typeof(T), pNames, propertyNameIgnoreCase, skipNotExistProperty);
                    FillFieldValue(obj, pNames, val, propertyNameIgnoreCase, skipNotExistProperty);
                }
            }
        }

        // -------------------------------------------------------------------------------------------

        public static DbParameter[] BuildDbParameters<T, P>(string cmdText, T entity)
            where T : class
            where P : DbParameter, new()
        {
            return BuildDbParameters<T, P>(cmdText, entity, null);
        }

        public static DbParameter[] BuildDbParameters<T, P>(string cmdText, T entity, InputMap<T> map)
            where T : class
            where P : DbParameter, new()
        {
            return BuildDbParameters<T, P>(cmdText, entity, map, '_');
        }

        public static DbParameter[] BuildDbParameters<T, P>(string cmdText, T entity, InputMap<T> map, char sqlParamSplitter)
            where T : class
            where P : DbParameter, new()
        {
            return BuildDbParameters<T>(cmdText, entity, () => new P(), map, sqlParamSplitter);
        }

        public static DbParameter[] BuildDbParameters<T>(string cmdText, T entity, Func<DbParameter> dbParamConstructor)
            where T : class
        {
            return BuildDbParameters<T>(cmdText, entity, dbParamConstructor, null, '_');
        }

        public static DbParameter[] BuildDbParameters<T>(string cmdText, T entity, Func<DbParameter> dbParamConstructor, InputMap<T> map)
            where T : class
        {
            return BuildDbParameters<T>(cmdText, entity, dbParamConstructor, map, '_');
        }

        public static DbParameter[] BuildDbParameters<T>(string cmdText, T entity, Func<DbParameter> dbParamConstructor, InputMap<T> map, char sqlParamSplitter)
            where T : class
        {
            List<string> list = Analyst.GetSqlParamNameList(cmdText);
            List<ParameterProperty<T>> maps = (map == null) ? null : map.GetMaps();
            if (maps == null)
            {
                maps = new List<ParameterProperty<T>>(0);
            }
            List<DbParameter> pList = new List<DbParameter>(list.Count);

            foreach (string pName in list)
            {
                DbParameter parm = dbParamConstructor();
                parm.ParameterName = pName;
                ParameterProperty<T> pp = maps.Find(p => p.Parameter == pName);

                string[] proArray;
                if (pp != null)
                {
                    proArray = pp.PropertyList.ToArray();
                }
                else
                {
                    proArray = pName.Split(sqlParamSplitter);
                }
                object tmp = GetFieldValueAllowNull(entity, proArray);
                parm.Value = (tmp == null ? DBNull.Value : tmp);
                pList.Add(parm);
            }
            return pList.ToArray();
        }

        #region public 辅助方法

        public static object GetFieldValueAllowNull(object entity, string[] proArray)
        {
            return GetFieldValueAllowNull(entity, proArray, false, true);
        }

        public static object GetFieldValueAllowNull(object entity, string[] proArray, bool ignoreCase, bool throwOnPropertyNotExist)
        {
            if (entity == null)
            {
                return null;
            }
            if (proArray == null || proArray.Length <= 0)
            {
                return null;
            }
            object obj = (object)entity;
            for (int i = 0; i < proArray.Length; i++)
            {
                if (obj == null)
                {
                    return null;
                }
                if (i == proArray.Length - 1)
                {
                    return Invoker.PropertyGet(obj, proArray[i], ignoreCase, throwOnPropertyNotExist);
                }
                else
                {
                    obj = Invoker.PropertyGet(obj, proArray[i], ignoreCase, throwOnPropertyNotExist);
                }
            }
            return null;
        }

        public static void FillFieldValue(object entity, string[] pNames, object val, bool propertyNameIgnoreCase, bool skipNotExistProperty)
        {
            if (entity == null)
            {
                return;
            }
            if (pNames == null || pNames.Length <= 0)
            {
                return;
            }
            object pro = (object)entity;
            int index = 0;
            foreach (string propertyName in pNames)
            {
                if (!Invoker.ExistPropertySet(pro.GetType(), propertyName, propertyNameIgnoreCase))
                {
                    if (!skipNotExistProperty)
                    {
                        throw new ApplicationException("There is no public instance property that can be set '" + propertyName + "' in type '" + pro.GetType().FullName + "'");
                    }
                    break;
                }
                // 根据property的值（不区分大小写）找到在pro对象的类型中的属性的名称
                string realName = Invoker.GetPropertyNameIgnoreCase(pro.GetType(), propertyName);
                if (realName == null || (realName != propertyName && !propertyNameIgnoreCase))
                // realName == null 说明pro对象的类型中不存在名为property变量值的属性
                // realName != propertyName 说明存在属性，但属性名与输入的值的大小写不一致
                {
                    if (!skipNotExistProperty)
                    {
                        throw new ApplicationException("There is no public instance property that can be set '" + propertyName + "' in type '" + pro.GetType().FullName + "'");
                    }
                    break;
                }
                if (index == pNames.Length - 1)
                {
                    Invoker.PropertySet(pro, realName, val);
                }
                else
                {
                    object tmp = null;
                    if (Invoker.ExistPropertyGet(pro.GetType(), realName))
                    {
                        tmp = Invoker.PropertyGet(pro, realName);
                    }
                    if (tmp == null)
                    {
                        Type type = Invoker.GetPropertyType(pro.GetType(), realName, false, false);
                        tmp = Invoker.CreateInstance(type);
                        Invoker.PropertySet(pro, realName, tmp);
                    }
                    pro = tmp;
                }
                index++;
            }
        }

        public static Type GetPropertyType(Type type, string[] pNames, bool propertyNameIgnoreCase, bool skipNotExistProperty)
        {
            bool throwOnNotExist = !skipNotExistProperty;
            Type pro = type;
            foreach (string propertyName in pNames)
            {
                pro = Invoker.GetPropertyType(pro, propertyName, propertyNameIgnoreCase, throwOnNotExist);
                if (pro == null)
                {
                    return null;
                }
            }
            return pro;
        }

        /// <summary>
        /// 如果入参type为枚举，那么把入参value按照枚举的mapping关系转换为对应的枚举值再返回，否则就直接返回value
        /// </summary>
        public static object ConvertIfEnum(object value, Type type)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }
            while (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && type.GetGenericArguments() != null
                    && type.GetGenericArguments().Length == 1 && type.GetGenericArguments()[0].IsEnum)
            {
                type = type.GetGenericArguments()[0];
            }

            if (type.IsEnum)
            {
                if (value.ToString().Trim().Length <= 0)
                {
                    return null;
                }
                //[[Modified by Poseidon.y.tong at [2012-08-29 10:24:22]
                //由于数据库中某些字段设置的是char(n)类型，如果数据库实际存放的值长度＜n，读取出来的值末尾会补空格，导致EnumCodeMapper映射不上而返回null
                //在执行Enum.Parse(type, value.ToString())时，如果value值和Enum.ToString()的值不一致，会导致报错。
                value = value.ToString().Trim();
                //]]

                object rst;
                EnumCodeMapper.TryGetEnum(value, type, out rst);
                if (rst == null)
                {
                    return Enum.Parse(type, value.ToString());
                }
                return rst;
            }
            return value;
        }

        /// <summary>
        /// 检查入参type的属性pNames是否为枚举，如果是，则把入参value按照枚举的mapping关系转换为对应的枚举值再返回，否则就直接返回value
        /// </summary>
        public static object ConvertIfEnum(object value, Type type, string[] pNames, bool propertyNameIgnoreCase, bool skipNotExistProperty)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }
            Type pType = DataMapper.GetPropertyType(type, pNames, propertyNameIgnoreCase, skipNotExistProperty);
            if (pType == null)
            {
                return value;
            }
            return ConvertIfEnum(value, pType);
        }
        #endregion
    }
}
