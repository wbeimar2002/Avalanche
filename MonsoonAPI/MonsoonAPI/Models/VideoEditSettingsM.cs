using System;
using IsmDmag;
using System.Diagnostics;

namespace MonsoonAPI.models
{

  
    public enum AutoEditStatusM
    {
        raw,
        interactive_processed,
        other
    }

    public class VideoEditSettingsM
    {
        public AutoEditStatusM autoedit_status { get; }
        public ProcMovieInfoM.EMovieModeM edited_output { get; }
        public double video_before_marker { get; }
        public double video_after_marker { get; }
        //public string autoedit_mode = "off";

        public VideoEditSettingsM(AutoEditSettings settings)
        {
            try
            {
                if (settings == null)
                    return;

                autoedit_status = ToEditStatusM(settings.VideoEditStatus);
                edited_output = ProcMovieInfoM.FromRecMode(settings.AEGenerationMode);
                video_before_marker = settings.AEPreLenSecs;
                video_after_marker = settings.AEPostLenSecs;
                //autoedit_mode = settings.AEMode == AutoEditSettings.eAutoEditMode.none
                    //? AutoEditModeM.off : AutoEditModeM.on;
            }
            catch (Exception e)
            {
                IsmLogCommon.IsmLog.LogEvent(EventLogEntryType.Error, 0, "VideoEditSettingsM constructor err: " + e.Message, 3);
            }
        }

        public static AutoEditStatusM ToEditStatusM(AutoEditSettings.EVideoEditStatus status)
        {
            switch (status)
            {
                case AutoEditSettings.EVideoEditStatus.ActivelyEditing:
                case AutoEditSettings.EVideoEditStatus.EditedWorkNow:
                    return AutoEditStatusM.interactive_processed;
                case AutoEditSettings.EVideoEditStatus.Unedited:
                case AutoEditSettings.EVideoEditStatus.EditedWaitingForTimer:
                    return AutoEditStatusM.raw;
                default:
                    return AutoEditStatusM.other;
            }

        }
     
    }
}
