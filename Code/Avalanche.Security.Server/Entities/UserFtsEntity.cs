using System.Diagnostics.CodeAnalysis;
using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Entities
{
    [ExcludeFromCodeCoverage]
    public class UserFtsEntity : IFtsEntity
    {
        public UserFtsEntity(
            int rowId,
            string match,
            double? rank)
        {
            RowId = rowId;
            Match = match;
            Rank = rank;
        }

        public UserEntity User { get; set; }

        public int RowId { get; set; }
        public string Match { get; set; }
        public double? Rank { get; set; }
    }
}
