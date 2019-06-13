using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IsmLogCommon;
using IsmRec.Types;
using IsmStateServer;

namespace MonsoonAPI.models
{
    public class StartRecInfo : RecInfoM
    {
        // ReSharper disable once InconsistentNaming
        public IEnumerable<string> m_ActiveVideoThumbnails;
        public Dictionary<string, string> Files2Streams; // from what I can tell, this is ONLY used for ACTIVE THUMBS!!!
        public Dictionary<string, DateTimeOffset> Files2Timestamps;

        public StartRecInfo(List<VideoData> allVideo)
        {
            try
            {
                m_Trigger = ERecEvent.StartRecord;
                InitRecState(eRecordState.proc_recording_mov);

                // fill in
                IEnumerable<VideoData> activeVideo = allVideo.Where(vid => vid.m_bFinished == false);
                var videoDatas = activeVideo as VideoData[] ?? activeVideo.ToArray();
                m_ActiveVideoThumbnails = videoDatas.Select(vid => vid.m_strPath);
                Files2Streams = videoDatas.ToDictionary(vid => vid.m_strPath, vid => vid.m_strEncoderName, StringComparer.OrdinalIgnoreCase);
                Files2Timestamps = videoDatas.ToDictionary(vid => vid.m_strPath, vid => vid.Timestamp, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                IsmLog.LogEvent(EventLogEntryType.Error, 0, "StartRecInfo failed to initialize: " + ex.Message, 1);
            }
        }
    }
}
