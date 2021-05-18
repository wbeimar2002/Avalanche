using Avalanche.Shared.Domain.Models.Media;
using Ism.Common.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    public class PgsConfiguration : IConfigurationPoco
    {
        public string MediaPath { get; set; }
        public PgsSettingsValuesPlayer Player { get; set; }
        public Shared.Domain.Models.Media.AliasIndexModel Source { get; set; }

        public bool Validate()
        {
            return true;
        }
    }

    public class PgsSettingsValuesPlayer
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
