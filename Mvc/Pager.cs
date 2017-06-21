using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace DynamicQuery.Mvc
{
    public class Pager
    {
        public Pager(int pageCount, QueryDescriptor descriptor)
        {
            this.pageCount = pageCount;
            this.descriptor = descriptor;
        }        
        int pageCount;
        QueryDescriptor descriptor;

         public int PageCount
        {
            get { return pageCount; }
        }

        public QueryDescriptor Descriptor
        {
            get { return descriptor; }
        }

    }

    public static class QueryPagerExtension
    {
        public static MvcHtmlString QueryPager(this HtmlHelper html, Pager pager)
        {
            TagBuilder div = new TagBuilder("div");
            div.AddCssClass("pagination");
            var requestContext=html.ViewContext.RequestContext;
            var controller=requestContext.RouteData.Values["controller"].ToString();
            var action = requestContext.RouteData.Values["action"].ToString();
            var urlHelper = new UrlHelper(requestContext);          
       
            StringBuilder linkString=new StringBuilder();
            for (int i = 1; i <= pager.PageCount; i++)
            {
                var routeValue = GetRouteValue(pager.Descriptor, i);
                var href = urlHelper.Action(action, controller, routeValue);          
                var link = string.Format("<li><a href='{0}'>{1}</a></li>", href, i);
                linkString.Append(link);
            }
            div.InnerHtml = "<ul>" + linkString.ToString() + "</ul>";
            return MvcHtmlString.Create(div.ToString());
        }

        static RouteValueDictionary GetRouteValue(QueryDescriptor descriptor, int currentPage)
        {
            var routeValue = new RouteValueDictionary();
            if (descriptor.Conditions != null)
            {
                foreach (var cond in descriptor.Conditions)
                {
                    routeValue.Add("_query."+cond.Key, cond.Value.ToString());
                    routeValue.Add("_query." + cond.Key + ".operator", cond.Operator.ToString());
                    if (!string.IsNullOrEmpty(cond.ValueType))
                        routeValue.Add("_query." + cond.Key + ".valuetype", cond.ValueType);
                }
            }

            if (descriptor.PageSize > 0)
            {
                routeValue.Add("_query.pagesize", descriptor.PageSize);
                routeValue.Add("page", currentPage);
            }

            if (descriptor.OrderBy != null)
            {
                if (descriptor.OrderBy.Order == OrderSequence.ASC)
                {
                    routeValue.Add("_query.orderbyasc", descriptor.OrderBy.Key);
                }
                else
                {
                    routeValue.Add("_query.orderbydesc", descriptor.OrderBy.Key);
                }
            }           
            return routeValue;             
        }
    }
}
