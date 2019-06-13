using Newtonsoft.Json;

namespace MonsoonAPI.Models
{
    public class DeleteLabelRequest
    {
        [JsonProperty(PropertyName = "ownerType")]
        public AutolabelOwnerType OwnerType { get; set; }

        [JsonProperty(PropertyName = "ownerName")]
        public string OwnerName { get; set; }

        [JsonProperty(PropertyName = "procedureType")]
        public string ProcedureType { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        public DeleteLabelRequest()
        {
        }

        public DeleteLabelRequest(AutolabelOwnerType ownerType, string ownerName, string procedureType, string label)
        {
            OwnerType = ownerType;
            OwnerName = ownerName;
            ProcedureType = procedureType;
            Label = label;
        }
    }
}
