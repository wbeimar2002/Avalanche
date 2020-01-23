using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class SettingCategory
    {
        public bool ReadOnly { get; set; }
        public string DefaultLabelValue { get; set; }
        public string TranslationKey { get; set; }
    }
}
