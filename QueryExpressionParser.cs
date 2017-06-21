using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DynamicQuery
{
    internal class QueryExpressionParser<T>
    {
        public Expression<Func<T, bool>> Parse(QueryDescriptor descriptor)
        {
            var query = ParseInternal(descriptor.Conditions);
            return Expression.Lambda<Func<T, bool>>(query, parameter);
        }

        ParameterExpression parameter = Expression.Parameter(typeof(T));

        private Expression ParseInternal(IEnumerable<QueryCondition> conditions)
        {
            if (conditions == null || conditions.Count() == 0)
            {
                return Expression.Constant(true, typeof(bool));
            }
            else if (conditions.Count() == 1)
            {
                return ParseSingle(conditions.First());
            }
            else
            {
                Expression a = ParseSingle(conditions.First());
                Expression b = ParseInternal(conditions.Skip(1));
                return Expression.AndAlso(a, b);
            }
        }

        private Expression ParseSingle(QueryCondition condition)
        {
            ParameterExpression p = parameter;
            Expression key = ParseKey(p, condition);
            Expression value = ParseValue(condition);
            Expression method = ParseMethod(key, value, condition);
            return method;
        }

        private Expression ParseKey(ParameterExpression parameter, QueryCondition condition)
        {
            Expression p = parameter;
            var properties = condition.Key.Split('.');
            foreach (var property in properties)
                p = Expression.Property(p, property);
            return p;
        }

        private Expression ParseValue(QueryCondition condition)
        {
            Expression value = Expression.Constant(condition.Value);
            return value;
        }

        private Expression ParseMethod(Expression key, Expression value, QueryCondition condition)
        {
            switch (condition.Operator) {
                case QueryOperator.CONTAINS:
                    return Expression.Call(key, typeof(string).GetMethod("Contains"), value);
                case QueryOperator.EQUAL:
                    return Expression.Equal(key, Expression.Convert(value, key.Type)); //黎又铭 update 2016.5.27 修复类型 Nullable
                case QueryOperator.GERATER:
                    return Expression.GreaterThan(key, Expression.Convert(value, key.Type));
                case QueryOperator.GREATEROREQUAL:
                    return Expression.GreaterThanOrEqual(key, Expression.Convert(value, key.Type));
                case QueryOperator.LESS:
                    return Expression.LessThan(key, Expression.Convert(value, key.Type));
                case QueryOperator.LESSOREQUAL:
                    return Expression.LessThanOrEqual(key, Expression.Convert(value, key.Type));
                case QueryOperator.IN:
                    ConstantExpression constand = value as ConstantExpression;
                    string[] names = constand.Value.ToString().Split(',');
                    List<BinaryExpression> conditions = new List<BinaryExpression>();

                    var type = key.Type;
                    if (type == typeof(int) || type == typeof(int?)) {
                        conditions = ConvertToBinaryExpressions<int>(Convert.ToInt32, key, names);
                    }
                    else if (type == typeof(double) || type == typeof(double?)) {
                        conditions = ConvertToBinaryExpressions<double>(Convert.ToDouble, key, names);
                    }
                    else if (type == typeof(DateTime) || type == typeof(DateTime?)) {
                        conditions = ConvertToBinaryExpressions<DateTime>(Convert.ToDateTime, key, names);
                    }
                    else if (type == typeof(string)) {
                        conditions = ConvertToBinaryExpressions<string>(Convert.ToString, key, names);
                    }
                    else if (type == typeof(byte) || type == typeof(byte?)){
                        conditions = ConvertToBinaryExpressions<byte>(Convert.ToByte, key, names);
                    }
                    else if (type == typeof(float) || type == typeof(float?)) {
                        conditions = ConvertToBinaryExpressions<float>(Convert.ToSingle, key, names);
                    }
                    else if (type == typeof(char) || type == typeof(char?)) {
                        conditions = ConvertToBinaryExpressions<char>(Convert.ToChar, key, names);
                    }
                    else if (type == typeof(decimal) || type == typeof(decimal?)) {
                        conditions = ConvertToBinaryExpressions<decimal>(Convert.ToDecimal, key, names);
                    }
                    else {
                        throw new Exception("没有实现的数据类型");
                    }
                    
                    if (!conditions.Any()) {
                        throw new Exception("条件不能为空");
                    }
                    return conditions.Aggregate<Expression>((acc, condation) => Expression.OrElse(acc, condation));
                default:
                    throw new NotImplementedException();   //Operator IN is difficult to implenment. Wait a sec.....                
            }

        }

        /// <summary>
        /// 转换为表达式树集
        /// </summary>
        /// <typeparam name="Target"></typeparam>
        /// <param name="func"></param>
        /// <param name="key"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        private List<BinaryExpression> ConvertToBinaryExpressions<Target>(Func<object, Target> func, Expression key, params string[] names) {
            var equals = new List<BinaryExpression>();

            foreach (var item in names) {
                if (item.ToLower() == "null") {
                    equals.Add(Expression.Equal(key, Expression.Constant(null)));
                    continue;
                }

                Target result = func(item);
                var t = Expression.Equal(key, Expression.Convert(Expression.Constant(result), key.Type));
                equals.Add(t);
            }
            return equals;
        }
    }
}
