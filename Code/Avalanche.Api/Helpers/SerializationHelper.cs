using Newtonsoft.Json;

namespace Avalanche.Api.Helpers
{
    public class SerializationHelper
    {
        public static T Get<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string Json(object fullObject)
        {
            return JsonConvert.SerializeObject(fullObject);
        }
    }
}
