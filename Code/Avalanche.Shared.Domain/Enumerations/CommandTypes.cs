using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Avalanche.Shared.Domain.Enumerations
{
    public enum CommandTypes
    {
        [Description("Pgs Play Video")]
        PgsPlayVideo = 0,
        [Description("Pgs Stop Video")]
        PgsStopVideo = 1,
        [Description("Pgs Play Audio")]
        PgsPlayAudio = 2,
        [Description("Pgs Stop Audio")]
        PgsStopAudio = 3,
        [Description("Pgs Mute Audio")]
        PgsMuteAudio = 4,
        [Description("Pgs Handle Message for Video")]
        PgsHandleMessageForVideo = 5,
        [Description("Pgs Get Audio Volume Up")]
        PgsGetAudioVolumeUp = 6,
        [Description("Pgs Get Audio Volume Down")]
        PgsGetAudioVolumeDown = 7,
        [Description("Timeout Play Pdf Slides")]
        TimeoutPlayPdfSlides = 8,
        [Description("Timeout Stop Pdf Slides")]
        TimeoutStopPdfSlides = 9,
        [Description("Timeout Next Pdf Slide")]
        TimeoutNextPdfSlide = 10,
        [Description("Timeout Previous Pdf Slide")]
        TimeoutPreviousPdfSlide = 11,
        [Description("Timeout Set Current Slide")]
        TimeoutSetCurrentSlide = 12,
        [Description("Fullscreen Mode")]
        FullScreen = 13,
        [Description("Exit Fullscreen Mode")]
        ExitFullScreen = 14
    }
}
