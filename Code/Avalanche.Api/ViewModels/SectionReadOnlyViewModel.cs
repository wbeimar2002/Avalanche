
using Avalanche.Shared.Domain.Models;
using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class SectionReadOnlyViewModel
    {
        public string JsonKey { get; set; }
        public List<SectionReadOnlyViewModel> Sections { get; set; }
        public List<SettingReadOnlyViewModel> Settings { get; set; }
    }
}
