using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class SettingCategory
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public bool ReadOnly { get; set; }
    }
}
