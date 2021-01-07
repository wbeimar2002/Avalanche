using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Infrastructure.Enumerations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class SourceKeyValuePairViewModel : KeyValuePairViewModel
    {
        public SettingTypes Type { get; set; }

        [JsonProperty(nameof(Types))]
        public List<KeyValuePairViewModel> TypesSetter { set { _types = value; } }

        [JsonIgnore]
        private List<KeyValuePairViewModel> _types;

        [JsonIgnore]
        public List<KeyValuePairViewModel> Types
        {
            get { return this._types; }
            set { this._types = value; }
        }
    }
}
