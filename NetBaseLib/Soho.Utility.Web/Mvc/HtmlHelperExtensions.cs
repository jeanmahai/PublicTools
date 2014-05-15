using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Soho.Utility.Web.Mvc
{
    public class HeadLink
    {
        public string Rel { get; set; }
        public string Href { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public object HtmlAttributes { get; set; }
    }

    public static class HtmlHelperExtensions
    {               
        #region HeadLink

        public static string HeadLink(this HtmlHelper htmlHelper, HeadLink headLink)
        {
            return htmlHelper.HeadLink(headLink.Rel, headLink.Href, headLink.Type, headLink.Title,
                                       headLink.HtmlAttributes);
        }

        public static string HeadLink(this HtmlHelper htmlHelper, string rel, string href, string type, string title)
        {
            return htmlHelper.HeadLink(rel, href, type, title, null);
        }

        public static string HeadLink(this HtmlHelper htmlHelper, string rel, string href, string type, string title,
                                      object htmlAttributes)
        {
            TagBuilder tagBuilder = new TagBuilder("link");

            tagBuilder.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            if (!string.IsNullOrEmpty(rel))
            {
                tagBuilder.MergeAttribute("rel", rel);
            }
            if (!string.IsNullOrEmpty(href))
            {
                tagBuilder.MergeAttribute("href", href);
            }
            if (!string.IsNullOrEmpty(type))
            {
                tagBuilder.MergeAttribute("type", type);
            }
            if (!string.IsNullOrEmpty(title))
            {
                tagBuilder.MergeAttribute("title", title);
            }

            return tagBuilder.ToString(TagRenderMode.SelfClosing);
        }

        #endregion        

        #region Input
       
        public static string Button(this HtmlHelper htmlHelper, string name, string buttonContent, object htmlAttributes)
        {
            return htmlHelper.Button(name, buttonContent, new RouteValueDictionary(htmlAttributes));
        }

        public static string Button(this HtmlHelper htmlHelper, string name, string buttonContent,
                                    IDictionary<string, object> htmlAttributes)
        {
            TagBuilder tagBuilder = new TagBuilder("button")
                                    {
                                        InnerHtml = buttonContent
                                    };
            tagBuilder.MergeAttributes(htmlAttributes);
            return tagBuilder.ToString(TagRenderMode.Normal);
        }

        #endregion

        #region Link

        public static string Link(this HtmlHelper htmlHelper, string linkText, string href)
        {
            return Link(htmlHelper, linkText, href, null);
        }

        public static string Link(this HtmlHelper htmlHelper, string linkText, string href, object htmlAttributes)
        {
            return htmlHelper.Link(linkText, href, new RouteValueDictionary(htmlAttributes));
        }

        public static string Link(this HtmlHelper htmlHelper, string linkText, string href,
                                  IDictionary<string, object> htmlAttributes)
        {
            TagBuilder tagBuilder = new TagBuilder("a")
                                    {
                                        InnerHtml = linkText
                                    };
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("href", href);
            return tagBuilder.ToString(TagRenderMode.Normal);
        }

        #endregion              

        #region ScriptBlock

        public static string ScriptBlock(this HtmlHelper htmlHelper, string type, string src)
        {
            return htmlHelper.ScriptBlock(type, src, null);
        }

        public static string ScriptBlock(this HtmlHelper htmlHelper, string type, string src, object htmlAttributes)
        {
            TagBuilder tagBuilder = new TagBuilder("script");

            tagBuilder.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            if (!string.IsNullOrEmpty(type))
            {
                tagBuilder.MergeAttribute("type", type);
            }
            if (!string.IsNullOrEmpty(src))
            {
                tagBuilder.MergeAttribute("src", src);
            }

            return tagBuilder.ToString(TagRenderMode.Normal);
        }

        #endregion

        #region UnorderedList

        public static string UnorderedList<T>(this HtmlHelper htmlHelper, IEnumerable<T> items,
                                              Func<T, string> generateContent)
        {
            return UnorderedList<T>(htmlHelper, items, (t, i) => generateContent(t));
        }

        public static string UnorderedList<T>(this HtmlHelper htmlHelper, IEnumerable<T> items,
                                              Func<T, string> generateContent, string cssClass)
        {
            return UnorderedList<T>(htmlHelper, items, (t, i) => generateContent(t), cssClass, null, null);
        }

        public static string UnorderedList<T>(this HtmlHelper htmlHelper, IEnumerable<T> items,
                                              Func<T, int, string> generateContent)
        {
            return UnorderedList<T>(htmlHelper, items, generateContent, null);
        }

        public static string UnorderedList<T>(this HtmlHelper htmlHelper, IEnumerable<T> items,
                                              Func<T, int, string> generateContent, string cssClass)
        {
            return UnorderedList<T>(htmlHelper, items, generateContent, cssClass, null, null);
        }

        public static string UnorderedList<T>(this HtmlHelper htmlHelper, IEnumerable<T> items,
                                              Func<T, int, string> generateContent, string cssClass, string itemCssClass,
                                              string alternatingItemCssClass)
        {
            if (items == null || items.Count() == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder(100);
            int counter = 0;

            sb.Append("<ul");
            if (!string.IsNullOrEmpty(cssClass))
            {
                sb.AppendFormat(" class=\"{0}\"", cssClass);
            }
            sb.Append(">");
            foreach (T item in items)
            {
                StringBuilder sbClass = new StringBuilder(40);

                if (counter == 0)
                {
                    sbClass.Append("first ");
                }
                if (item.Equals(items.Last()))
                {
                    sbClass.Append("last ");
                }

                if (counter % 2 == 0 && !string.IsNullOrEmpty(itemCssClass))
                {
                    sbClass.AppendFormat("{0} ", itemCssClass);
                }
                else if (counter % 2 != 0 && !string.IsNullOrEmpty(alternatingItemCssClass))
                {
                    sbClass.AppendFormat("{0} ", alternatingItemCssClass);
                }

                sb.Append("<li");
                if (sbClass.Length > 0)
                {
                    sb.AppendFormat(" class=\"{0}\"", sbClass.Remove(sbClass.Length - 1, 1));
                }
                sb.AppendFormat(">{0}</li>", generateContent(item, counter));

                counter++;
            }
            sb.Append("</ul>");

            return sb.ToString();
        }

        #endregion

        #region Pager
        /// <summary>
        /// 分页控件
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="currentPage">当前页</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="totalCount">总记录数</param>
        /// <param name="extensionParam">扩展URL参数，格式：&A=a&B=b</param>
        /// <returns></returns>
        public static HtmlString ShowPageNavigate(this HtmlHelper htmlHelper, int currentPage, int pageSize, int totalCount, string extensionParam)
        {
            var redirectTo = htmlHelper.ViewContext.RequestContext.HttpContext.Request.Url.AbsolutePath;
            pageSize = pageSize == 0 ? 3 : pageSize;
            //总页数
            var totalPages = Math.Max((totalCount + pageSize - 1) / pageSize, 1);
            var output = new StringBuilder();
            if (totalPages > 1)
            {
                if (currentPage > 1)
                {
                    //处理上一页的连接
                    output.AppendFormat("<a class='pre' href='{0}?page={1}{2}'><span><s class=\"arrow\"></s>上一页</span></a>", redirectTo, currentPage - 1, extensionParam);
                }

                int currint = 5;
                for (int i = 0; i <= 10; i++)
                {
                    //一共最多显示10个页码，前面5个，后面5个
                    if ((currentPage + i - currint) >= 1 && (currentPage + i - currint) <= totalPages)
                    {
                        if (currint == i)
                        {
                            //当前页处理
                            output.AppendFormat("<a class='curr' href='javascript:void(0)'><span>{0}</span></a> ", currentPage);
                        }
                        else
                        {
                            //一般页处理
                            output.AppendFormat("<a href='{0}?page={1}{2}'><span>{3}</span></a> ", redirectTo, currentPage + i - currint, extensionParam, currentPage + i - currint);
                        }
                    }
                }
                if (currentPage < totalPages)
                {
                    //处理下一页的链接
                    output.AppendFormat("<a class='next' href='{0}?page={1}{2}'><span>下一页<s class=\"arrow\"></s></span></a> ", redirectTo, currentPage + 1, extensionParam);
                }
            }
            //统计
            output.AppendFormat("<span class=\"pageTotal mr5 ml10\">共<strong>{0}</strong>页</span>", totalPages);

            return new HtmlString(output.ToString());
        }
        #endregion
    }
}
