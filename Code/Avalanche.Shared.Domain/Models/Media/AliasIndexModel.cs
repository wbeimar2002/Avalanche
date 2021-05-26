using System;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class AliasIndexModel
    {
        public string? Alias { get; set; }
        public string? Index { get; set; }

        public bool IsEmpty() => string.IsNullOrEmpty(Alias) || string.IsNullOrEmpty(Index);
    }
}
