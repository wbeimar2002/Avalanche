using Avalanche.Shared.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class Setting
    {
        public string Key { get; set; }
        public string Format { get; set; }
        public int MaximumValue { get; set; }
        public int MinimumValue { get; set; }
        public SettingType SettingType { get; set; }
        public string Value { get; set; }
    }
}
