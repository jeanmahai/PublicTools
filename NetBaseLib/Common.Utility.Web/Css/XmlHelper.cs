/**********************************************************************************************
// Copyright (c) 2013, Newegg (Chengdu) Co., Ltd. All rights reserved.
// Created by victor.w.ye at 7/23/2013 5:17:06 PM.
// E-Mail: victor.w.ye@newegg.com
// File Name : XmlHelper
// CLR Version : 4.0.30319.586
// Target Framework : 4.0
// Description :
//
//*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Common.Utility.Web
{
    internal class XmlHelper
    {
        // Fields
        private XDocument m_xDocument;
        private string xmlNamespace;

        // Methods
        public XElement GetElement(XElement document, string elementName)
        {
            XName name = XName.Get(elementName, this.xmlNamespace);
            return document.Element(name);
        }

        public XElement GetElement(XDocument document, string elementName, string attributeName, string attributeValue)
        {
            XName name = XName.Get(elementName, this.xmlNamespace);
            return Enumerable.SingleOrDefault<XElement>(document.Root.Elements(name), (Func<XElement, bool>)(x => (x.Attribute(attributeName).Value == attributeValue.Trim())));
        }

        public XElement GetElement(XElement element, string elementName, string attributeName, string attributeValue)
        {
            XName name = XName.Get(elementName, this.xmlNamespace);
            return Enumerable.SingleOrDefault<XElement>(element.Elements(name), (Func<XElement, bool>)(x => (x.Attribute(attributeName).Value.Trim() == attributeValue)));
        }

        public List<XElement> GetElements(string elementName)
        {
            XName name = XName.Get(elementName, this.xmlNamespace);
            return this.m_xDocument.Root.Elements(name).ToList<XElement>();
        }

        public string GetRootAttribute(string attributeName)
        {
            return this.m_xDocument.Root.Attribute(attributeName).Value;
        }

        public List<XElement> GetElements(XElement element, string elementName)
        {
            XName name = XName.Get(elementName, this.xmlNamespace);
            return element.Elements(name).ToList<XElement>();
        }

        public void LoadXml(string xmlFilePath)
        {
            this.m_xDocument = XDocument.Load(xmlFilePath);
            this.xmlNamespace = this.m_xDocument.Root.Name.NamespaceName;
        }

    }
}
