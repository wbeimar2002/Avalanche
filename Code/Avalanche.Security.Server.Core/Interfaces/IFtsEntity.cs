namespace Avalanche.Security.Server.Core.Interfaces
{
    public interface IFtsEntity
    {
        public int RowId { get; set; }
        public string Match { get; set; }
        public double? Rank { get; set; }
    }
}
