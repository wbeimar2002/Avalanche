using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    public class PgsSettingsValues
    {
        public PgsSettingsValuesConfiguration Configuration { get; set; }
    }

    public class PgsSettingsValuesPlayer
    {
        public string X { get; set; }
        public string Y { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
    }

    public class PgsSettingsValuesConfiguration
    {
        public PgsSettingsValuesPlayer Player { get; set; }
        public SinkModel Source { get; set; }
    }
}
