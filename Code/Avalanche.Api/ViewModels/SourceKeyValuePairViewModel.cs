using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Infrastructure.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class SourceKeyValuePairViewModel : KeyValuePairViewModel
    {
        public string Name { get; set; }
        public SettingTypes Type { get; set; }
        public List<KeyValuePairViewModel> Types { get; set; }
    }
}
