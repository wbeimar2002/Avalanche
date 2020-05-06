using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static T Get<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string Json(this object fullObject)
        {
            return JsonConvert.SerializeObject(fullObject);
        }
    }
}
