using Avalanche.Shared.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Avalanche.Shared.Domain.Models
{
    public class Device
    {
        /// <summary>
        /// Alias component of an AliasIndex
        /// </summary>
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsVisible { get; set; }
        public int PositionInScreen { get; set; }

        /// <summary>
        /// Index component of an AliasIndex
        /// </summary>
        public int InternalIndex { get; set; }

        public string Type { get; set; }
    }
}
