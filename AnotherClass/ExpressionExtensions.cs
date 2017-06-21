using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamicQuery
{
    public static class ExpressionExtensions
    {
        //public static CustomerDto[] SelectCustomer(CustomerSearchCriteriaDto searchCriteria,
        //             SortingPagingDto sortingPaging)
        //{
        //    using (var context = new MyContext())
        //    {
        //        var predicate = ExpressionExtensions.BuildPredicate<Customer,
        //                               CustomerSearchCriteriaDto>(searchCriteria);

        //        var query = context.Customers.AsExpandable().Where(predicate)
        //                            as IOrderedQueryable<Customer>;

        //        var dbCustomers = query.ApplySortingPaging(sortingPaging);

        //        var customerDtos = dbCustomers.ToDto();

        //        return customerDtos;
        //    }
        //}

        //public static CustomerDto[] ToDto(this Customer[] dbCustomers)
        //{
        //    var customerDtos = new CustomerDto[dbCustomers.Length];
        //    for (var i = 0; i < dbCustomers.Length; i++)
        //    {
        //        var currentDbCustomer = dbCustomer[i];
        //        var customerDto = new CustomerDto
        //        {
        //            FirstName = currentDbCustomer.firstName,
        //            LastName = currentDbCustomer.lastName,
        //            IsActive = currentDbCustomer.isActive
        //            // etc.
        //        }
        //        customerDtos[i] = customerDto;
        //    }
        //    return customerDto;
        //}

        //public class CustomerSearchCriteriaDto
        //{
        //    public string FirstName { get; set; }
        //    public string LastName { get; set; }
        //    public bool? IsActive { get; set; }
        //    public DateTime? CreatedDate { get; set; }
        //    // etc.
        //}

        public class CustomerDto
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public bool IsActive { get; set; }
            // etc.
        }

        private static readonly MethodInfo StringContainsMethod =
  typeof(string).GetMethod(@"Contains", BindingFlags.Instance |
  BindingFlags.Public, null, new[] { typeof(string) }, null);
        private static readonly MethodInfo BooleanEqualsMethod =
          typeof(bool).GetMethod(@"Equals", BindingFlags.Instance |
          BindingFlags.Public, null, new[] { typeof(bool) }, null);

        public static Expression<Func<TDbType, bool>>
          BuildPredicate<TDbType, TSearchCriteria>(TSearchCriteria searchCriteria)
        {
            var predicate = PredicateBuilder.True<TDbType>();

            // Iterate the search criteria properties
            var searchCriteriaPropertyInfos = searchCriteria.GetType().GetProperties();
            foreach (var searchCriteriaPropertyInfo in searchCriteriaPropertyInfos)
            {
                // Get the name of the DB field, which may not be the same as the property name.
                var dbFieldName = GetDbFieldName(searchCriteriaPropertyInfo);
                // Get the target DB type (table)
                var dbType = typeof(TDbType);
                // Get a MemberInfo for the type's field (ignoring case
                // so "FirstName" works as well as "firstName")
                var dbFieldMemberInfo = dbType.GetMember(dbFieldName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).Single();
                // STRINGS
                if (searchCriteriaPropertyInfo.PropertyType == typeof(string))
                {
                    predicate = ApplyStringCriterion(searchCriteria,
                      searchCriteriaPropertyInfo, dbType, dbFieldMemberInfo, predicate);
                }
                // BOOLEANS
                else if (searchCriteriaPropertyInfo.PropertyType == typeof(bool?))
                {
                    predicate = ApplyBoolCriterion(searchCriteria,
                      searchCriteriaPropertyInfo, dbType, dbFieldMemberInfo, predicate);
                }
                // ADD MORE TYPES...
            }

            return predicate;
        }

        private static Expression<Func<TDbType, bool>> ApplyStringCriterion<TDbType,
            TSearchCriteria>(TSearchCriteria searchCriteria, PropertyInfo searchCriterionPropertyInfo,
            Type dbType, MemberInfo dbFieldMemberInfo, Expression<Func<TDbType, bool>> predicate)
        {
            // Check if a search criterion was provided
            var searchString = searchCriterionPropertyInfo.GetValue(searchCriteria) as string;
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return predicate;
            }
            // Then "and" it to the predicate.
            // e.g. predicate = predicate.And(x => x.firstName.Contains(searchCriterion.FirstName)); ...
            // Create an "x" as TDbType
            var dbTypeParameter = Expression.Parameter(dbType, @"x");
            // Get at x.firstName
            var dbFieldMember = Expression.MakeMemberAccess(dbTypeParameter, dbFieldMemberInfo);
            // Create the criterion as a constant
            var criterionConstant = new Expression[] { Expression.Constant(searchString) };
            // Create the MethodCallExpression like x.firstName.Contains(criterion)
            var containsCall = Expression.Call(dbFieldMember, StringContainsMethod, criterionConstant);
            // Create a lambda like x => x.firstName.Contains(criterion)
            var lambda = Expression.Lambda(containsCall, dbTypeParameter) as Expression<Func<TDbType, bool>>;
            // Apply!
            return predicate.And(lambda);
        }

        private static Expression<Func<TDbType, bool>> ApplyBoolCriterion<TDbType,
          TSearchCriteria>(TSearchCriteria searchCriteria, PropertyInfo searchCriterionPropertyInfo,
          Type dbType, MemberInfo dbFieldMemberInfo, Expression<Func<TDbType, bool>> predicate)
        {
            // Check if a search criterion was provided
            var searchBool = searchCriterionPropertyInfo.GetValue(searchCriteria) as bool?;
            if (searchBool == null)
            {
                return predicate;
            }
            // Then "and" it to the predicate.
            // e.g. predicate = predicate.And(x => x.isActive.Contains(searchCriterion.IsActive)); ...
            // Create an "x" as TDbType
            var dbTypeParameter = Expression.Parameter(dbType, @"x");
            // Get at x.isActive
            var dbFieldMember = Expression.MakeMemberAccess(dbTypeParameter, dbFieldMemberInfo);
            // Create the criterion as a constant
            var criterionConstant = new Expression[] { Expression.Constant(searchBool) };
            // Create the MethodCallExpression like x.isActive.Equals(criterion)
            var equalsCall = Expression.Call(dbFieldMember, BooleanEqualsMethod, criterionConstant);
            // Create a lambda like x => x.isActive.Equals(criterion)
            var lambda = Expression.Lambda(equalsCall, dbTypeParameter) as Expression<Func<TDbType, bool>>;
            // Apply!
            return predicate.And(lambda);
        }

        private static string GetDbFieldName(PropertyInfo propertyInfo)
        {
            var fieldMapAttribute =
                 propertyInfo.GetCustomAttributes(typeof(FieldMapAttribute), false).FirstOrDefault();
            var dbFieldName = fieldMapAttribute != null ?
                    ((FieldMapAttribute)fieldMapAttribute).Field : propertyInfo.Name;
            return dbFieldName;
        }

        public static IQueryable<T> ApplySortingPaging<T>(this IOrderedQueryable<T> query, SortingPagingDto sortingPaging)
        {
            var firstPass = true;
            foreach (var sortOrder in sortingPaging.SortOrders)
            {
                if (firstPass)
                {
                    firstPass = false;
                    query = sortOrder.ColumnOrder == SortOrderDto.SortOrder.Ascending
                                ? query.OrderBy(sortOrder.ColumnName) :
                                  query.OrderByDescending(sortOrder.ColumnName);
                }
                else
                {
                    query = sortOrder.ColumnOrder == SortOrderDto.SortOrder.Ascending
                                ? query.ThenBy(sortOrder.ColumnName) :
                                  query.ThenByDescending(sortOrder.ColumnName);
                }
            }

            //var result = query.Skip((sortingPaging.PageNumber - 1) *
            //  sortingPaging.NumberRecords).Take(sortingPaging.NumberRecords);

            return query.AsQueryable();
        }

        // From: http://stackoverflow.com/questions/41244/dynamic-linq-orderby-on-ienumerablet
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderBy");
        }
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderByDescending");
        }
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenBy");
        }
        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenByDescending");
        }
        static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            object result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), type)
                    .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class FieldMapAttribute : Attribute
    {
        public string Field { get; set; }
        public FieldMapAttribute(string field)
        {
            Field = field;
        }
    }

    public class SortingPagingDto
    {
        public SortOrderDto[] SortOrders { get; set; }
        public int PageNumber { get; set; }
        public int NumberRecords { get; set; }
    }
    public class SortOrderDto
    {
        public enum SortOrder
        {
            Ascending,
            Descending
        }
        public string ColumnName { get; set; }
        public SortOrder ColumnOrder { get; set; }
    }

}
