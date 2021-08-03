using System.Collections.Generic;
using Avalanche.Shared.Infrastructure.Enumerations;

namespace Avalanche.Api.ViewModels
{
    public class DynamicBaseSettingViewModel
    {
        public string? LabelTranslationKey { get; set; }
        public string? DescriptionTranslationKey { get; set; }
        public string? Format { get; set; }
        public string? PlaceHolderTranslationKey { get; set; }
        public int? MaxValue { get; set; }
        public int? MinValue { get; set; }
        public int? Steps { get; set; }
        public string? DefaultValue { get; set; }
        public bool Required { get; set; }
        public int? MaxLength { get; set; }

        public string? JsonKey { get; set; }

        public SettingTypes SettingType { get; set; }
        public VisualStyles VisualStyle { get; set; }

        public string? SourceKey { get; set; }
        public string? SourceKeyId { get; set; }
        public string? SourceKeyValue { get; set; }
        public string? SourceKeyTranslationKey { get; set; }

        public IList<dynamic>? SourceValues { get; set; }

        public string? Folder { get; set; }
        public string? Filter { get; set; }
    }
}
