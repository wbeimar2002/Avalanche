using ISM.Middleware2Si;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MonsoonAPI.Models
{
    public class SetAutolabelsRequest
    {
        [JsonProperty(PropertyName = "ownerType")]
        public AutolabelOwnerType OwnerType { get; set; }

        [JsonProperty(PropertyName = "ownerName")]
        public string OwnerName { get; set; }

        [JsonProperty(PropertyName = "procedureType")]
        public string ProcedureType { get; set; }

        [JsonProperty(PropertyName = "labels")]
        public List<AutoLabelInfo> Labels { get; set; }
        
        public SetAutolabelsRequest()
        {
        }

        public SetAutolabelsRequest(AutolabelOwnerType ownerType, string ownerName, string procedureType, List<AutoLabelInfo> labels)
        {
            OwnerType = ownerType;
            OwnerName = ownerName;
            ProcedureType = procedureType;
            Labels = labels;
        }
    }

    public enum AutolabelOwnerType
    {
        Department = 0,
        User = 1
    }
}
