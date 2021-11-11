namespace Avalanche.Security.Server.Entities
{
    public interface IFtsEntity
    {
        public int RowId { get; set; }
        public string Match { get; set; }
        public double? Rank { get; set; }
    }
}
