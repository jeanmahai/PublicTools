using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Soho.Utility.DataAccess
{
    /// <summary>
    /// This class can help you build a sql for dynamic query.
    /// When use this class, PLEASE BE SURE your dynamic query sql template is like this:
    /// 
    /// SELECT @TotalCount = COUNT(a.xxx) 
    /// FROM dbo.xxx a WITH(NOLOCK) 
    /// #StrWhere# 
    /// SELECT xxx
    /// FROM
    /// (
    ///     SELECT TOP (@EndNumber)
    ///         xxx,
    ///         (ROW_NUMBER() OVER(ORDER BY #SortColumnName#)) AS RowNumber 
    ///     FROM dbo.xxx a WITH(NOLOCK) 
    ///     #StrWhere#
    /// ) Result
    /// WHERE RowNumber > @StartNumber
    /// 
    /// MAKE SURE the sql parameter name of this template CAN NOT be change.
    /// </summary>
    public class DynamicQuerySqlBuilder : IDisposable
    {
        private const int Default_PageSize = 1000;

        private ConditionConstructor m_conditionConstructor;
        private PagingInfoEntity m_pagingInfo;
        private CustomDataCommand m_dataCommand;
        private string m_querySqlTemplate;
        private string m_defaultOrderBy;
        private bool m_needPaging = true;

        public DynamicQuerySqlBuilder(CustomDataCommand dataCommand)
            : this(dataCommand.CommandText, dataCommand, new PagingInfoEntity() { StartRowIndex = 0, MaximumRows = 5000, SortField = string.Empty }, string.Empty)
        {
            this.m_needPaging = false;
        }

        public DynamicQuerySqlBuilder(CustomDataCommand dataCommand, string defaultOrderBy)
            : this(dataCommand.CommandText, dataCommand, new PagingInfoEntity() { StartRowIndex = 0, MaximumRows = 5000, SortField = string.Empty }, defaultOrderBy)
        {

        }

        public DynamicQuerySqlBuilder(CustomDataCommand dataCommand,
            PagingInfoEntity pagingInfo, string defaultOrderBy)
            : this(dataCommand.CommandText, dataCommand, pagingInfo, defaultOrderBy)
        {

        }

        public DynamicQuerySqlBuilder(
            string querySqlTemplate, CustomDataCommand dataCommand,
            PagingInfoEntity pagingInfo, string defaultOrderBy)
        {
            m_pagingInfo = pagingInfo;
            m_dataCommand = dataCommand;
            m_querySqlTemplate = querySqlTemplate;
            m_defaultOrderBy = defaultOrderBy;
            m_conditionConstructor = new ConditionConstructor();
        }

        private string BuildOrderByString()
        {
            string orderByString = m_defaultOrderBy;
            if (m_needPaging)
            {
                if (m_pagingInfo != null && !ConditionConstructor.IsStringNullOrEmpty(m_pagingInfo.SortField))
                {
                    orderByString = m_pagingInfo.SortField;
                }
                if (ConditionConstructor.IsStringNullOrEmpty(orderByString))
                {
                    throw new ApplicationException("Daynamic query must have one OrderBy field at least.");
                }
            }
            return orderByString;
        }

        private void SetPagingInformation()
        {
            if (m_needPaging)
            {
                int pageSize = Default_PageSize;
                int startIndex = 0;
                if (m_pagingInfo != null)
                {
                    if (m_pagingInfo.MaximumRows.HasValue)
                    {
                        pageSize = m_pagingInfo.MaximumRows.Value;
                    }
                    if (m_pagingInfo.StartRowIndex.HasValue)
                    {
                        startIndex = (m_pagingInfo.StartRowIndex.Value);
                    }
                }
                m_dataCommand.AddInputParameter("@EndNumber", DbType.Int32, pageSize + startIndex);
                m_dataCommand.AddInputParameter("@StartNumber", DbType.Int32, startIndex);
                m_dataCommand.AddOutParameter("@TotalCount", DbType.Int32, 4);
            }
        }

        public string BuildQuerySql()
        {
            // Build Query Condition
            ConditionCollectionContext buildContext = new ConditionCollectionContext(m_dataCommand);
            string result = m_querySqlTemplate.Replace("#StrWhere#", m_conditionConstructor.BuildQuerySqlConditionString(buildContext));
            // Build OrderBy String
            result = result.Replace("#SortColumnName#", BuildOrderByString());
            // Set Paging Information
            SetPagingInformation();
            return result;
        }

        public ConditionConstructor ConditionConstructor
        {
            get { return m_conditionConstructor; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                if (m_pagingInfo != null)
                {
                    object totalCount = m_dataCommand.GetParameterValue("@TotalCount");
                    if (totalCount != null && totalCount != DBNull.Value)
                    {
                        m_pagingInfo.TotalCount = Convert.ToInt32(totalCount);
                    }
                }
            }
            catch
            {
            }
        }

        #endregion
    }

    #region Condition Handler Helper

    public class GroupCondition : IDisposable
    {
        private DynamicQuerySqlBuilder m_builder;

        public GroupCondition(DynamicQuerySqlBuilder builder, QueryConditionRelationType groupRelationType)
        {
            m_builder = builder;
            m_builder.ConditionConstructor.BeginGroupCondition(groupRelationType);
        }

        #region IDisposable Members

        public void Dispose()
        {
            m_builder.ConditionConstructor.EndGroupCondition();
        }

        #endregion
    }

    public class ConditionConstructor
    {
        private List<Condition> m_conditions;

        internal static bool IsStringNullOrEmpty(string value)
        {
            if (value == null || value.Trim() == string.Empty)
            {
                return true;
            }
            return false;
        }

        internal static bool DefaultParameterValueValidationCheck(object parameterValue)
        {
            // If parameter value is null, it's type is ReferenceType or Nullable<T> type.
            if (parameterValue == null)
            {
                return false;
            }
            Type parameterType = parameterValue.GetType();
            if (parameterType.Equals(typeof(string)) && IsStringNullOrEmpty(parameterValue.ToString()))
            {
                return false;
            }
            return true;
        }

        internal ConditionConstructor()
        {
            m_conditions = new List<Condition>();
        }

        internal ConditionConstructor(List<Condition> conditions)
        {
            m_conditions = conditions;
        }

        private void AddInOrNotInCondition(QueryConditionOperatorType operationType,
            QueryConditionRelationType conditionRelationType, string fieldName, DbType listValueDbType, List<Object> inValues)
        {
            if (operationType != QueryConditionOperatorType.In && operationType != QueryConditionOperatorType.NotIn)
            {
                throw new ArgumentException("Operation Type must be 'In' or 'NotIn'.");
            }
            AddCondition(conditionRelationType, fieldName, listValueDbType, null,
                operationType, inValues, value => { return inValues != null && inValues.Count > 0; });
        }

        private void AddInOrNotInCondition<TListValueType>(QueryConditionOperatorType operationType,
            QueryConditionRelationType conditionRelationType, string fieldName, DbType listValueDbType, List<TListValueType> inValues)
        {
            List<Object> convertedInValues = new List<object>();
            if (inValues != null)
            {
                foreach (TListValueType element in inValues)
                {
                    convertedInValues.Add(element);
                }
            }
            AddInOrNotInCondition(operationType, conditionRelationType, fieldName, listValueDbType, convertedInValues);
        }

        public void AddCondition(
            QueryConditionRelationType conditionRelationType,
            string fieldName, DbType parameterDbType, string parameterName, QueryConditionOperatorType conditionOperatorType,
            object parameterValue, ParameterValueValidateCheckDelegate parameterValueValidateCheckHandler)
        {
            if (parameterValueValidateCheckHandler == null)
            {
                parameterValueValidateCheckHandler = DefaultParameterValueValidationCheck;
            }
            if (parameterValueValidateCheckHandler(parameterValue))
            {
                m_conditions.Add(new SqlCondition
                {
                    ConditionRelationType = conditionRelationType,
                    ParameterDbType = parameterDbType,
                    FieldName = fieldName,
                    ParameterName = parameterName,
                    OperatorType = conditionOperatorType,
                    ParameterValue = parameterValue
                });
            }
        }

        public void BeginGroupCondition(QueryConditionRelationType groupRelationType)
        {
            m_conditions.Add(new LeftBracketCondition() { GroupConditionRelationType = groupRelationType });
        }

        public void EndGroupCondition()
        {
            m_conditions.Add(new RightBraketCondition());
        }

        public void AddCondition(
            QueryConditionRelationType conditionRelationType,
            string fieldName, DbType parameterDbType, string parameterName, QueryConditionOperatorType conditionOperatorType,
            object parameterValue)
        {
            AddCondition(conditionRelationType, fieldName, parameterDbType, parameterName,
                conditionOperatorType, parameterValue, DefaultParameterValueValidationCheck);
        }

        public void AddBetweenCondition(
            QueryConditionRelationType conditionRelationType,
            string fieldName, DbType parameterDbType, string parameterName,
            QueryConditionOperatorType leftConditionOperatorType, QueryConditionOperatorType rightConditionOperatorType,
            object leftParameterValue, object rightParameterValue,
            ParameterValueValidateCheckDelegate parameterValueValidateCheckHandler)
        {
            BeginGroupCondition(conditionRelationType);
            AddCondition(QueryConditionRelationType.AND, fieldName, parameterDbType, parameterName + "_Left",
                leftConditionOperatorType, leftParameterValue, parameterValueValidateCheckHandler);
            AddCondition(QueryConditionRelationType.AND, fieldName, parameterDbType, parameterName + "_Right",
                rightConditionOperatorType, rightParameterValue, parameterValueValidateCheckHandler);
            EndGroupCondition();
        }

        public void AddBetweenCondition(
            QueryConditionRelationType conditionRelationType,
            string fieldName, DbType parameterDbType, string parameterName,
            QueryConditionOperatorType leftConditionOperatorType, QueryConditionOperatorType rightConditionOperatorType,
            object leftParameterValue, object rightParameterValue)
        {
            AddBetweenCondition(conditionRelationType, fieldName, parameterDbType, parameterName,
                leftConditionOperatorType, rightConditionOperatorType, leftParameterValue, rightParameterValue,
                DefaultParameterValueValidationCheck);
        }

        public void AddNullCheckCondition(
            QueryConditionRelationType conditionRelationType, string fieldName, QueryConditionOperatorType conditionOperatorType)
        {
            if (conditionOperatorType != QueryConditionOperatorType.IsNull
                && conditionOperatorType != QueryConditionOperatorType.IsNotNull)
            {
                throw new ArgumentException("Parameter conditionOperatorType must be IsNull or IsNotNull in this method.");
            }
            AddCondition(conditionRelationType, fieldName, DbType.Object, null,
                conditionOperatorType, null, value => { return true; });
        }

        public void AddInCondition(QueryConditionRelationType conditionRelationType,
            string fieldName, DbType listValueDbType, List<Object> inValues)
        {
            AddInOrNotInCondition(QueryConditionOperatorType.In, conditionRelationType, fieldName, listValueDbType, inValues);
        }

        public void AddNotInCondition(QueryConditionRelationType conditionRelationType,
            string fieldName, DbType listValueDbType, List<Object> inValues)
        {
            AddInOrNotInCondition(QueryConditionOperatorType.NotIn, conditionRelationType, fieldName, listValueDbType, inValues);
        }

        public void AddInCondition<TListValueType>(QueryConditionRelationType conditionRelationType,
            string fieldName, DbType listValueDbType, List<TListValueType> inValues)
        {
            AddInOrNotInCondition<TListValueType>(QueryConditionOperatorType.In, conditionRelationType, fieldName, listValueDbType, inValues);
        }

        public void AddNotInCondition<TListValueType>(QueryConditionRelationType conditionRelationType,
            string fieldName, DbType listValueDbType, List<TListValueType> inValues)
        {
            AddInOrNotInCondition<TListValueType>(QueryConditionOperatorType.NotIn, conditionRelationType, fieldName, listValueDbType, inValues);
        }

        public ConditionConstructor AddSubQueryCondition(QueryConditionRelationType conditionRelationType,
            string filedName, QueryConditionOperatorType conditionOperatorType, string subQuerySQLTemplate)
        {
            if (!IsStringNullOrEmpty(subQuerySQLTemplate))
            {
                SubQueryCondition condition = new SubQueryCondition()
                {
                    ConditionRelationType = conditionRelationType,
                    FieldName = filedName,
                    OperatorType = conditionOperatorType,
                    SubQuerySQLTemplate = subQuerySQLTemplate,
                    SubQueryConditions = new List<Condition>()
                };
                m_conditions.Add(condition);
                ConditionConstructor result = new ConditionConstructor(condition.SubQueryConditions);
                return result;
            }
            return null;
        }

        public void AddCustomCondition(QueryConditionRelationType conditionRelationType, string customQueryString)
        {
            m_conditions.Add(new CustomCondition()
            {
                ConditionRelationType = conditionRelationType,
                CustomQueryString = customQueryString
            });
        }

        internal static string BuildQuerySqlConditionString(List<Condition> conditions, ConditionCollectionContext buildContext)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return string.Empty;
            }
            StringBuilder resultBuilder = new StringBuilder();
            resultBuilder.Append("WHERE ");
            foreach (Condition condition in conditions)
            {
                condition.BuildQueryString(resultBuilder, buildContext);
            }
            string result = resultBuilder.ToString().Trim();
            return result == "WHERE" ? string.Empty : result;
        }

        internal string BuildQuerySqlConditionString(ConditionCollectionContext buildContext)
        {
            return BuildQuerySqlConditionString(m_conditions, buildContext);
        }

        internal List<Condition> Conditions
        {
            get { return m_conditions; }
        }

        public int ValidateConditionCount
        {
            get { return m_conditions != null ? m_conditions.Count : 0; }
        }
    }

    #endregion Condition Handler Helper

    #region Condition Classes

    internal class ConditionCollectionContext
    {
        public bool IsFirstCondition { get; set; }

        public Stack<bool> FirstGroupConditionFlags { get; set; }

        public CustomDataCommand DataCommand { get; set; }

        public List<string> AddedParameterNames { get; set; }

        public ConditionCollectionContext(CustomDataCommand contextDataCommand)
        {
            IsFirstCondition = true;
            FirstGroupConditionFlags = new Stack<bool>();
            AddedParameterNames = new List<string>();
            DataCommand = contextDataCommand;
        }
    }

    internal abstract class Condition
    {
        internal virtual void BuildQueryString(StringBuilder queryStringBuilder, ConditionCollectionContext buildContext)
        {
            if (queryStringBuilder == null)
            {
                throw new ArgumentException("Input query string builder can not be null.");
            }
            if (buildContext == null)
            {
                throw new ArgumentException("Input build context can not be null.");
            }
        }

        protected string GetOperatorString(QueryConditionOperatorType operatorType)
        {
            switch (operatorType)
            {
                case QueryConditionOperatorType.Equal:
                    return "=";
                case QueryConditionOperatorType.LessThan:
                    return "<";
                case QueryConditionOperatorType.LessThanOrEqual:
                    return "<=";
                case QueryConditionOperatorType.MoreThan:
                    return ">";
                case QueryConditionOperatorType.MoreThanOrEqual:
                    return ">=";
                case QueryConditionOperatorType.NotEqual:
                    return "<>";
                case QueryConditionOperatorType.Like:
                case QueryConditionOperatorType.LeftLike:
                case QueryConditionOperatorType.RightLike:
                    return "LIKE";
                case QueryConditionOperatorType.IsNull:
                    return "IS NULL";
                case QueryConditionOperatorType.IsNotNull:
                    return "IS NOT NULL";
                case QueryConditionOperatorType.In:
                    return "IN";
                case QueryConditionOperatorType.NotIn:
                    return "NOT IN";
                case QueryConditionOperatorType.Exist:
                    return "EXISTS";
                case QueryConditionOperatorType.NotExist:
                    return "NOT EXISTS";
                default: return string.Empty;
            }
        }

        protected object TryConvertToLikeString(object value, QueryConditionOperatorType type)
        {
            if (value != null && value.GetType().Equals(typeof(string)) && !ConditionConstructor.IsStringNullOrEmpty(value.ToString()))
            {
                if (type == QueryConditionOperatorType.Like)
                {
                    return "%" + value.ToString() + "%";
                }
                else if (type == QueryConditionOperatorType.LeftLike)
                {
                    return value.ToString() + "%";
                }
                else if (type == QueryConditionOperatorType.RightLike)
                {
                    return "%" + value.ToString();
                }
            }
            return value;
        }
    }

    internal class SqlCondition : Condition
    {
        public string FieldName { get; set; }

        public string ParameterName { get; set; }

        public object ParameterValue { get; set; }

        public DbType ParameterDbType { get; set; }

        public QueryConditionOperatorType OperatorType { get; set; }

        public QueryConditionRelationType ConditionRelationType { get; set; }

        private string BuildSqlConditionString(ConditionCollectionContext buildContext)
        {
            string result = string.Empty;
            if (this.OperatorType == QueryConditionOperatorType.IsNull
                || this.OperatorType == QueryConditionOperatorType.IsNotNull)
            {
                result = string.Format(" {0} {1} {2}", buildContext.IsFirstCondition ? string.Empty : this.ConditionRelationType.ToString(),
                    this.FieldName, GetOperatorString(this.OperatorType));
            }
            else if (this.OperatorType == QueryConditionOperatorType.In
                || this.OperatorType == QueryConditionOperatorType.NotIn)
            {
                List<Object> parameterValues = this.ParameterValue as List<Object>;
                StringBuilder parameterNamesBuilder = new StringBuilder();
                for (int i = 0; i < parameterValues.Count; i++)
                {
                    string parameterName = string.Format("@{0}_Values{1}", this.FieldName.Replace(".", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty), i);
                    parameterNamesBuilder.AppendFormat("{0},", parameterName);
                    if (buildContext.DataCommand.HasDefinedParameter(parameterName))
                    {
                        buildContext.DataCommand.SetParameterValue(parameterName, this.ParameterDbType, parameterValues[i]);
                    }
                    else
                    {
                        buildContext.DataCommand.AddInputParameter(parameterName, this.ParameterDbType, parameterValues[i]);
                    }
                }
                string parameterNames = parameterNamesBuilder.ToString();
                parameterNames = parameterNames.Substring(0, parameterNames.Length - 1);
                result = string.Format(" {0} {1} {2} ({3})", buildContext.IsFirstCondition ? string.Empty : this.ConditionRelationType.ToString(),
                    this.FieldName, GetOperatorString(this.OperatorType), parameterNames);
            }
            else
            {
                result = string.Format(" {0} {1} {2} {3}", buildContext.IsFirstCondition ? string.Empty : this.ConditionRelationType.ToString(),
                    this.FieldName, GetOperatorString(this.OperatorType), this.ParameterName);
                if (!buildContext.AddedParameterNames.Contains(this.ParameterName))
                {
                    object val = (this.OperatorType == QueryConditionOperatorType.Like || this.OperatorType == QueryConditionOperatorType.LeftLike || this.OperatorType == QueryConditionOperatorType.RightLike) ?
                            TryConvertToLikeString(this.ParameterValue, this.OperatorType) : this.ParameterValue;
                    if (buildContext.DataCommand.HasDefinedParameter(this.ParameterName))
                    {
                        buildContext.DataCommand.SetParameterValue(this.ParameterName, this.ParameterDbType, val);
                    }
                    else
                    {
                        buildContext.DataCommand.AddInputParameter(this.ParameterName, this.ParameterDbType, val);
                    }
                    buildContext.AddedParameterNames.Add(this.ParameterName);
                }
            }
            return result;
        }

        internal override void BuildQueryString(StringBuilder queryStringBuilder, ConditionCollectionContext buildContext)
        {
            base.BuildQueryString(queryStringBuilder, buildContext);
            queryStringBuilder.Append(BuildSqlConditionString(buildContext));
            buildContext.IsFirstCondition = false;
        }
    }

    internal class LeftBracketCondition : Condition
    {
        public QueryConditionRelationType GroupConditionRelationType { get; set; }

        internal override void BuildQueryString(StringBuilder queryStringBuilder, ConditionCollectionContext buildContext)
        {
            base.BuildQueryString(queryStringBuilder, buildContext);
            queryStringBuilder.Append(string.Format(" {0} (",
                buildContext.IsFirstCondition ? string.Empty : this.GroupConditionRelationType.ToString()));
            buildContext.FirstGroupConditionFlags.Push(buildContext.IsFirstCondition);
            buildContext.IsFirstCondition = true;
        }
    }

    internal class RightBraketCondition : Condition
    {
        internal override void BuildQueryString(StringBuilder queryStringBuilder, ConditionCollectionContext buildContext)
        {
            base.BuildQueryString(queryStringBuilder, buildContext);
            if (buildContext.FirstGroupConditionFlags.Count > 0)
            {
                bool isFirstGroupCondition = buildContext.FirstGroupConditionFlags.Pop();
                string currentQueryString = queryStringBuilder.ToString().Trim();
                if (currentQueryString.Substring(currentQueryString.Length - 1, 1) == "(")
                {
                    if (isFirstGroupCondition == true)
                    {
                        queryStringBuilder.Remove(queryStringBuilder.Length - 1, 1);
                    }
                    else
                    {
                        queryStringBuilder.Remove(queryStringBuilder.Length - 5, 5);
                        buildContext.IsFirstCondition = false;
                    }
                }
                else
                {
                    queryStringBuilder.Append(" )");
                    buildContext.IsFirstCondition = false;
                }
            }
        }
    }

    internal class SubQueryCondition : Condition
    {
        public QueryConditionRelationType ConditionRelationType { get; set; }

        public string FieldName { get; set; }

        public QueryConditionOperatorType OperatorType { get; set; }

        public string SubQuerySQLTemplate { get; set; }

        public List<Condition> SubQueryConditions { get; set; }

        internal override void BuildQueryString(StringBuilder queryStringBuilder, ConditionCollectionContext buildContext)
        {
            base.BuildQueryString(queryStringBuilder, buildContext);
            ConditionCollectionContext subQueryBuildCondition = new ConditionCollectionContext(buildContext.DataCommand);
            subQueryBuildCondition.AddedParameterNames = buildContext.AddedParameterNames;
            queryStringBuilder.Append(string.Format(" {0} {1} {2} ({3} {4})",
                buildContext.IsFirstCondition ? string.Empty : this.ConditionRelationType.ToString(),
                this.FieldName, GetOperatorString(this.OperatorType), this.SubQuerySQLTemplate,
                ConditionConstructor.BuildQuerySqlConditionString(this.SubQueryConditions, subQueryBuildCondition)));
            buildContext.IsFirstCondition = false;
        }
    }

    internal class CustomCondition : Condition
    {
        public QueryConditionRelationType ConditionRelationType { get; set; }

        public string CustomQueryString { get; set; }

        internal override void BuildQueryString(StringBuilder queryStringBuilder, ConditionCollectionContext buildContext)
        {
            base.BuildQueryString(queryStringBuilder, buildContext);
            queryStringBuilder.Append(string.Format(" {0} {1}",
                buildContext.IsFirstCondition ? string.Empty : this.ConditionRelationType.ToString(),
                this.CustomQueryString));
            buildContext.IsFirstCondition = false;
        }
    }

    #endregion Consition Classes

    [Serializable]
    public class PagingInfoEntity
    {
        public int TotalCount { get; set; }

        public int? StartRowIndex { get; set; }

        public int? MaximumRows { get; set; }

        public string SortField { get; set; }
    }

    public delegate bool ParameterValueValidateCheckDelegate(object value);

    public enum QueryConditionRelationType
    {
        AND,
        OR
    }

    public enum QueryConditionOperatorType
    {
        Equal,
        NotEqual,
        MoreThan,
        LessThan,
        MoreThanOrEqual,
        LessThanOrEqual,
        Like,
        IsNull,
        IsNotNull,
        In,
        NotIn,
        Exist,
        NotExist,
        LeftLike,
        RightLike
    }
}
