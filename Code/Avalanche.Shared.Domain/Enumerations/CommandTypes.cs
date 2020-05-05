using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Enumerations
{
    public enum CommandTypes
    {
        PlayVideo = 0,
        StopVideo = 1,
        PlayAudio = 2,
        StopAudio = 3,
        MuteAudio = 4,
        HandleMessageForVideo = 5,
        GetVolumeUp = 6,
        GetVolumeDown = 7,
        PlaySlides = 8,
        StopSlides = 9,
        NextSlide = 10,
        PreviousSlide = 11
    }
}
