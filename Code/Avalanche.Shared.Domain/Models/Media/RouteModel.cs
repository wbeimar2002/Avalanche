namespace Avalanche.Shared.Domain.Models.Media
{
    public class RouteModel
    {
        public AliasIndexModel? Source { get; set; }
        public AliasIndexModel Sink { get; set; } = new AliasIndexModel();
    }
}
