
namespace Avalanche.Shared.Domain.Models.Media
{
    public class RouteVideoTilingModel
    {
        public AliasIndexModel Source { get; set; }
        public AliasIndexModel Sink { get; set; }
        public int ViewportIndex { get; set; }
    }
}
