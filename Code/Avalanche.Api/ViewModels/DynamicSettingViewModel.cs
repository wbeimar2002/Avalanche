using Avalanche.Shared.Infrastructure.Enumerations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;

namespace Avalanche.Api.ViewModels
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DynamicSettingViewModel : DynamicBaseSettingViewModel
    {
        public string Policy { get; set; }
        public IList<KeyValuePairViewModel> PoliciesValues { get; set; }

        public VisualStyles SourceVisualStyle { get; set; }

        public DynamicListViewModel CustomList { get; set; }
        
        public IList<DynamicDependencySettingViewModel> Dependencies { get; set; }

        public string Value { get; set; }
        
        public string ActionEndPoint { get; set; }
        public string ActionMethod { get; set; }
        public string ActionTranslationKey { get; set; }
    }
}
