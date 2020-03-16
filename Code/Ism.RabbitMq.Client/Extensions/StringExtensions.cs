using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.RabbitMq.Client.Extensions
{
    public static class StringExtensions
    {
        public static T Get<T>(this String json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string Json<T>(this T log)
        {
            return JsonConvert.SerializeObject(log);
        }
    }
}
