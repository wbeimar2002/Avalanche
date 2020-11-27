
using Avalanche.Shared.Domain.Models;
using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class SettingCategoryViewModel
    {
        public SettingCategory Category { get; set; }
        public List<Setting> Settings { get; set; }
    }
}
