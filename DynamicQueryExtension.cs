using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicQuery
{
    public static class DynamicQueryExtension
    {
        public static IQueryable<T> Query<T>(this IQueryable<T> data, QueryDescriptor descriptor)
        {
            int totalPages = 0;
            int totalItems = 0;
            return Query<T>(data, descriptor, out totalPages,out totalItems);
        }

        public static IQueryable<T> Query<T>(this IQueryable<T> data, QueryDescriptor descriptor,out int totalPages,out int totalCount)
        {
            totalPages = 0;
            totalCount = 0;
            var parser = new QueryExpressionParser<T>();
           
            //filter
            var filter = parser.Parse(descriptor);
            var result = data.Where(filter);

            //ordermuilt 多个列排序
            if (descriptor.OrderByMuilt != null)
            {
                foreach (var orderitem in descriptor.OrderByMuilt)
                {
                    Type type = typeof(T);
                    var parameter = Expression.Parameter(type);

                    var propertyInfo = type.GetProperty(orderitem.Key);
                    Expression propertySelector = Expression.Property(parameter, propertyInfo);

                    //To deal with different type of the property selector, very hacky
                    //looking for a better solution
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        var orderby = Expression.Lambda<Func<T, string>>(propertySelector, parameter);
                        if (orderitem.Order == OrderSequence.DESC)
                            result = result.OrderByDescending(orderby);
                        else
                            result = result.OrderBy(orderby);
                    }
                    else if (propertyInfo.PropertyType == typeof(decimal))
                    {
                        var orderby = Expression.Lambda<Func<T, decimal>>(propertySelector, parameter);
                        if (orderitem.Order == OrderSequence.DESC)
                            result = result.OrderByDescending(orderby);
                        else
                            result = result.OrderBy(orderby);
                    }
                    else if (propertyInfo.PropertyType == typeof(decimal?))
                    {
                        var orderby = Expression.Lambda<Func<T, decimal?>>(propertySelector, parameter);
                        if (orderitem.Order == OrderSequence.DESC)
                            result = result.OrderByDescending(orderby);
                        else
                            result = result.OrderBy(orderby);
                    }
                    else if (propertyInfo.PropertyType == typeof(int))
                    {
                        var orderby = Expression.Lambda<Func<T, int>>(propertySelector, parameter);
                        if (orderitem.Order == OrderSequence.DESC)
                            result = result.OrderByDescending(orderby);
                        else
                            result = result.OrderBy(orderby);
                    }
                    else if (propertyInfo.PropertyType == typeof(int?))
                    {
                        var orderby = Expression.Lambda<Func<T, int?>>(propertySelector, parameter);
                        if (orderitem.Order == OrderSequence.DESC)
                            result = result.OrderByDescending(orderby);
                        else
                            result = result.OrderBy(orderby);
                    }
                    else if (propertyInfo.PropertyType == typeof(double))
                    {
                        var orderby = Expression.Lambda<Func<T, double>>(propertySelector, parameter);
                        if (orderitem.Order == OrderSequence.DESC)
                            result = result.OrderByDescending(orderby);
                        else
                            result = result.OrderBy(orderby);
                    }
                    else if (propertyInfo.PropertyType == typeof(double?))
                    {
                        var orderby = Expression.Lambda<Func<T, double?>>(propertySelector, parameter);
                        if (orderitem.Order == OrderSequence.DESC)
                            result = result.OrderByDescending(orderby);
                        else
                            result = result.OrderBy(orderby);
                    }
                    else if (propertyInfo.PropertyType == typeof(float))
                    {
                        var orderby = Expression.Lambda<Func<T, float>>(propertySelector, parameter);
                        if (orderitem.Order == OrderSequence.DESC)
                            result = result.OrderByDescending(orderby);
                        else
                            result = result.OrderBy(orderby);
                    }
                    else if (propertyInfo.PropertyType == typeof(float?))
                    {
                        var orderby = Expression.Lambda<Func<T, float?>>(propertySelector, parameter);
                        if (orderitem.Order == OrderSequence.DESC)
                            result = result.OrderByDescending(orderby);
                        else
                            result = result.OrderBy(orderby);
                    }
                    else if (propertyInfo.PropertyType == typeof(DateTime))
                    {
                        var orderby = Expression.Lambda<Func<T, DateTime>>(propertySelector, parameter);
                        if (orderitem.Order == OrderSequence.DESC)
                            result = result.OrderByDescending(orderby);
                        else
                            result = result.OrderBy(orderby);
                    }
                    else if (propertyInfo.PropertyType == typeof(DateTime?))
                    {
                        var orderby = Expression.Lambda<Func<T, DateTime?>>(propertySelector, parameter);
                        if (orderitem.Order == OrderSequence.DESC)
                            result = result.OrderByDescending(orderby);
                        else
                            result = result.OrderBy(orderby);
                    }

                }


            }



            //order
            if (descriptor.OrderBy != null)
            {
                Type type = typeof(T);
                var parameter=Expression.Parameter(type);

                var propertyInfo = type.GetProperty(descriptor.OrderBy.Key);
                Expression propertySelector = Expression.Property(parameter, propertyInfo);

                //To deal with different type of the property selector, very hacky
                //looking for a better solution
                if (propertyInfo.PropertyType == typeof(string))
                {
                    var orderby = Expression.Lambda<Func<T, string>>(propertySelector, parameter);
                    if (descriptor.OrderBy.Order == OrderSequence.DESC)
                        result = result.OrderByDescending(orderby);
                    else
                        result = result.OrderBy(orderby);
                }
                else if (propertyInfo.PropertyType == typeof(decimal))
                {
                    var orderby = Expression.Lambda<Func<T, decimal>>(propertySelector, parameter);
                    if (descriptor.OrderBy.Order == OrderSequence.DESC)
                        result = result.OrderByDescending(orderby);
                    else
                        result = result.OrderBy(orderby);
                }
                else if (propertyInfo.PropertyType == typeof(decimal?))
                {
                    var orderby = Expression.Lambda<Func<T, decimal?>>(propertySelector, parameter);
                    if (descriptor.OrderBy.Order == OrderSequence.DESC)
                        result = result.OrderByDescending(orderby);
                    else
                        result = result.OrderBy(orderby);
                }
                else if (propertyInfo.PropertyType == typeof(int))
                {
                    var orderby = Expression.Lambda<Func<T, int>>(propertySelector, parameter);
                    if (descriptor.OrderBy.Order == OrderSequence.DESC)
                        result = result.OrderByDescending(orderby);
                    else
                        result = result.OrderBy(orderby);
                }
                else if (propertyInfo.PropertyType == typeof(int?))
                {
                    var orderby = Expression.Lambda<Func<T, int?>>(propertySelector, parameter);
                    if (descriptor.OrderBy.Order == OrderSequence.DESC)
                        result = result.OrderByDescending(orderby);
                    else
                        result = result.OrderBy(orderby);
                }
                else if (propertyInfo.PropertyType == typeof(double))
                {
                    var orderby = Expression.Lambda<Func<T, double>>(propertySelector, parameter);
                    if (descriptor.OrderBy.Order == OrderSequence.DESC)
                        result = result.OrderByDescending(orderby);
                    else
                        result = result.OrderBy(orderby);
                }
                else if (propertyInfo.PropertyType == typeof(double?))
                {
                    var orderby = Expression.Lambda<Func<T, double?>>(propertySelector, parameter);
                    if (descriptor.OrderBy.Order == OrderSequence.DESC)
                        result = result.OrderByDescending(orderby);
                    else
                        result = result.OrderBy(orderby);
                }
                else if (propertyInfo.PropertyType == typeof(float))
                {
                    var orderby = Expression.Lambda<Func<T, float>>(propertySelector, parameter);
                    if (descriptor.OrderBy.Order == OrderSequence.DESC)
                        result = result.OrderByDescending(orderby);
                    else
                        result = result.OrderBy(orderby);
                }
                else if (propertyInfo.PropertyType == typeof(float?))
                {
                    var orderby = Expression.Lambda<Func<T, float?>>(propertySelector, parameter);
                    if (descriptor.OrderBy.Order == OrderSequence.DESC)
                        result = result.OrderByDescending(orderby);
                    else
                        result = result.OrderBy(orderby);
                }
                else if (propertyInfo.PropertyType == typeof(DateTime))
                {
                    var orderby = Expression.Lambda<Func<T, DateTime>>(propertySelector, parameter);
                    if (descriptor.OrderBy.Order == OrderSequence.DESC)
                        result = result.OrderByDescending(orderby);
                    else
                        result = result.OrderBy(orderby);
                }
                else if (propertyInfo.PropertyType == typeof(DateTime?))
                {
                    var orderby = Expression.Lambda<Func<T, DateTime?>>(propertySelector, parameter);
                    if (descriptor.OrderBy.Order == OrderSequence.DESC)
                        result = result.OrderByDescending(orderby);
                    else
                        result = result.OrderBy(orderby);
                }
            }

            //paging
            //if (descriptor.PageSize > 0)
            //{
            //    totalCount = result.Count();
            //    totalPages = totalCount % descriptor.PageSize == 0 ? totalCount / descriptor.PageSize : totalCount / descriptor.PageSize + 1;
            //    //return result.Skip((descriptor.PageIndex - 1) * descriptor.PageSize).Take(descriptor.PageSize);
            //}
            return result;
        }
    }
}
