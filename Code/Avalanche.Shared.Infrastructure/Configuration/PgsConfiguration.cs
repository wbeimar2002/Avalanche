using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    public class PgsConfiguration
    {
        public string MediaPath { get; set; }
        public PgsSettingsValuesPlayer Player { get; set; }
        public Shared.Domain.Models.Media.AliasIndexModel Source { get; set; }
    }

    public class PgsSettingsValuesPlayer
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
