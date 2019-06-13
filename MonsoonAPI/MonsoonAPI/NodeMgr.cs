using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using IsmLogCommon;
using IsmRec.Types;
using IsmRecSi;
using IsmStateServer;
using IsmStateServer.Types;
using IsmUtility;
using MonsoonAPI.models;
using Newtonsoft.Json.Linq;

namespace MonsoonAPI
{
    public interface INodeMgr
    {
        void Init(IMonsoonResMgr rm, IActiveMgr activeMgr);

        void ProcessIsmEvent(SubscriberInfoWeb.EventMonsoon evt);

        void SendRecorderReady(bool bReady, eRecordState recState = eRecordState.unknown, bool? bSafeZone = null);
    }

    public class NodeMgr : INodeMgr
    {
        private IMonsoonResMgr _rm;
        private IActiveMgr _activeMgr;

        public void Init(IMonsoonResMgr rm, IActiveMgr activeMgr)
        {
            if (_rm == null)
                _rm = rm;
            if (_activeMgr == null && activeMgr != null)
                _activeMgr = activeMgr;
        }

        public void ProcessIsmEvent(SubscriberInfoWeb.EventMonsoon evt)
        {
            IssEventCodes code = IssEventCodes.unknown;
            try
            {
                code = (IssEventCodes) evt.Code;
                _rm.LogEvent(EventLogEntryType.Information, 0, $"ProcessIsmEvent received {code}", 3);

                switch (code)
                {
                    case IssEventCodes.lib_forwarding:
                        ProcessForwardStatus(evt);
                        break;
                    case IssEventCodes.lib_safe_zone_changed:
                        bool bSafeZone = (bool)evt.SerializedObj[0];
                        SendRecorderReady(true, eRecordState.unknown, bSafeZone); // this is just how our implementation is now... 
                        break;
                    case IssEventCodes.on_data_invalid:
                        ProcessDataInvalid(evt);
                        break;
                    case IssEventCodes.on_data_valid:
                        ProcessDataValid(evt);
                        break;
                    //case IssEventCodes.print_autoprint_done:
                    //    SendFullMonsoonUrl("recorder", "view", "refresh");
                    //    break;
                    case IssEventCodes.print_status_update:
                        ProcessPrintUpdate(evt);
                        break;
                    case IssEventCodes.recorder_start_rec:
                        ProcessRecorderStartRec();
                        break;
                    case IssEventCodes.recorder_stop_rec:
                        ProcessRecorderStopRec(evt);
                        break;
                    case IssEventCodes.recorder_channel_enabled_changed:
                        var chanChangeRecInfo = new RecInfoChannelChange(_rm, evt);
                        SendFullMonsoonUrlObj("recorder", "info", chanChangeRecInfo);
                        break;
                    case IssEventCodes.recorder_start_pm_rec:
                    case IssEventCodes.recorder_stop_pm_rec:
                        var recInfo = new RecInfoM(code);
                        SendFullMonsoonUrlObj("recorder", "info", recInfo);
                        break;
                    case IssEventCodes.recorder_dvi_switch_state_changed:
                        //21131
                        //TODO we should serialize monsoon events the same way as the state server to avoid having to cast to JObjects
                        var switchStateJObject = (JObject)evt.SerializedObj[0];
                        var switchState = switchStateJObject.ToObject<DviSwitchState>();
                        SendFullMonsoonUrl("recorder", "input", switchState.m_nSelectedChannel.ToString());
                        break;
                    case IssEventCodes.recorder_image_captured:
                        ProcessRecorderImageCaptured(evt);
                        break;
                    case IssEventCodes.recorder_image_deleted:
                        string strImgDeleted = (string)evt.SerializedObj[0];
                        SendFullMonsoonUrl("images", "deleted", strImgDeleted);
                        break;
                    case IssEventCodes.recorder_max_clip_time_elapsed:
                        string strMsg = evt.SerializedObj.Any() ?
                            (string)evt.SerializedObj[0] : string.Empty;
                        SendFullMonsoonUrl("recorder", "maxtime", strMsg);
                        break;
                    case IssEventCodes.recorder_new_proc:
                        SendFullMonsoonUrl("recorder", "view", "capture");
                        break;
                    case IssEventCodes.recorder_proc_finished:
                        var objFinishDetail = (JObject)evt.SerializedObj[0];
                        var finishDetail = objFinishDetail.ToObject<FinishDetail>();
                        SendFullMonsoonUrlObj("recorder", "finish_detail", finishDetail);
                        break;
                    case IssEventCodes.recorder_session_state_change:
                        eRecordState recState = (eRecordState)(long)evt.SerializedObj[0];
                        SendRecorderReady(true, recState);
                        break;
                    case IssEventCodes.recorder_usb_drive_state_changed:
                        ProcessUsbStateChange(evt);
                        break;
                    case IssEventCodes.recorder_usb_export_status:
                        ProcessUsbStatus(evt);
                        break;
                    case IssEventCodes.recorder_video_deleted:
                        var video = (string)evt.SerializedObj[0];
                        SendFullMonsoonUrl("videos", "deleted", video);
                        break;
                    //case IssEventCodes.recorder_mobile_video_added:
                    //    var vidDataObj = (JArray)evt.SerializedObj[0];
                    //    var vidData = vidDataObj.ToObject<List<VideoData>>();
                    //    var newMobileVid = vidData.Where(v => string.Equals(v.m_strEncoderName, "M")).OrderByDescending(v => v.Timestamp).FirstOrDefault();
                    //    SendFullMonsoonUrlObj("mobilevideos", "new", newMobileVid);
                    //    break;
                }

            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0,
                    $"ProcessIsmEvent for code {code} failed with err {ex.Message}", 3);
            }
        }

