using Avalanche.Shared.Domain.Models.Media;
using Ism.Common.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    public class TimeoutConfiguration : IConfigurationPoco
    {
        public string FileName { get; set; }
        public string TimeoutPath { get; set; }
        public Shared.Domain.Models.Media.AliasIndexModel Source { get; set; }

        public bool Validate()
        {
            return true;
        }
    }
}
