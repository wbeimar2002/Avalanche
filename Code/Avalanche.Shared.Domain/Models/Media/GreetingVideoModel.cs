using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class GreetingVideoModel
    {
        public int Index { get; set; } = 0;
        public string FilePath { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