        private void ProcessRecorderStopRec(SubscriberInfoWeb.EventMonsoon evt)
        {
            try
            {
                var channelInfos = ((Dictionary<string, RecChanInfo>) _rm.GetIssData(IssDataCodes.recorder_channels)).ToOrdinalIgnoreCaseDictionary();
                var stopRecInfo = new StopRecInfo(evt, channelInfos);
                SendFullMonsoonUrlObj("recorder", "info", stopRecInfo);
            }
            catch (Exception e)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "ProcessRecorderStopRec err: " + e.Message, 3);
            }
        }

        private void ProcessRecorderStartRec()
        {
            try
            {
                var allVideo = _rm.GetVideoData();
                var startRecInfo = new StartRecInfo(allVideo);
                SendFullMonsoonUrlObj("recorder", "info", startRecInfo);
            }
            catch (Exception e)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "ProcessRrecorderStartRec err: "+e.Message,3);
            }
        }

        private void SendFullMonsoonUrlObj(string strChannel, string strMethod, object msg)
        {
            try
            {
                var strMsg = Newtonsoft.Json.JsonConvert.SerializeObject(msg);
                SendFullMonsoonUrl(strChannel, strMethod, strMsg);
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, $"SendFullMonsoonUrlObj failed for {strChannel}.{strMethod} with err {ex.Message}", 3);
            }
        }

        private void SendFullMonsoonUrl(string strChannel, string strMethod, string strMsg, Dictionary<string, string> parameters = null)
        {
            try
            {
                if (_rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0,
                        "SendFullMonsoonUrl cannot proceed because I am an EasyView", 3);
                    throw new Exception("Functionality unavailable on EasyView");
                }

                // put together URL 
                parameters = parameters ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                parameters["msg"] = strMsg;

                var params2Url = parameters.Select(param => $"&{param.Key}={Uri.EscapeDataString(param.Value)}");

                var urlParams = $"{strChannel}.{strMethod}{String.Concat(params2Url)}";
                if (false == string.IsNullOrEmpty(_rm.RoomId))
                {
                    urlParams = $"{urlParams}&room={_rm.RoomId.ToLower()}";
                }
                var strUrl = $"http://127.0.0.1:8080/?channel={urlParams}";

                var bRes = SendHttp(strUrl, out _, out var strErr);

                if (bRes)
                {
                    _rm.LogEvent(EventLogEntryType.Information, 0,
                        _rm.IsDev
                            ? $"SendFullMonsoonUrl succeeded for {urlParams}"
                            : $"SendFullMonsoonUrl succeeded for {strChannel}.{strMethod}", 4);
                    return;
                }

                var strWarning = _rm.IsDev
                    ? $"SendHttp failed for url {strUrl}. Err {strErr}"
                    : $"SendHttp failed for url {strChannel}.{strMethod}. Err {strErr}";
                _rm.LogEvent(EventLogEntryType.Warning, 0, strWarning, 3);
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0,
                    $"SendFullMonsoonUrl failed for channel {strChannel}, method {strMethod}, msg {strMsg} with err {ex.Message}",
                    2);
            }
        }

        private bool SendHttp(string strUrl, out string strResp, out string strErr)
        {
            strErr = strResp = string.Empty;
            try
            {
                var req = (HttpWebRequest) WebRequest.Create(strUrl);

                using (var resp = req.GetResponse())
                {
                    var respCode = ((HttpWebResponse) (resp)).StatusCode;
                    if (respCode != HttpStatusCode.OK && respCode != HttpStatusCode.NoContent)
                        throw new Exception("status - " + ((HttpWebResponse) (resp)).StatusCode);

                    if (respCode != HttpStatusCode.NoContent)
                    {
                        try
                        {
                            var dataStream = resp.GetResponseStream();
                            if (dataStream == null)
                            {
                                _rm.LogEvent(EventLogEntryType.Error, 0, "GetResponseStream returned null", 3);
                                return false;
                            }
                            using (var streamReader = new StreamReader(dataStream))
                            {
                                strResp = streamReader.ReadToEnd();
                            }
                            return true;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Failed to process response stream with err " + ex.Message);
                        }
                    }
                }

                _rm.LogEventWeb(EventLogEntryType.Information, 0, "SendHttp: " + strUrl, 7);
                return true;
            }
            catch (Exception ex)
            {
                strErr = ex.Message;
                return false;
            }
        }

        public void SendRecorderReady(bool bReady, eRecordState recState = eRecordState.unknown, bool? bSafeZone = null)
        {
            try
            {

                if (!bReady)
                {
                    SendFullMonsoonUrl("recorder", "ready", "false");
                }

                // ready has some more stuff with it...
                if (recState == eRecordState.unknown && bSafeZone == null)
                {
                    IssDataCodes[] codes = {IssDataCodes.lib_safe_zone, IssDataCodes.recorder_session_state};
                    if (_rm.GetIssDataMap(codes, out var ssData) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "Failed to get library and recorder data", 3);
                        return;
                    }
                    bSafeZone = (bool) ssData[IssDataCodes.lib_safe_zone];
                    recState = (eRecordState) ssData[IssDataCodes.recorder_session_state];
                }
                else if (recState == eRecordState.unknown)
                {
                    recState = (eRecordState) _rm.GetIssData(IssDataCodes.recorder_session_state);
                }
                else if (bSafeZone == null)
                {
                    bSafeZone = (bool) _rm.GetIssData(IssDataCodes.lib_safe_zone);
                }

                Dictionary<string, string> extraParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["active"] = RsUtils.IsActive(recState).ToString(),
                    ["safe_zone"] = bSafeZone.ToString()
                };
                SendFullMonsoonUrl("recorder", "ready", "true", extraParams);
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error,0, $"SendRecorderReady for {bReady} failed with err {ex.Message}", 3);
            }
        }

        private  void  ProcessUsbStateChange(SubscriberInfoWeb.EventMonsoon evt)
        {
            try
            {
                var objUsbState = (JObject)evt.SerializedObj[0];
                var usbStatus = objUsbState.ToObject<KeyValuePair<string, EUsbState>>();
                SendFullMonsoonUrl("recorder", "usb", usbStatus.Value.ToString());
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "ProcessUsbStateChange failed with err " + ex.Message, 3);
            }   
        }

        private void ProcessUsbStatus(SubscriberInfoWeb.EventMonsoon evt)
        {
            try
            {
                _rm.LogEvent(EventLogEntryType.Information, 0, "ProcessUsbStatus entered", 3);
                var obj = (JObject)evt.SerializedObj[0];
                var usbMsg = obj.ToObject<UsbExportStatusMessage>();
                if (usbMsg?.Id == null)
                    throw new Exception("Bad object passed into the event");

                var sendParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (usbMsg.ExportStatus == EJobStatus.ErrorNoSpace)
                {
                    sendParams["mb_needed"] = usbMsg.BytesNeeded.HasValue ? DirManager.BytesToMB(usbMsg.BytesNeeded.Value).ToString("F") : "Unknown";
                    sendParams["mb_available"] = usbMsg.BytesAvailable.HasValue ? DirManager.BytesToMB(usbMsg.BytesAvailable.Value).ToString("F") : "Unknown";
                }

                // translate status into format that MonsoonWeb understands
                string msg;
                switch (usbMsg.ExportStatus)
                {
                    case EJobStatus.Success:
                        msg = "101";
                        break;
                    case EJobStatus.Sending:
                        msg = $"{usbMsg.ProgressPercent}";
                        break;
                    case EJobStatus.Cancel:
                        msg = "-1";
                        break;
                    case EJobStatus.ErrorNoSpace:
                        msg = "-2";
                        break;
                    case EJobStatus.ErrorOther:
                        msg = "-3";
                        break;
                    case EJobStatus.ErrorDestinationUnavailable:
                        msg = "-4";
                        break;
                    default:
                        _rm.LogEvent(EventLogEntryType.Error, 0, $"ProcessUsbStatus cannot handle status {usbMsg.ExportStatus}. No notification to MonsoonWeb will be sent", 3);
                        return;
                }


                var strChannel = usbMsg.Trigger != UsbExportTrigger.Manual ? "usb_export" : "usb_export_mr";
                SendFullMonsoonUrl(strChannel, "status", msg, sendParams);
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "ProcessUsbStatus failed with err " + ex.Message, 3);
            }
        }

        private void ProcessRecorderImageCaptured(SubscriberInfoWeb.EventMonsoon evt)
        {
            try
            {
                var obj = (JObject) evt.SerializedObj[0];
                var imgAded = obj.ToObject<ImageData>();
                var nIndex = (long) evt.SerializedObj[1];

                var extraParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["stream"] = imgAded.m_strEncoderName,
                    ["index"] = nIndex.ToString(),
                    ["timestamp"] = imgAded.Timestamp.ToString(DateTimeFormatInfo.InvariantInfo)
                };

                SendFullMonsoonUrl("images", "new", imgAded.m_strPath, extraParams);
            }
            catch (Exception e)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "ProcessRecorderImageCaptured err: " + e.Message, 3);
            }
        }

        private void ProcessForwardStatus(SubscriberInfoWeb.EventMonsoon evt)
        {
            try
            {
                bool bForw = (bool)evt.SerializedObj[0];
                SendFullMonsoonUrl("forwarding", "status", bForw.ToString());
            }
            catch (Exception e)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "ProcessForwardStatus err: " + e.Message, 3);
            }
        }

        private void ProcessDataInvalid(SubscriberInfoWeb.EventMonsoon evt)
        {
            try
            {
                if (_rm.MonsoonCfg == EMonsoonConfig.EasyView)
                    return; // don't worry about EV here

                IssDataCodes dataCode = (IssDataCodes)Convert.ToInt32(evt.SerializedObj[0]);
                if (dataCode == IssDataCodes.library_data_usable || dataCode == IssDataCodes.recorder_data_usable)
                    SendRecorderReady(false);

            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0,
                    $"ProcessDataInvalid failed for {evt.Code} with err {ex.Message}", 3);
            }
        }

        private void ProcessPrintUpdate(SubscriberInfoWeb.EventMonsoon evt)
        {
            // verify input
            EJobStatus jobStatus;
            ProcedureId incomingProcId;
            try
            {
                if (evt.SerializedObj.Count < 2) throw  new Exception("Print status object cannot have fewer than 2 elements");

                var objIncomingProcId = (JObject)evt.SerializedObj[0];
                incomingProcId = objIncomingProcId.ToObject<ProcedureId>();

                jobStatus = (EJobStatus)(long)evt.SerializedObj[1];
            }
            catch (Exception ex)
            {
                IsmLog.LogEvent(EventLogEntryType.Error, 0, $"ProcessPrintUpdate failed to parse input: {ex.Message}", 1);
                return;
            }

            try
            {
                // ignore 'sending' 
                if (jobStatus == EJobStatus.Sending)
                {
                    _rm.LogEvent(EventLogEntryType.Information, 0, "Ignoring sending status", 3);
                    return;
                }

                // get info on active now
                if (!_activeMgr.GetActiveProcCodes(false, out var dataValues))
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0, "failed to get active proc values", 3);
                    return;
                }

                // only go on for active
                var recordState = eRecordState.unknown;
                if (dataValues.TryGetValue(IssDataCodes.recorder_session_state, out var obj) && obj != null)
                    recordState = (eRecordState) obj;
                if (!RsUtils.IsActive(recordState))
                {
                    _rm.LogEvent(EventLogEntryType.Information, 0,
                        $"ProcessPrintUpdate ignored because record state is {recordState}", 3);
                    return;
                }

                // compare proc id
                ProcedureData activeProcData = null;
                if (dataValues.TryGetValue(IssDataCodes.recorder_proc_info, out obj) && obj != null)
                    activeProcData = (ProcedureData) obj;
                if (activeProcData == null)
                {
                    _rm.LogEvent(EventLogEntryType.Warning, 0, "ProcessPrintUpdate skipping because unable to get active proc data", 3);
                    return;
                }

                if (!activeProcData.m_Id.m_strLibId.Equals(incomingProcId.m_strLibId, StringComparison.OrdinalIgnoreCase))
                {
                    _rm.LogEvent(EventLogEntryType.Warning, 0, "ProcessPrintUpdate skipping because lib id doesn't match active", 3);
                    return;
                }

                // yes, tell UI to refresh
                SendFullMonsoonUrl("recorder", "view", "refresh");
            }
            catch (Exception e)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "ProcessPrintUpdate err: " + e.Message, 3);
            }

        }

        /// <summary>
        /// We have received event that recorder or library (others can be ignored) is online. Check the other one & send notice accordingly
        /// </summary>
        private void ProcessDataValid(SubscriberInfoWeb.EventMonsoon evt)
        {
            try
            {
                if (_rm.MonsoonCfg == EMonsoonConfig.EasyView)
                    return; // don't worry about EV here

                var dataCodeReceived = (IssDataCodes) Convert.ToInt32(evt.SerializedObj[0]);
                IssDataCodes codeToGet;
                switch (dataCodeReceived)
                {
                    case IssDataCodes.recorder_data_usable:
                        codeToGet = IssDataCodes.library_data_usable;
                        break;
                    case IssDataCodes.library_data_usable:
                        codeToGet = IssDataCodes.recorder_data_usable;
                        break;
                    default: return; // ignore
                }
                var otherDataUsable = _rm.GetIssData(codeToGet);
                if (otherDataUsable == null)
                    throw new Exception($"Failed to retrieve {codeToGet} from SS");
                SendRecorderReady((bool) otherDataUsable);
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0,
                    $"ProcessDataValid failed for {evt.Code} with err {ex.Message}", 3);
            }
        }
    }
}
