
using System.Collections.Generic;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class TileVideoRouteModel
    {
        public List<AliasIndexModel> Sources { get; set; } = new List<AliasIndexModel>();
        public AliasIndexModel? Sink { get; set; }
        public string LayoutName { get; set; } = string.Empty;
        public int SourceCount { get; set; }
    }
}
