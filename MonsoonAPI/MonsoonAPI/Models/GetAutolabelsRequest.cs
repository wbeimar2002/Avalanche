using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonsoonAPI.Models
{
    public class GetAutolabelsRequest
    {
        [JsonProperty(PropertyName = "ownerType")]
        public AutolabelOwnerType OwnerType { get; set; }

        [JsonProperty(PropertyName = "ownerName")]
        public string OwnerName { get; set; }

        [JsonProperty(PropertyName = "procedureType")]
        public string ProcedureType { get; set; }

        public GetAutolabelsRequest()
        {
        }

        public GetAutolabelsRequest(AutolabelOwnerType ownerType, string ownerName, string procedureType)
        {
            OwnerType = ownerType;
            OwnerName = ownerName;
            ProcedureType = procedureType;
        }
    }
}
