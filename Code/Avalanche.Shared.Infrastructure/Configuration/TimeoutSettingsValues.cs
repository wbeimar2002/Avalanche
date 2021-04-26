using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    public class TimeoutSettingsValues
    {
        public TimeoutSettingsValuesConfiguration Configuration { get; set; }
    }

    public class TimeoutSettingsValuesMode
    {
        public int TimeoutMode { get; set; }
        public string FileName { get; set; }
        public string ImageFolder { get; set; }
    }

    public class TimeoutSettingsValuesSlideShowDuration
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
    }

    public class TimeoutSettingsValuesConfiguration
    {
        public TimeoutSettingsValuesMode Mode { get; set; }
        public TimeoutSettingsValuesSlideShowDuration SlideShowDuration { get; set; }
        public SinkModel Source { get; set; }
    }
}
