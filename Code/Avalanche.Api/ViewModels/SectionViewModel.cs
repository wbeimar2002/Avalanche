
using Avalanche.Shared.Domain.Models;
using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class SectionViewModel
    {
        public string TitleTranslationKey { get; set; }
        public string JsonKey { get; set; }
        public List<SectionViewModel> Sections { get; set; }
        public List<SettingViewModel> Settings { get; set; }
    }
}
