/**********************************************************************************************
// Copyright (c) 2013, Newegg (Chengdu) Co., Ltd. All rights reserved.
// Created by victor.w.ye at 7/23/2013 5:16:50 PM.
// E-Mail: victor.w.ye@newegg.com
// File Name : CssConfigHelper
// CLR Version : 4.0.30319.586
// Target Framework : 4.0
// Description :
//
//*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Soho.Utility.Web
{
    internal class CssConfigHelper
    {
        public static readonly string CSS_CONFIG_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configuration/Resources/Css/CssGroups.config");
        public static readonly string VIEW_CSS_MAPPING_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configuration/Resources/Css/CssViewMappings.config");
        public static readonly string VIEW_ALIAS_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configuration/Resources/Css/ViewName.config");

        /// <summary>
        /// 获取Css文件配置config
        /// </summary>
        /// <returns></returns>
        public static List<CSSGroupInfo> BuildCssGroupInfoList(out bool enableOptimizations)
        {
            if (!File.Exists(CSS_CONFIG_PATH))
            {
                throw new FileNotFoundException(string.Format("未找到配置文件:{0}!", CSS_CONFIG_PATH));
            }

            List<CSSGroupInfo> resultList = new List<CSSGroupInfo>();

            XmlHelper xmlHelper = new XmlHelper();
            xmlHelper.LoadXml(CSS_CONFIG_PATH);


            enableOptimizations = (xmlHelper.GetRootAttribute("enableOptimizations").ToLower() == "true" ? true : false);

            var getGroupList = xmlHelper.GetElements("Group");
            if (null != getGroupList && getGroupList.Count > 0)
            {
                getGroupList.ForEach(g =>
                {
                    string getVirtualPath = (g.Attribute("virtualPath") == null ? string.Empty : g.Attribute("virtualPath").Value);
                    string getName = g.Attribute("name").Value;

                    CSSGroupInfo newGroupInfo = new CSSGroupInfo() { Name = getName };
                    var getCssList = xmlHelper.GetElements(g, "Css");
                    if (null != getCssList && getCssList.Count > 0)
                    {
                        getCssList.ForEach(c =>
                        {
                            newGroupInfo.CSSPathList.Add(c.Value.Trim());

                        });
                    }
                    string defautVirtualPath = newGroupInfo.CSSPathList[0].Substring(0, newGroupInfo.CSSPathList[0].LastIndexOf('/')) + "/Css";
                    newGroupInfo.VirtualPath = (string.IsNullOrEmpty(getVirtualPath) ? defautVirtualPath : getVirtualPath);
                    resultList.Add(newGroupInfo);
                });
            }
            return resultList;
        }

        /// <summary>
        /// 获取view-name映射配置config
        /// </summary>
        /// <returns></returns>
        public static List<ViewAliasInfo> BuildViewAliasInfoList()
        {
            if (!File.Exists(VIEW_ALIAS_PATH))
            {
                throw new FileNotFoundException(string.Format("未找到配置文件:{0}!", VIEW_ALIAS_PATH));
            }
            List<ViewAliasInfo> returnList = new List<ViewAliasInfo>();
            XmlHelper xmlHelper = new XmlHelper();
            xmlHelper.LoadXml(VIEW_ALIAS_PATH);
            var getList = xmlHelper.GetElements("add");
            if (null != getList && getList.Count > 0)
            {
                getList.ForEach(x =>
                {
                    returnList.Add(new ViewAliasInfo() { Name = x.Attribute("name").Value, Path = x.Value.Trim() });
                });
            }
            return returnList;
        }

        /// <summary>
        /// 获取ViewCss隐射关系config
        /// </summary>
        /// <returns></returns>
        public static List<ViewCSSMappingInfo> BuildViewCssMappingList()
        {
            if (!File.Exists(VIEW_CSS_MAPPING_PATH))
            {
                throw new FileNotFoundException(string.Format("未找到配置文件:{0}!", VIEW_CSS_MAPPING_PATH));
            }
            List<ViewCSSMappingInfo> returnList = new List<ViewCSSMappingInfo>();
            XmlHelper xmlHelper = new XmlHelper();
            xmlHelper.LoadXml(VIEW_CSS_MAPPING_PATH);
            var getList = xmlHelper.GetElements("Mapping");
            if (null != getList && getList.Count > 0)
            {
                getList.ForEach(m =>
                {
                    ViewCSSMappingInfo newInfo = new ViewCSSMappingInfo();
                    var getGroupList = xmlHelper.GetElements(xmlHelper.GetElement(m, "CssGroups"), "Group");
                    if (null != getGroupList && getGroupList.Count > 0)
                    {
                        getGroupList.ForEach(g =>
                        {
                            newInfo.CSSGroupNameList.Add(g.Value.Trim());
                        });
                    }
                    var getViewNameList = xmlHelper.GetElements(xmlHelper.GetElement(m, "ViewList"), "View");
                    if (null != getViewNameList && getViewNameList.Count > 0)
                    {
                        getViewNameList.ForEach(v =>
                        {
                            newInfo.ViewNameList.Add(v.Value.Trim());
                        });
                    }
                    returnList.Add(newInfo);

                });
            }
            return returnList;
        }
    }

    internal class CSSGroupInfo
    {
        public string Name { get; set; }
        public string VirtualPath { get; set; }
        public List<string> CSSPathList { get; set; }

        public CSSGroupInfo()
        {
            CSSPathList = new List<string>();
        }
    }

    /// <summary>
    /// View对应名称实体
    /// </summary>
    internal class ViewAliasInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string Path { get; set; }
        public string Name { get; set; }
    }

    internal class ViewCSSMappingInfo
    {
        public List<string> CSSGroupNameList { get; set; }
        public List<string> ViewNameList { get; set; }

        public ViewCSSMappingInfo()
        {
            CSSGroupNameList = new List<string>();
            ViewNameList = new List<string>();
        }
    }
}
