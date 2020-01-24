﻿using Avalanche.Shared.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class Setting
    {
        public string TranslationKey { get; set; }
        public string Format { get; set; }
        public int MaximumValue { get; set; }
        public int MinimumValue { get; set; }
        public SettingType SettingType { get; set; }
        public string Value { get; set; }
        public string DefaultLabelValue { get; set; }
        public VisualStyle VisualStyle { get; set; }
        public string SourceKey { get; set; }
    }
}
