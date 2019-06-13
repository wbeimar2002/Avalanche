using Newtonsoft.Json;

namespace MonsoonAPI.Models
{
    public class AddAutolabelRequest
    {
        [JsonProperty(PropertyName = "ownerType")]
        public AutolabelOwnerType OwnerType { get; set; }

        [JsonProperty(PropertyName = "ownerName")]
        public string OwnerName { get; set; }

        [JsonProperty(PropertyName = "procedureType")]
        public string ProcedureType { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "autolabelPosition")]
        public int? AutolabelPosition { get; set; }

        [JsonProperty(PropertyName = "autolabelColor")]
        public string AutolabelColor { get; set; }
        

        public AddAutolabelRequest()
        {
        }

        public AddAutolabelRequest(AutolabelOwnerType ownerType, string ownerName, string procedureType, string label, int? position, string color)
        {
            OwnerType = ownerType;
            OwnerName = ownerName;
            ProcedureType = procedureType;
            Label = label;
            AutolabelPosition = position;
            AutolabelColor = color;
        }
    }
}
