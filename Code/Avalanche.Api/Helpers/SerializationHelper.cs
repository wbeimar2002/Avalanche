using Newtonsoft.Json;

namespace Avalanche.Api.Helpers
{
    public class SerializationHelper
    {
        public static T Get<T>(string json) =>
            JsonConvert.DeserializeObject<T>(json);

        public static string Json(object fullObject) =>
            JsonConvert.SerializeObject(fullObject);
    }
}
