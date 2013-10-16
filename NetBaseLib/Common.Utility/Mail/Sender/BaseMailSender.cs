using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Common.Utility
{
    public abstract class BaseMailSender : IMailSender
    {
        protected XmlNode[] GetChildrenNodes(XmlNode node, string nodeName)
        {
            return GetChildrenNodes(node, delegate(XmlNode child)
            {
                return child.Name == nodeName;
            });
        }

        protected XmlNode[] GetChildrenNodes(XmlNode node, Predicate<XmlNode> match)
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

        protected string GetNodeAttribute(XmlNode node, string attributeName)
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

        protected Dictionary<string, string> GetParams(XmlNode node)
        {
            if (node.Attributes == null || node.Attributes.Count <= 0)
            {
                return new Dictionary<string, string>(0);
            }
            Dictionary<string, string> rst = new Dictionary<string, string>(node.Attributes.Count * 2);
            foreach (XmlAttribute attr in node.Attributes)
            {
                string v = attr.Value == null ? string.Empty : attr.Value.Trim();
                rst.Add(attr.Name, v);
            }
            return rst;
        }

        protected abstract string NodeName
        {
            get;
        }

        protected virtual string SubNodeName
        {
            get { return "add"; }
        }

        protected virtual void ProcessParams(ref Dictionary<string, string> dic, XmlNode node, XmlNode subNode)
        {

        }

        protected virtual void ProcessNode(ref List<Dictionary<string, string>> dic, XmlNode node)
        {

        }

        public List<Dictionary<string, string>> AnalyseConfig(XmlNode section, out int recoverSeconds)
        {
            recoverSeconds = -1;
            if (section == null)
            {
                return new List<Dictionary<string, string>>(0);
            }
            XmlNode[] nodeList = GetChildrenNodes(section, NodeName);
            if (nodeList == null || nodeList.Length <= 0 || nodeList[0] == null)
            {
                return new List<Dictionary<string, string>>(0);
            }
            XmlNode node = nodeList[0];
            string t = GetNodeAttribute(node, "recover");
            if (t == null || t.Trim().Length <= 0 || int.TryParse(t, out recoverSeconds) == false)
            {
                recoverSeconds = 60;
            }
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            XmlNode[] subNodeList = GetChildrenNodes(node, SubNodeName);
            foreach (XmlNode sub in subNodeList)
            {
                Dictionary<string, string> dic = GetParams(sub);
                ProcessParams(ref dic, node, sub);
                list.Add(dic);
            }
            ProcessNode(ref list, node);
            return list;
        }

        public abstract void Send(MailEntity entity, Dictionary<string, string> parameters);
    }
}
