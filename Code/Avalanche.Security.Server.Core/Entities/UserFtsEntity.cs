using System.Diagnostics.CodeAnalysis;
using Avalanche.Security.Server.Core.Interfaces;

namespace Avalanche.Security.Server.Core.Entities
{
    [ExcludeFromCodeCoverage]
    public class UserFtsEntity : IFtsEntity
    {
        public UserFtsEntity(
            int rowId,
            string match,
            double? rank
        )
        {
            RowId = rowId;
            Match = match;
            Rank = rank;
        }

        /// <summary>
        /// Overriding nullable reference warning with the null-forgiving operater, since EF can't use constructors to initialize navigation properties
        /// https://docs.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types#non-nullable-properties-and-initialization
        /// </summary>
        public UserEntity User { get; set; } = null!;
        public int RowId { get; set; }
        public string Match { get; set; }
        public double? Rank { get; set; }
    }
}
