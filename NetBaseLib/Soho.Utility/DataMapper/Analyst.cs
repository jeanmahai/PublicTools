using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Soho.Utility
{
    internal static class Analyst
    {
        public static string GetPropertyCombineStr(Expression exp)
        {
            return Visit(exp);
        }

        private static string Visit(Expression exp)
        {
            if (exp == null)
                return string.Empty;
            switch (exp.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return Visit(((UnaryExpression)exp).Operand);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)exp);
                default:
                    throw new ApplicationException(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        private static string VisitMemberAccess(MemberExpression m)
        {
            if (m.Member is PropertyInfo)
            {
                string prefix = Visit(m.Expression);
                string name = m.Member.Name;
                if (prefix != null && prefix.Length > 0)
                {
                    return prefix + "." + name;
                }
                return name;
            }
            else
            {
                throw new ApplicationException(m.Member.Name + "不是Property.");
            }
        }

        private static string VisitParameter(ParameterExpression p)
        {
            return string.Empty;
        }

        private const string PARAM_EXPRESSION = @"([^@]+|^)@(?<Variable>\w+)[.\n]*";
        private const string VAR_EXPRESSION = @"\bDECLARE\s*@(?<Variable>\w+)\s*[.\n]*";
        private static Regex s_RegexParam = new Regex(PARAM_EXPRESSION, RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex s_RegexVar = new Regex(VAR_EXPRESSION, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static List<string> GetSqlParamNameList(string sqlText)
        {
            List<string> varList = new List<string>();
            MatchCollection matchCollection = s_RegexVar.Matches(sqlText);
            for (int i = 0; i < matchCollection.Count; i++)
            {
                string txt = matchCollection[i].Groups["Variable"].Value.Trim();
                if (!varList.Contains(txt))
                {
                    varList.Add(txt);
                }
            }

            List<string> rstList = new List<string>(matchCollection.Count);
            matchCollection = s_RegexParam.Matches(sqlText);
            for (int i = 0; i < matchCollection.Count; i++)
            {
                string txt = matchCollection[i].Groups["Variable"].Value.Trim();
                if (!rstList.Contains(txt) && !varList.Contains(txt))
                {
                    rstList.Add(txt);
                }
            }

            return rstList;
        }
    }
}
