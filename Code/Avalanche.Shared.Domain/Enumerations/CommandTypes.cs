using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Enumerations
{
    public enum CommandTypes
    {
        PlayVideo,
        StopVideo,
        PlayAudio,
        StopAudio,
        MuteAudio,
        HandleMessage,
        GetVolumeUp,
        GetVolumeDown,
        PlaySlides,
        StopSlides,
        NextSlide,
        PreviousSlide
    }
}
