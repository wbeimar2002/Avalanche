
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class SettingCategoryViewModel
    {
        public SettingCategory Category { get; set; }
        public List<Setting> Settings { get; set; }
    }
}
