using Newtonsoft.Json;

namespace MonsoonAPI.Models {
    public class IpAddressValidationRequest {
        [JsonProperty(PropertyName = "login")]
        public string Login { get; private set; }
        [JsonProperty(PropertyName = "ipAddress")]
        public string IpAddress { get; private set; }
    }
}