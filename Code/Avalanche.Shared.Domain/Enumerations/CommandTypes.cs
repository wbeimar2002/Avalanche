using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Enumerations
{
    public enum CommandTypes
    {
        PgsPlayVideo = 0,
        PgsStopVideo = 1,
        PgsPlayAudio = 2,
        PgsStopAudio = 3,
        PgsMuteAudio = 4,
        PgsHandleMessageForVideo = 5,
        PgsGetAudioVolumeUp = 6,
        PgsGetAudioVolumeDown = 7,
        TimeoutPlayPdfSlides = 8,
        TimeoutStopPdfSlides = 9,
        TimeoutNextPdfSlide = 10,
        TimeoutPreviousPdfSlide = 11,
        TimeoutSetCurrentSlide = 12
    }
}
