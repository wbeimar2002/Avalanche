using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using IsmRec.Types;
using IsmStateServer;
using IsmRecSi;
using IsmUtility;

namespace MonsoonAPI.models
{
    public class RecInfoM
    {
        public class RecChanInfoMonsoon
        {
            // ReSharper disable once InconsistentNaming
            public double m_dMovLength ;
            // ReSharper disable once InconsistentNaming
            public bool m_bEnabled;

            public string name;

            public RecChanInfoMonsoon(string name, double length, bool enabled)
            {
                this.name = name;
                m_dMovLength = length;
                m_bEnabled = enabled;
            }
        }

        // much of this enum doesn't really make sense, but that's to accomodate existing Monsoon code :/
        public enum ERecEvent
        {
            OnDemand, NewProc, ProcModified, FileSwitch, StartRecord, StopRecord,
            Finishing, UsbFail, Finished, ChannelEnable, ImageDeleted, VideoDeleted,
            ImageCaptured, MarkerAdded, StartBackgroundRecord, StopBackgroundRecord,
            Other
        }

        // ReSharper disable once InconsistentNaming
        protected ERecEvent m_Trigger;
        // ReSharper disable once InconsistentNaming
        public int m_RecordState ;

        public int Trigger => (int)m_Trigger;

        internal RecInfoM()
        {
        }

        public RecInfoM(IssEventCodes code)
        {
            m_RecordState = 2;
            switch (code)
            {
                case IssEventCodes.recorder_start_pm_rec: m_Trigger = ERecEvent.StartBackgroundRecord; break;
                case IssEventCodes.recorder_stop_pm_rec: m_Trigger = ERecEvent.StopBackgroundRecord; break;
                default: m_Trigger = ERecEvent.Other; break;
            }
        }

        protected IEnumerable<RecChanInfoMonsoon> GetChannelInfos(Dictionary<string, RecChanInfo> channelInfos, 
            Dictionary<string, double> activeDurations, IEnumerable<VideoData> doneVideo, bool bIncludeOldState)
        {
            // translate current state into what UI expects
            var channels =  channelInfos.Select(chan =>
            {
                RecChanInfoMonsoon chanInfo = new RecChanInfoMonsoon(chan.Key, 0, chan.Value.m_bEnabled);

                // add active
                if (activeDurations.ContainsKey(chan.Key))
                    chanInfo.m_dMovLength = activeDurations[chan.Key];

                // add done
                IEnumerable<VideoData> doneVideoThisChan = doneVideo.Where(vid => vid.m_strEncoderName.Equals(chan.Key));
                double doneDurations = doneVideoThisChan.Select(vid => vid.m_dSeconds).Sum();
                chanInfo.m_dMovLength += doneDurations;

                return chanInfo;
            });

            // if we don't have to include 'old state', we're done!
            if (!bIncludeOldState)
                return channels;

            // now (and yes, this is very very very ugly!), capture the state prior to change 
            var oldChannels = channelInfos.Select(chan =>
                new RecChanInfoMonsoon(chan.Key, chan.Value.MovieDiscarded ? 1 : 0, chan.Value.PreviouslySelected));

            // return concatenation of the two
            return channels.Concat(oldChannels);
        }

        /// <summary>
        /// Implementation looks like this because that's what Monsoon expects
        /// </summary>
        protected void InitRecState(eRecordState state)
        {
            if (RsUtils.IsMovRecording(state))
                m_RecordState = 3;
            else if (state != eRecordState.proc_usb_exporting && state != eRecordState.proc_discarding && state != eRecordState.proc_saving)
                m_RecordState = 2;
            else
                m_RecordState = 4;
        }
    }

 
    public class RecInfoChannelChange : RecInfoM
    {
        public IEnumerable<RecChanInfoMonsoon> ChannelInfos;
        // ReSharper disable once InconsistentNaming
        public IEnumerable<string> m_ActiveVideoThumbnails;
        // ReSharper disable once InconsistentNaming
        public IEnumerable<string> m_Videos;
        public Dictionary<string, string> Files2Streams; // from what I can tell, this is ONLY used for ACTIVE THUMBS!!!
        public Dictionary<string, DateTimeOffset> Files2Timestamps;

        public RecInfoChannelChange(IMonsoonResMgr rm, SubscriberInfoWeb.EventMonsoon evt)
        {
            Dictionary<IssDataCodes, object> dataMap ;
            Dictionary<string, RecChanInfo> channelsInfo = new Dictionary<string, RecChanInfo>(StringComparer.OrdinalIgnoreCase);
            try
            {
                m_Trigger = ERecEvent.ChannelEnable;

                // channels Info comes from evt             
                var obj = (Newtonsoft.Json.Linq.JObject) evt.SerializedObj[0];
                channelsInfo.AssignFrom(obj.ToObject<Dictionary<string, RecChanInfo>>());

                // the rest comes from SS
                IssDataCodes[] codes =
                {
                    IssDataCodes.recorder_session_state,
                    IssDataCodes.recorder_videos,
                    IssDataCodes.recorder_active_video_duration
                };
                if (rm.GetIssDataMap(codes, out dataMap) != 0)
                    throw new Exception("Failed to retrieve SS map");
            }
            catch (Exception ex)
            {
                rm.LogEvent(EventLogEntryType.Error, 0, "RecInfoChannelChange failed to initialize: " + ex.Message, 1);
                return;
            }

            try
            {
                // state (if not active - done; shouldn't happen though...)
                var recordState = (eRecordState)dataMap[IssDataCodes.recorder_session_state];
                if (!RsUtils.IsActive(recordState))
                    return;
                InitRecState(recordState);
            }
            catch (Exception ex)
            {
                rm.LogEvent(EventLogEntryType.Error, 0, "RecInfoChannelChange failed to initialize recorder_session_state. Err- " + ex.Message, 3);
                return;
            }

            VideoData[] doneVideo ;
            try
            {
                // videos
                List<VideoData> allVideo = (List<VideoData>)dataMap[IssDataCodes.recorder_videos];
                var activeVideo = allVideo.Where(vid => vid.m_bFinished == false).ToArray();
                m_ActiveVideoThumbnails = activeVideo.Select(vid => vid.m_strPath);
                Files2Streams = activeVideo.ToDictionary(vid => vid.m_strPath, vid => vid.m_strEncoderName, StringComparer.OrdinalIgnoreCase);
                Files2Timestamps = activeVideo.ToDictionary(vid => vid.m_strPath, vid => vid.Timestamp, StringComparer.OrdinalIgnoreCase);

                doneVideo = allVideo.Where(vid => vid.m_bFinished ).ToArray();
                m_Videos = doneVideo.Select(vid => vid.m_strPath);
            }
            catch (Exception ex)
            {
                rm.LogEvent(EventLogEntryType.Error, 0, "RecInfoChannelChange failed to initialize recorder_videos. Err- " + ex.Message, 3);
                return;
            }

            try
            {
                // channels
                Dictionary<string, double> channelDurations = ((Dictionary<string, double>) dataMap[IssDataCodes.recorder_active_video_duration]).ToOrdinalIgnoreCaseDictionary();
                ChannelInfos = GetChannelInfos(channelsInfo, channelDurations, doneVideo, true);
            }
            catch (Exception ex)
            {
                rm.LogEvent(EventLogEntryType.Error, 0, "RecInfoChannelChange failed to initialize recorder_active_video_duration. Err- " + ex.Message, 3);
            }

        }
    }
}