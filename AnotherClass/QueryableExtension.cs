﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DynamicQuery
{
    /// <summary>
    /// 来自“entity framework 6 自己编写的通用数据类”
    /// http://blog.csdn.net/laokaizzz/article/details/25730813
    /// </summary>
    public static class QueryableExtension
    {
        /// <summary>  
        /// 扩展方法  支持 in 操作  
        /// </summary>  
        /// <typeparam name="TEntity">需要扩展的对象类型</typeparam>  
        /// <typeparam name="TValue">in 的值类型</typeparam>  
        /// <param name="source">需要扩展的对象</param>  
        /// <param name="valueSelector">值选择器 例如c=>c.UserId</param>  
        /// <param name="values">值集合</param>  
        /// <returns></returns>  
        public static IQueryable<TEntity> WhereIn<TEntity, TValue>(this IQueryable<TEntity> source,
            Expression<Func<TEntity, TValue>> valueSelector,
                IEnumerable<TValue> values)
        {
            if (null == valueSelector) { throw new ArgumentNullException("valueSelector"); }
            if (null == values) { throw new ArgumentNullException("values"); }
            ParameterExpression p = valueSelector.Parameters.Single();

            if (!values.Any())
            {
                return new List<TEntity>().AsQueryable();
            }
            var equals = values.Select(value => (Expression)Expression.Equal(valueSelector.Body, Expression.Constant(value, typeof(TValue))));
            var body = equals.Aggregate<Expression>((accumulate, equal) => Expression.Or(accumulate, equal));
            return source.Where(Expression.Lambda<Func<TEntity, bool>>(body, p));
        }
    }
}
