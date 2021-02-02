using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    /// <summary>
    /// Domain model fpr querying video sources and sinks from the hardware
    /// </summary>
    public class AliasIndexApiModel
    {
        public string Alias { get; set; } = string.Empty;
        public int Index { get; set; } = 0;

        public AliasIndexApiModel() { }

        public AliasIndexApiModel(string alias, int index) => (Alias, Index) = (alias, index);
    }
}
