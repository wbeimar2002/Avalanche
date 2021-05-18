using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Configuration.Lists
{
    /// <summary>
    /// Contains a list of AliasIndex's which represent video sinks
    /// Common data type shared with PGS, timeout, and DBR for getting a list of displays enabled for feature X
    /// </summary>
    public class SinksData
    {
        public List<AliasIndexModel> Items { get; set; }
    }
}
