using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MyBackend.Application.Common.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public static IQueryable<T> ApplySorting<T>(
            this IQueryable<T> query,
            string? sortBy,
            string? sortOrder,
            string defaultProperty = "Id")
        {
            var isDescending = string.Equals(sortOrder?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);
            var propertyName = !string.IsNullOrWhiteSpace(sortBy) ? sortBy.Trim() : defaultProperty;

            var property = typeof(T).GetProperty(
                propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
            {
                property = typeof(T).GetProperty(
                    defaultProperty,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            }

            if (property == null)
            {
                return query;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);

            var methodName = isDescending ? "OrderByDescending" : "OrderBy";
            var resultExp = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), property.PropertyType },
                query.Expression,
                Expression.Quote(orderByExp));

            return query.Provider.CreateQuery<T>(resultExp);
        }
    }
}
