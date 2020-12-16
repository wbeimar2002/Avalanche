using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Avalanche.Shared.Domain.Enumerations
{
    public enum CommandTypes
    {
        [Description("Pgs Play Video")]
        PgsPlayVideo = 10,
        [Description("Pgs Stop Video")]
        PgsStopVideo = 11,
        [Description("Pgs Play Audio")]
        PgsPlayAudio = 12,
        [Description("Pgs Stop Audio")]
        PgsStopAudio = 13,
        [Description("Pgs Mute Audio")]
        PgsMuteAudio = 14,
        [Description("Pgs Handle Message for Video")]
        PgsHandleMessageForVideo = 15,
        [Description("Pgs Get Audio Volume Up")]
        PgsGetAudioVolumeUp = 16,
        [Description("Pgs Get Audio Volume Down")]
        PgsGetAudioVolumeDown = 17,

        [Description("Timeout Play Pdf Slides")]
        TimeoutPlayPdfSlides = 20,
        [Description("Timeout Stop Pdf Slides")]
        TimeoutStopPdfSlides = 21,
        [Description("Timeout Next Pdf Slide")]
        TimeoutNextPdfSlide = 22,
        [Description("Timeout Previous Pdf Slide")]
        TimeoutPreviousPdfSlide = 23,
        [Description("Timeout Set Current Slide")]
        TimeoutSetCurrentSlide = 24,
        [Description("Show video routing preview")]
        SetTimeoutMode = 25,

        [Description("Enter Fullscreen Mode")]
        EnterFullScreen = 30,
        [Description("Exit Fullscreen Mode")]
        ExitFullScreen = 31,
        [Description("Route Video Source")]
        RouteVideoSource = 32,
        [Description("Unroute Video Source")]
        UnrouteVideoSource = 33,
        [Description("Show video routing preview")]
        ShowVideoRoutingPreview = 34,
        [Description("Hide video routing preview")]
        HideVideoRoutingPreview = 35,

        [Description("Start recording")]
        StartRecording = 40,
        [Description("Stop Recording")]
        StopRecording = 41,

        [Description("Capture image")]
        CaptureImage = 42,
    }
}
