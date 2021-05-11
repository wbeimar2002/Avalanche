using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    public class TimeoutSettingsValues
    {
        public string FileName { get; set; }
        public string TimeoutPath { get; set; }
        public Shared.Domain.Models.Media.AliasIndexModel Source { get; set; }
    }
}
