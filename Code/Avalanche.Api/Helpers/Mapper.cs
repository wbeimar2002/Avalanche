using Avalanche.Api.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Avalanche.Api.Helpers
{
    public static class Mapper
    {
        public static void Map(ExpandoObject source, object destination)
        {
            source = source ?? throw new ArgumentNullException(nameof(source));
            destination = destination ?? throw new ArgumentNullException(nameof(destination));

            string normalizeName(string name) => name.ToLowerInvariant();

            IDictionary<string, object> dict = source;
            var type = destination.GetType();

            var setters = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetSetMethod() != null)
                .ToDictionary(p => normalizeName(p.Name));

            foreach (var item in dict)
            {
                if (setters.TryGetValue(normalizeName(item.Key), out var setter))
                {
                    var value = setter.PropertyType.ChangeType(item.Value);
                    setter.SetValue(destination, value);
                }
            }
        }

    }
}
