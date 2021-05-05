using Avalanche.Shared.Domain.Models.Media;

namespace Avalanche.Api.ViewModels
{
    public class RoutingPreviewViewModel : FullScreenRequestViewModel
    {
        /// <summary>
        /// Preview index. Will be used on MX as it will have 2 preview windows
        /// </summary>
        public int Index { get; set; }
        public RegionModel Region { get; set; }
    }
}
