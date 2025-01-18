using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Model.Helpers
{
    public static class LinqExtensions
    {
        // Entity Framework throws exception of order is done by a value type.
        // In that case the following exception will be thrown:
        //  Unable to cast the type 'System.Guid' to type 'System.Object'. LINQ to Entities only supports casting EDM primitive or enumeration types.
        // This method transforms the <T, object> expressions into <T, ConcreteValueType> expressions.
        // For more details, see: http://stackoverflow.com/questions/1145847/entity-framework-linq-to-entities-only-supports-casting-entity-data-model-primi
        public static IQueryable<T> OrderByWithExpressionTransform<T>(this IQueryable<T> queryable, List<OrderByModel<T>> orderBy) where T : class
        {
            if (queryable == null) throw new ArgumentException(nameof(queryable));

            if (!orderBy.Any()) return queryable;

            var firstOrderBy = orderBy.First();
            var orderedQueryable = queryable.OrderByWithExpressionTransform(firstOrderBy);

            if (orderBy.Count > 1)
            {
                orderedQueryable = orderBy.Skip(1)
                    .Aggregate(orderedQueryable, (current, orderByItem) => queryable.OrderByWithExpressionTransform(orderByItem));
            }

            return orderedQueryable;
        }

        public static IOrderedQueryable<T> OrderByWithExpressionTransform<T>(this IQueryable<T> queryable, OrderByModel<T> orderByItem) where T : class
        {
            var unaryExpression = orderByItem.Expression.Body as UnaryExpression;
            if (unaryExpression == null) return orderByItem.Ascending ? queryable.OrderBy(orderByItem.Expression) : queryable.OrderByDescending(orderByItem.Expression);

            var propertyExpression = unaryExpression.Operand;
            if (propertyExpression is MemberExpression) propertyExpression = (MemberExpression)propertyExpression;
            var parameters = orderByItem.Expression.Parameters;

            if (propertyExpression.Type == typeof(string))
            {
                var newExpression = Expression.Lambda<Func<T, string>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }

            if (propertyExpression.Type == typeof(bool))
            {
                var newExpression = Expression.Lambda<Func<T, bool>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }

            if (propertyExpression.Type == typeof(bool?))
            {
                var newExpression = Expression.Lambda<Func<T, bool?>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }

            if (propertyExpression.Type == typeof(Guid))
            {
                var newExpression = Expression.Lambda<Func<T, Guid>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }

            if (propertyExpression.Type == typeof(int))
            {
                var newExpression = Expression.Lambda<Func<T, int>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }

            if (propertyExpression.Type == typeof(int?))
            {
                var newExpression = Expression.Lambda<Func<T, int?>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }

            if (propertyExpression.Type == typeof(long))
            {
                var newExpression = Expression.Lambda<Func<T, int>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }

            if (propertyExpression.Type == typeof(double))
            {
                var newExpression = Expression.Lambda<Func<T, double>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }
            if (propertyExpression.Type == typeof(double?))
            {
                var newExpression = Expression.Lambda<Func<T, double?>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }
            if (propertyExpression.Type == typeof(float))
            {
                var newExpression = Expression.Lambda<Func<T, float>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }

            if (propertyExpression.Type == typeof(decimal))
            {
                var newExpression = Expression.Lambda<Func<T, float>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }

            if (propertyExpression.Type == typeof(decimal?))
            {
                var newExpression = Expression.Lambda<Func<T, decimal?>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }

            if (propertyExpression.Type == typeof(DateTime))
            {
                var newExpression = Expression.Lambda<Func<T, DateTime>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }

            if (propertyExpression.Type == typeof(DateTime?))
            {
                var newExpression = Expression.Lambda<Func<T, DateTime?>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }


            if (propertyExpression.Type == typeof(DateTimeOffset))
            {
                var newExpression = Expression.Lambda<Func<T, DateTimeOffset>>(propertyExpression, parameters);
                return orderByItem.Ascending ? queryable.OrderBy(newExpression) : queryable.OrderByDescending(newExpression);
            }

            throw new NotSupportedException(string.Format("Expression of the type 'Expression<Func<{0}, System.Object>>' cannot be transformed to " +
                                                          "an expression of the type 'Expression<Func<{0}, {1}>>'. " +
                                                          "Expression transformation for the type '{1}' is currently not supported.", typeof(T), propertyExpression.Type));
        }

    }
}
