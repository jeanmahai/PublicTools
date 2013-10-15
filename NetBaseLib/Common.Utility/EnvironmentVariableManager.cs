using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Text.RegularExpressions;

namespace Common.Utility
{
    public static class EnvironmentVariableManager
    {
        private const char SPLITTER = '%';
        private static Regex s_Regex = new Regex("%(\\w+?)%", RegexOptions.Compiled);

        public static string GetVariableValue(string variableName)
        {
            var dictDictionarySectionHandler = ConfigurationManager.GetSection("environmentVariable") as IDictionary;
            if (dictDictionarySectionHandler == null || !dictDictionarySectionHandler.Contains(variableName))
            {
                return string.Empty;
            }
            object v = dictDictionarySectionHandler[variableName];
            if(v == null)
            {
                return string.Empty;
            }
            return v.ToString().Trim();
        }

        public static bool IsVariableDefined(string variableName)
        {
            var dictDictionarySectionHandler = ConfigurationManager.GetSection("environmentVariable") as IDictionary;
            return (dictDictionarySectionHandler != null && dictDictionarySectionHandler.Contains(variableName));
        }

        public static string ReplaceVariable(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.IndexOf(SPLITTER) < 0)
            {
                return input;
            }
            MatchCollection mc = s_Regex.Matches(input);
            return s_Regex.Replace(input, delegate(Match m)
            {
                if (m.Groups.Count > 1 && !string.IsNullOrWhiteSpace(m.Groups[1].Value))
                {
                    string key = m.Groups[1].Value.Trim();
                    if (EnvironmentVariableManager.IsVariableDefined(key))
                    {
                        return EnvironmentVariableManager.GetVariableValue(key);
                    }
                }
                return m.Value;
            });
        }
    }
}
