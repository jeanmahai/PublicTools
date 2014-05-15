/**********************************************************************************************
// Copyright (c) 2013, Newegg (Chengdu) Co., Ltd. All rights reserved.
// Created by victor.w.ye at 7/23/2013 5:16:40 PM.
// E-Mail: victor.w.ye@newegg.com
// File Name : CssBundleManager
// CLR Version : 4.0.30319.586
// Target Framework : 4.0
// Description :
//
//*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Web.Optimization;

namespace Soho.Utility.Web
{
    public class CssBundleManager
    {
        public static bool IsEnableOptimizations;
        private static List<CSSGroupInfo> s_CSSConfigList;
        private static List<ViewAliasInfo> s_ViewAliasConfigList;
        private static List<ViewCSSMappingInfo> s_ViewCSSMappingConfigList;
        private static object s_SyncObj = new object();

        static CssBundleManager()
        {
            if (s_CSSConfigList == null && s_ViewAliasConfigList == null && s_ViewCSSMappingConfigList == null)
            {
                lock (s_SyncObj)
                {
                    s_CSSConfigList = CssConfigHelper.BuildCssGroupInfoList(out IsEnableOptimizations);
                    s_ViewAliasConfigList = CssConfigHelper.BuildViewAliasInfoList();
                    s_ViewCSSMappingConfigList = CssConfigHelper.BuildViewCssMappingList();
                }
            }
        }

        /// <summary>
        /// 根据配置注册Css Bundle(Globle.asax里面的Application_Start方法里面调用)
        /// </summary>
        /// <param name="bundles">BundleTable.Bundles</param>
        public static void Register(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = CssBundleManager.IsEnableOptimizations;
            foreach (var item in s_CSSConfigList)
            {
                string[] getCssPaths = item.CSSPathList.ToArray();
                bundles.Add(new Bundle(item.VirtualPath).Include(getCssPaths));
            }
        }

        /// <summary>
        /// View Render出css引用
        /// </summary>
        /// <param name="context">当前View的ViewContext</param>
        /// <returns>IHtmlString</returns>
        public static IHtmlString Render(ViewContext context)
        {
            string getViewPath = ((System.Web.Mvc.BuildManagerCompiledView)(context.View)).ViewPath;
            getViewPath = getViewPath.Substring(0, getViewPath.LastIndexOf('.'));
            List<string> virtualPaths = new List<string>();

            if (!string.IsNullOrEmpty(getViewPath))
            {
                var getViewAliasObj = s_ViewAliasConfigList.SingleOrDefault(x => x.Path.ToLower() == getViewPath.Trim().ToLower());
                if (null == getViewAliasObj)
                {
                    throw new ApplicationException(string.Format("未能在ViewName.config中找到路径为{0}的配置项!", getViewPath));
                }
                string getCurrentViewAlias = getViewAliasObj.Name;
                //1.找到mapping中包含此alias名称的项目:
                List<ViewCSSMappingInfo> getMappingList = s_ViewCSSMappingConfigList.Where(x => x.ViewNameList.Contains(getCurrentViewAlias)).ToList();
                if (null != getMappingList && getMappingList.Count > 0)
                {
                    foreach (var mappingItem in getMappingList)
                    {
                        //2.开始不重复加载CSSGroup里面的资源:
                        mappingItem.CSSGroupNameList.ForEach(x =>
                        {
                            var getGroupObj = s_CSSConfigList.SingleOrDefault(e => e.Name == x);
                            if (null == getGroupObj)
                            {
                                throw new ApplicationException(string.Format("未能在Css.config中找到组名称为{0}的配置项!", x));
                            }
                            if (!virtualPaths.Contains(getGroupObj.VirtualPath))
                            {
                                virtualPaths.Add(getGroupObj.VirtualPath);
                            }
                        });
                    }
                }
            }
            return Styles.Render(virtualPaths.ToArray());
        }
    }
}
