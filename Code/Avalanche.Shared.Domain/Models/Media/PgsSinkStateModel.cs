namespace Avalanche.Shared.Domain.Models.Media
{
    /// <summary>
    /// Represents the checkbox state next to a pgs display
    /// </summary>
    public class PgsSinkStateModel
    {
        public AliasIndexModel Sink { get; set; } = new AliasIndexModel();

        public bool Enabled { get; set; } = true;
    }
}
