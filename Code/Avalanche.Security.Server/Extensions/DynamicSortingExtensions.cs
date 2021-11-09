using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace Avalanche.Security.Server.Extensions
{
    public static class DynamicSortingExtensions
    {
        //Reference: https://grauenwolf.github.io/DotNet-ORM-Cookbook/DynamicSorting.htm
        //Inspired by https://stackoverflow.com/a/31959568/5274

        private static readonly MethodInfo OrderByDescendingMi = typeof(Queryable)
            .GetMethods()
            .Single(m => m.Name == "OrderByDescending" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);

        private static readonly MethodInfo OrderByMi = typeof(Queryable)
            .GetMethods()
            .Single(m => m.Name == "OrderBy" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);

        private static readonly MethodInfo ThenByDescendingMi = typeof(Queryable)
            .GetMethods()
            .Single(m => m.Name == "ThenByDescending" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);

        private static readonly MethodInfo ThenByMi = typeof(Queryable)
            .GetMethods()
            .Single(m => m.Name == "ThenBy" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> query, string propertyName) =>
            BuildQuery(OrderByMi, query, propertyName);

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> query, string propertyName, bool isDescending)
        {
            if (isDescending)
            {
                return BuildQuery(OrderByDescendingMi, query, propertyName);
            }

            return BuildQuery(OrderByMi, query, propertyName);
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> query, List<string> properties, bool isDescending)
        {
            var propertyName = properties[0];
            var queryOrdered = OrderBy(query, propertyName, isDescending);

            properties.RemoveAt(0);

            foreach (var property in properties)
            {
                queryOrdered = ThenBy(queryOrdered, property, isDescending);
            }

            return queryOrdered;
        }

        public static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IQueryable<TSource> query, string propertyName) =>
            BuildQuery(OrderByDescendingMi, query, propertyName);

        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IQueryable<TSource> query, string propertyName,
                            bool isDescending)
        {
            if (isDescending)
            {
                return BuildQuery(ThenByDescendingMi, query, propertyName);
            }

            return BuildQuery(ThenByMi, query, propertyName);
        }

        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IQueryable<TSource> query, string propertyName) =>
            BuildQuery(ThenByMi, query, propertyName);

        public static IOrderedQueryable<TSource> ThenByDescending<TSource>(this IQueryable<TSource> query,
            string propertyName) => BuildQuery(ThenByDescendingMi, query, propertyName);

        private static IOrderedQueryable<TSource> BuildQuery<TSource>(MethodInfo method, IQueryable<TSource> query,
            string propertyName)
        {
            var entityType = typeof(TSource);

            var propertyInfo = entityType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentOutOfRangeException(nameof(propertyName), "Unknown column " + propertyName);
            }

            var arg = Expression.Parameter(entityType, "x");
            var property = Expression.Property(arg, propertyName);
            var selector = Expression.Lambda(property, new ParameterExpression[] { arg });

            var genericMethod = method.MakeGenericMethod(entityType, propertyInfo.PropertyType);

            return (IOrderedQueryable<TSource>)genericMethod.Invoke(genericMethod, new object[] { query, selector })!;
        }
    }
}
