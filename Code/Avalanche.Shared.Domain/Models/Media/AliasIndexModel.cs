using System;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class AliasIndexModel
    {
        public string? Alias { get; set; }
        public string? Index { get; set; }

        public bool IsEmpty() => string.IsNullOrEmpty(Alias) || string.IsNullOrEmpty(Index);

        public override bool Equals(object? obj) =>
            obj is AliasIndexModel other &&
                   string.Equals(Alias, other.Alias, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Index, other.Index, StringComparison.OrdinalIgnoreCase);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Alias, StringComparer.OrdinalIgnoreCase);
            hash.Add(Index, StringComparer.OrdinalIgnoreCase);
            return hash.ToHashCode();
        }

        public static bool operator ==(AliasIndexModel left, AliasIndexModel right) => left.Equals(right);

        public static bool operator !=(AliasIndexModel left, AliasIndexModel right) => !(left == right);
    }
}
