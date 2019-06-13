using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IsmLogCommon;
using IsmRec.Types;
using IsmStateServer;

namespace MonsoonAPI.models
{
    public class StopRecInfo : RecInfoM
    {
        public IEnumerable<RecChanInfoMonsoon> ChannelInfos;
        // ReSharper disable once InconsistentNaming
        public IEnumerable<string> m_Videos;

        public StopRecInfo(SubscriberInfoWeb.EventMonsoon evt, Dictionary<string, RecChanInfo> channelInfos)
        {
            try
            {
                m_Trigger = ERecEvent.StopRecord;
                InitRecState(eRecordState.proc_not_recording);

                // some stuff comes from the event paramas
                Newtonsoft.Json.Linq.JArray array = (Newtonsoft.Json.Linq.JArray)evt.SerializedObj[0];
                VideoData[] allVideo = array.ToObject<VideoData[]>();

                var obj = (Newtonsoft.Json.Linq.JObject)evt.SerializedObj[1];
                Dictionary<string, double> channelDurations = obj.ToObject<Dictionary<string, double>>();

                // initialize videos
                IEnumerable<VideoData> doneVideo = allVideo.Where(vid => vid.m_bFinished);
                var videoDatas = doneVideo as VideoData[] ?? doneVideo.ToArray();
                m_Videos = videoDatas.Select(vid => vid.m_strPath);

                // Initialize Channel Info
                ChannelInfos = GetChannelInfos(channelInfos, channelDurations, videoDatas, false);
            }
            catch (Exception ex)
            {
                IsmLog.LogEvent(EventLogEntryType.Error, 0, "StopRecInfo failed to initialize: " + ex.Message, 1);
            }
        }
    }
}
