using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Enumerations
{
    public enum RecorderState
    {
        unknown = 0,
        idle = 1,
        proc_not_recording = 2,
        proc_recording_mov = 3,
        proc_recording_pm = 4,
        proc_recording_mov_and_pm = 5,
        proc_usb_exporting = 6,
        proc_saving = 7,
        proc_discarding = 8
    }
}
