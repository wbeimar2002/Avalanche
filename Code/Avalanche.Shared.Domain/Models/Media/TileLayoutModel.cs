using System.Collections.Generic;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class TileLayoutModel
    {
        public string LayoutName { get; set; } = string.Empty;
        public IList<TileViewportModel>? ViewPorts { get; set; }

        public TileLayoutModel()
        {
        }

        public TileLayoutModel(string layoutName, IList<TileViewportModel> viewPorts)
        {
            LayoutName = layoutName;
            ViewPorts = viewPorts;
        }
    }
}
