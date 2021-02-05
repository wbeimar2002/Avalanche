using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Infrastructure.Enumerations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class DynamicSourceKeyValuePairViewModel : KeyValuePairViewModel
    {
        public string Type { get; set; }
        public IList<KeyValuePairViewModel> Types { get; set; }
    }
}
