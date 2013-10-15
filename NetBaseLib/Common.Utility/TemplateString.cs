using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;

namespace Common.Utility
{
    public static class TemplateString
    {
        private const string OUTPUT_PLACE_HOLDER = @"<%=\s*(?<id>[\s\S]+?)\s*%>";

        public static string BuildHtml(string html, KeyValueVariables keyValue, KeyTableVariables keyTable)
        {
            List<Node> nodeList = AnalyseTemplate(html);
            Stack<Node> stack = new Stack<Node>(nodeList);
            while (stack.Count > 0)
            {
                Node node = stack.Pop();
                if (node.PlaceholdType == PlaceholdType.Table)
                {
                    html = html.Replace(node.OuterHtml, node.UniqueSign);
                }
                else
                {
                    string id = node.ID;
                    bool show;
                    if (keyValue.ContainsKey(id) && keyValue[id] != null && bool.TryParse(keyValue[id].ToString(), out show) && show)
                    {
                        foreach (var n in node.Children)
                        {
                            stack.Push(n);
                        }
                        nodeList.AddRange(node.Children);
                        html = html.Replace(node.OuterHtml, node.InnerHtml);
                    }
                    else
                    {
                        html = html.Replace(node.OuterHtml, string.Empty);
                    }
                    nodeList.Remove(node);
                }
            }

            // 处理KeyValue
            MatchCollection matchCollectioin = Regex.Matches(html, OUTPUT_PLACE_HOLDER, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            foreach (Match matchField in matchCollectioin)
            {
                if (matchField.Success)
                {
                    if (matchField.Groups["id"] == null || string.IsNullOrWhiteSpace(matchField.Groups["id"].Value))
                    {
                        throw new ApplicationException("no id");
                    }
                    string id = matchField.Groups["id"].Value.Trim();
                    if (keyValue.ContainsKey(id))
                    {
                        html = html.Replace(matchField.Groups[0].Value, CovertToString(keyValue[id]));
                    }
                    else
                    {
                        html = html.Replace(matchField.Groups[0].Value, string.Empty);
                    }
                }
            }

            // 处理循环
            foreach (Node node in nodeList)
            {
                if (keyTable.ContainsKey(node.ID) && keyTable[node.ID] != null)
                {
                    html = html.Replace(node.UniqueSign, node.BuildHtml(keyTable[node.ID]));
                }
                else
                {
                    html = html.Replace(node.UniqueSign, string.Empty);
                }
            }

            return html;
        }

        private static List<Node> AnalyseTemplate(string html)
        {
            string pattern = @"<%\s*(?<place>IF|TABLE_ROWS)_(?<tag>BEGIN|END)\s+ID\s*=(?<id>[\s\S]+?)\s*%>";
            MatchCollection matchCollectioin = Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            Stack<Placehold> stack = new Stack<Placehold>(matchCollectioin.Count);
            List<KeyValuePair<int, Node>> stackNode = new List<KeyValuePair<int, Node>>(matchCollectioin.Count / 2);
            StringBuilder htmlCopy = new StringBuilder(html);
            int cut_len = 0;
            foreach (Match matchField in matchCollectioin)
            {
                if (matchField.Success && matchField.Groups["id"] != null && !string.IsNullOrWhiteSpace(matchField.Groups["id"].Value))
                {
                    string tag = matchField.Groups["tag"].Value.ToUpper();
                    string place = matchField.Groups["place"].Value.ToUpper();
                    Placehold p = new Placehold();
                    if (tag == "BEGIN")
                    {
                        p.TagType = TagType.Begin;
                    }
                    else
                    {
                        p.TagType = TagType.End;
                    }
                    if (place == "IF")
                    {
                        p.PlaceholdType = PlaceholdType.If;
                    }
                    else
                    {
                        p.PlaceholdType = PlaceholdType.Table;
                    }
                    p.ID = matchField.Groups["id"].Value.Trim();
                    p.Word = matchField.Groups[0].Value;
                    int t_start = htmlCopy.ToString().IndexOf(p.Word);
                    p.StartPosition = t_start + cut_len;
                    cut_len = cut_len + p.Word.Length;
                    htmlCopy.Remove(t_start, p.Word.Length);
                    if (p.TagType == TagType.Begin)
                    {
                        stack.Push(p);
                    }
                    else
                    {
                        if (stack.Count <= 0)
                        {
                            throw new ApplicationException("The tag '" + p.Word + "' has not a matched Begin tag.");
                        }
                        Placehold tmp = stack.Pop();
                        if (tmp.TagType == TagType.Begin && tmp.PlaceholdType == p.PlaceholdType && tmp.ID == p.ID)
                        {
                            Node n = new Node(tmp, p, html);
                            stackNode.Add(new KeyValuePair<int, Node>(stack.Count, n));
                        }
                        else
                        {
                            throw new ApplicationException("The tag '" + p.Word + "' is not matched with the Begin tag '" + tmp.Word + "'");
                        }
                    }
                }
            }

            List<Node> nodeList = new List<Node>(stackNode.Count(k => k.Key == 0));
            for (int i = 0; i < stackNode.Count; i++)
            {
                KeyValuePair<int, Node> n = stackNode[i];
                if (n.Key == 0)
                {
                    nodeList.Add(n.Value);
                }
                else
                {
                    for (int j = i + 1; j < stackNode.Count; j++)
                    {
                        if (stackNode[j].Key == n.Key - 1)
                        {
                            stackNode[j].Value.Children.Add(n.Value);
                            break;
                        }
                    }
                }
            }
            return nodeList;
        }

        private static string CovertToString(object value)
        {
            if (value == null || value == DBNull.Value || value.ToString().Trim().Length <= 0)
            {
                return string.Empty;
            }
            Type type = value.GetType();
            while (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && type.GetGenericArguments() != null
                    && type.GetGenericArguments().Length == 1 && type.IsEnum)
            {
                type = type.GetGenericArguments()[0];
            }
            //if (type.IsEnum)
            //{
            //    return ((Enum)value).ToDisplayText();
            //}
            //else
            //{
                return value.ToString();
            //}
        }

        private class Placehold
        {
            public string ID { get; set; }
            public string Word { get; set; }
            public int StartPosition { get; set; }
            public int EndPosition
            {
                get
                {
                    int len = 0;
                    if (Word != null)
                    {
                        len = Word.Length;
                    }
                    return StartPosition + len;
                }
            }
            public TagType TagType { get; set; }
            public PlaceholdType PlaceholdType { get; set; }
        }

        private enum TagType
        {
            Begin,
            End
        }

        private enum PlaceholdType
        {
            If,
            Table
        }

        private class Node
        {
            public string ID
            {
                get;
                private set;
            }

            public List<Node> Children
            {
                get;
                private set;
            }

            public PlaceholdType PlaceholdType
            {
                get;
                private set;
            }

            public string InnerHtml
            {
                get;
                private set;
            }

            public string OuterHtml
            {
                get;
                private set;
            }

            public string UniqueSign
            {
                get;
                private set;
            }

            public Node(Placehold begin, Placehold end, string html)
            {
                ID = begin.ID;
                Children = new List<Node>();
                this.PlaceholdType = begin.PlaceholdType;
                InnerHtml = html.Substring(begin.EndPosition, end.StartPosition - begin.EndPosition);
                OuterHtml = html.Substring(begin.StartPosition, end.EndPosition - begin.StartPosition);
                UniqueSign = string.Format("<^*?{0}?*^>", Guid.NewGuid().ToString());
            }

            public string BuildHtml(DataTable data)
            {
                StringBuilder sb = new StringBuilder();
                foreach (DataRow row in data.Rows)
                {
                    string html = this.InnerHtml;
                    List<Node> nodeList = new List<Node>(this.Children);
                    Stack<Node> stack = new Stack<Node>(nodeList);
                    while (stack.Count > 0)
                    {
                        Node node = stack.Pop();
                        if (node.PlaceholdType == PlaceholdType.Table)
                        {
                            html = html.Replace(node.OuterHtml, node.UniqueSign);
                        }
                        else
                        {
                            string id = node.ID;
                            int cIndex = data.Columns.IndexOf(id);
                            bool show;
                            if (cIndex >= 0 && cIndex < data.Columns.Count && row[cIndex] != null && bool.TryParse(row[cIndex].ToString(), out show) && show)
                            {
                                foreach (var n in node.Children)
                                {
                                    stack.Push(n);
                                }
                                nodeList.AddRange(node.Children);
                                html = html.Replace(node.OuterHtml, node.InnerHtml);
                            }
                            else
                            {
                                html = html.Replace(node.OuterHtml, string.Empty);
                            }
                            nodeList.Remove(node);
                        }
                    }

                    MatchCollection matchCollectioin = Regex.Matches(InnerHtml, OUTPUT_PLACE_HOLDER, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    foreach (Match matchField in matchCollectioin)
                    {
                        if (matchField.Success)
                        {
                            if (matchField.Groups["id"] == null || string.IsNullOrWhiteSpace(matchField.Groups["id"].Value))
                            {
                                throw new ApplicationException("no id");
                            }
                            string id = matchField.Groups["id"].Value.Trim();
                            int cIndex = data.Columns.IndexOf(id);
                            if (cIndex >= 0 && cIndex < data.Columns.Count)
                            {
                                html = html.Replace(matchField.Groups[0].Value, row[cIndex].ToString());
                            }
                            else
                            {
                                html = html.Replace(matchField.Groups[0].Value, string.Empty);
                            }
                        }
                    }

                    // 处理循环
                    foreach (Node node in nodeList)
                    {
                        string id = node.ID;
                        int cIndex = data.Columns.IndexOf(id);
                        if (cIndex >= 0 && cIndex < data.Columns.Count && row[cIndex] != null && row[cIndex] != DBNull.Value)
                        {
                            if (row[cIndex] is DataTable)
                            {
                                html = html.Replace(node.UniqueSign, node.BuildHtml((DataTable)row[cIndex]));
                            }
                            else
                            {
                                throw new ApplicationException("The datasource for tag 'TABLE_ROWS' with id '" + id + "' only can be DataTable, but now the data is '" + row[cIndex].GetType() + "'");
                            }
                        }
                        else
                        {
                            html = html.Replace(node.UniqueSign, string.Empty);
                        }
                    }
                    sb.Append(html);
                }

                return sb.ToString();
            }
        }
    }

    public class KeyTableVariables : Dictionary<string, DataTable>
    {

    }

    public class KeyValueVariables : Dictionary<string, object>
    {

    }

    public static class KeyValueVariablesExtension
    {
        public static void AddKeyValue(this KeyValueVariables variables, string key, string value)
        {
            variables.Add(key, value);
        }
        public static void RemoveKeyValue(this KeyValueVariables variables, string key)
        {
            variables.Remove(key);
        }
    }
}
