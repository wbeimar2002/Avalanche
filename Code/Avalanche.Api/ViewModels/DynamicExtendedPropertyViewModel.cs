using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class DynamicExtendedPropertyViewModel
    {
        public string? JsonKeyField { get; set; }
        public string? JsonKeyValue { get; set; }
        public List<string>? ReadOnlyFields { get; set; }
    }
}
