using Newtonsoft.Json;

namespace MonsoonAPI.Models
{
    public class EditLabelRequest
    {
        [JsonProperty(PropertyName = "ownerType")]
        public AutolabelOwnerType OwnerType { get; set; }

        [JsonProperty(PropertyName = "ownerName")]
        public string OwnerName { get; set; }

        [JsonProperty(PropertyName = "procedureType")]
        public string ProcedureType { get; set; }

        [JsonProperty(PropertyName = "oldLabel")]
        public string OldLabel { get; set; }

        [JsonProperty(PropertyName = "newLabel")]
        public string NewLabel { get; set; }

        [JsonProperty(PropertyName = "newIsAutolabel")]
        public bool? NewIsAutolabel { get; set; }

        [JsonProperty(PropertyName = "newAutolabelPosition")]
        public int? NewAutolabelPosition { get; set; }

        [JsonProperty(PropertyName = "newAutolabelColor")]
        public string NewAutolabelColor { get; set; }

        public EditLabelRequest()
        {
        }

        public EditLabelRequest(AutolabelOwnerType ownerType, string ownerName, string procedureType, string oldLabel, string newLabel, bool? newIsAutolabel, int? newAutolabelPosition, string newAutolabelColor)
        {
            OwnerType = ownerType;
            OwnerName = ownerName;
            ProcedureType = procedureType;
            OldLabel = oldLabel;
            NewLabel = newLabel;
            NewIsAutolabel = newIsAutolabel;
            NewAutolabelPosition = newAutolabelPosition;
            NewAutolabelColor = newAutolabelColor;
        }
    }
}
