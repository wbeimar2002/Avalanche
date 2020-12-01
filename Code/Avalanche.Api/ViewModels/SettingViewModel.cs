using Avalanche.Shared.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.ViewModels
{
    public class SettingViewModel
    {
        public string LabelTranslationKey { get; set; }
        public string Format { get; set; }
        public int MaximumValue { get; set; }
        public int MinimumValue { get; set; }
        public SettingTypes SettingType { get; set; }
        public string Value { get; set; }
        public VisualStyles VisualStyle { get; set; }
        public string SourceKey { get; set; }
        public bool ReadOnly { get; set; }

        public List<SettingViewModel> Dependencies { get; set; }
        public string JsonKey { get; set; }
    }
}
