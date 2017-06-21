using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DynamicQuery;

namespace DynamicQuery.Mvc
{
    public class QueryDescriptorBinder:IModelBinder
    {
        public QueryDescriptorBinder()
        {
            OptDictionary.Add(QueryOperator.EQUAL.ToString(), QueryOperator.EQUAL);
            OptDictionary.Add(QueryOperator.GREATEROREQUAL.ToString(), QueryOperator.GREATEROREQUAL);
            OptDictionary.Add(QueryOperator.LESSOREQUAL.ToString(), QueryOperator.LESSOREQUAL);
            OptDictionary.Add(QueryOperator.GERATER.ToString(), QueryOperator.GERATER);
            OptDictionary.Add(QueryOperator.LESS.ToString(), QueryOperator.LESS);
            OptDictionary.Add(QueryOperator.IN.ToString(), QueryOperator.IN);
            OptDictionary.Add(QueryOperator.CONTAINS.ToString(), QueryOperator.CONTAINS);
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {            
            QueryDescriptor descriptor = new QueryDescriptor { PageIndex = 1 , Conditions=new List<QueryCondition>(),OrderByMuilt=new List<OrderByClause>()};
            var provider = controllerContext.Controller.ValueProvider;

            var orderby=provider.GetValue("_query.orderbyasc");
            if (orderby != null)
            {
                descriptor.OrderBy = new OrderByClause { Key = orderby.AttemptedValue, Order = OrderSequence.ASC };
            }
            orderby = provider.GetValue("_query.orderbydesc");
            if (orderby != null)
            {
                descriptor.OrderBy = new OrderByClause { Key = orderby.AttemptedValue, Order = OrderSequence.DESC };
            }

            var pageSize = provider.GetValue("_query.pagesize");
            if (pageSize != null)
            {
                descriptor.PageSize = int.Parse(pageSize.AttemptedValue);
            }

            var pageIndex = provider.GetValue("page");
            if (pageIndex != null)
            {
                descriptor.PageIndex = int.Parse(pageIndex.AttemptedValue);
            }

            var request=controllerContext.HttpContext.Request;

            var keys = FilterExcludeKeys(controllerContext.HttpContext.Request);

            foreach (var k in keys)
            {
                var segments = k.Split('.');
                if (!ContainKeyword(segments)&&segments.Last()!="operator")
                {                   
                    string value = provider.GetValue(k).AttemptedValue;
                    if (!string.IsNullOrEmpty(value))
                    {
                        string key = k.Substring(k.IndexOf('.') + 1);
                        string opt = QueryOperator.EQUAL.ToString();
                        if (provider.GetValue(k + ".operator") != null &&
                            !string.IsNullOrEmpty(provider.GetValue(k + ".operator").AttemptedValue))
                        {
                            opt = provider.GetValue(k + ".operator").AttemptedValue;
                        }

                        string valueType = "string";
                        if (provider.GetValue(k + ".valuetype") != null &&
                             !string.IsNullOrEmpty(provider.GetValue(k + ".valuetype").AttemptedValue))
                        {
                            valueType = provider.GetValue(k + ".valuetype").AttemptedValue;
                        }
                        object realValue = value;

                        if (opt == "IN") {
                            // IN操作不需要转换类型
                            realValue = value;
                        } else { 
                            switch (valueType.ToLower())
                            {
                                case "int":
                                    //realValue = value
                                    realValue = int.Parse(value);
                                    break;
                                case "float":
                                    realValue = float.Parse(value);
                                    break;
                                case "double":
                                    realValue = double.Parse(value);
                                    break;
                                case "decimal":
                                    realValue = decimal.Parse(value);
                                    break;
                                case "time":
                                    realValue = DateTime.Parse(value);
                                    break;
                                case "datetime":
                                    realValue = DateTime.Parse(value);
                                    break;
                                case "string":
                                    break;
                                case "enum":
                                    realValue = Enum.Parse(typeof(Enum), value);
                                    break;
                                case "boolean":
                                    realValue = Boolean.Parse(value);
                                    break;
                                default:
                                    throw new NotSupportedException(string.Format(valueType,"Value Type of {0} is not supported"));
                            }
                        }

                        //deal with duplicate key names.
                        string postfix = key.LastIndexOf('.') > -1 ? key.Substring(key.LastIndexOf('.') + 1) : "nope";
                        int post;
                        if (int.TryParse(postfix, out post))
                            key = key.Substring(0, key.LastIndexOf('.'));
                        
                        QueryCondition cond = new QueryCondition
                        {
                            Key = key,
                            Operator = OptDictionary[opt],
                            Value = realValue,
                            ValueType= valueType
                        };
                        descriptor.Conditions.Add(cond);
                    }
                }              
            }
            return descriptor;
        }

        /// <summary>
        /// 过滤掉键值列表中需要被排除的项，用来支持下拉列表的“全部”项
        /// </summary>
        /// <param name="request"></param>
        /// <param name="keys"></param>
        private IEnumerable<String> FilterExcludeKeys(HttpRequestBase request)
        {
            var keys = from s in request.QueryString.AllKeys.Union(request.Form.AllKeys)
                       where s.StartsWith("_query")
                       select s;
            String propertyName, propertyValue, excludeValue;
            List<String> excludeProperties = new List<string>();
            for (int i = 0; i < keys.Count(); i++)
            {
                if (keys.ElementAt(i).ToLower().Contains("exclude"))
                {
                    // 如果某个元素包含exclude项，判断该元素当前value和exclude的value是否一致，
                    // 如果一致就清除掉所有和该元素相关的key，如果不一致则仅清除exclude项，避免ParseKey时发生类型错误
                    excludeValue = request.Form.Get(keys.ElementAt(i));
                    propertyName = keys.ElementAt(i).ToLower().Replace(".exclude", "");
                    propertyValue = request.Form.Get(propertyName);
                    if (excludeValue == propertyValue)
                    {
                        excludeProperties.Add(propertyName);
                    }
                    else
                    {
                        excludeProperties.Add(keys.ElementAt(i));
                    }
                }

            }
            var keysList = keys.ToList();
            foreach (String p in excludeProperties)
            {
                for (int i = keysList.Count() - 1; i >= 0; i--)
                {
                    if (keysList[i].ToLower().Contains(p) ||
                        keysList[i].ToLower() == p.ToLower())
                        keysList.RemoveAt(i);
                }
            }
            return keysList;
        }
        
        bool ContainKeyword(string[] segments)
        {          
            return (from s in segments select s.ToLower()).Intersect(keywords).Count() > 0;
        }

        string[] keywords = { "page","orderbyasc", "orderbydesc", "pageindex", "pagesize", "operator","valuetype" };

        Dictionary<string,QueryOperator> OptDictionary=new Dictionary<string,QueryOperator>();
    }


}