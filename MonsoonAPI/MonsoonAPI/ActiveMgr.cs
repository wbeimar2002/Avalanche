using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using  MonsoonAPI.models;
using  System.Net;
using System.Xml.Linq;
using IsmRec.Types;
using IsmRecSi;
using  IsmStateServer.Types;
using IsmStateServer;
using IsmUtility;
using ISM.LibrarySi;
using ISM.Middleware2Si;
using PatInfoEngine.Types;
using IsmLogCommon;
using Microsoft.AspNetCore.Http;

namespace MonsoonAPI
{
    public interface IActiveMgr
    {
        HttpStatusCode GetProcInfo(out DmagM procInfo, CultureInfo searchCulture, bool bFullRecord = true);
        HttpStatusCode RegisterProc(ClinInfoDataExM clinInfo, CultureInfo searchCulture);
        HttpStatusCode FinishProc(Dictionary<string, bool> finishOptions);
        HttpStatusCode SaveRecorderScreenshot(ScreenshotInfo screenshotInfo, byte[] img);
        HttpStatusCode UpdateActiveProcedure(ClinInfoDataExM clinInfo, PersonNameData physician);
        HttpStatusCode SetLabel(string strFile, string strLabel);
        bool GetActiveProcCodes(bool bFullRecord, out Dictionary<IssDataCodes, object> dataValues);
        //HttpStatusCode UpdateAutolabelOverrides(List<string> procTypeLabels, List<string> commonLabels);
    }

    public class ActiveMgr :IActiveMgr
    {
        private readonly IMonsoonResMgr _rm;
        private readonly IMwMgr _mwMgr;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActiveMgr(IMonsoonResMgr rm, IMwMgr mwMgr, IHttpContextAccessor accessor)
        {
            _rm = rm;
            _mwMgr = mwMgr;
            _httpContextAccessor = accessor;
        }

        public HttpStatusCode SetLabel(string strFile, string strLabel)
        {
            try
            {
                _rm.LogEvent(EventLogEntryType.Information, 0, $"ActiveMgr.SetLabel entered for {strFile}, {strLabel}",
                    4);

                Dictionary<string, string> labels = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase) {[strFile] = strLabel};

                using (var ismRecProxy = _rm.GetIsmRecProxy())
                {
                    if (ismRecProxy == null)
                        return HttpStatusCode.ServiceUnavailable;

                    if (ismRecProxy.Proxy.Record_SetAnnotations(labels, out var strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "Record_SetAnnotations err: " + strErr, 3);
                        return HttpStatusCode.InternalServerError;
                    }
                }
                _rm.LogEvent(EventLogEntryType.Information, 0, "ActiveMgr.SetLabel succeeded", 4);
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error,0, "LabelsController.SetRecorderLabel err: " + ex.Message,3);
                return HttpStatusCode.InternalServerError;
            }
        }

        public bool GetActiveProcCodes(bool bFullRecord, out Dictionary<IssDataCodes, object> dataValues)
        {
            try
            {
                var codesToGet = new[]
                {
                    IssDataCodes.recorder_session_state,
                    IssDataCodes.recorder_proc_info
                }.ToList();

                if (bFullRecord)
                {
                    codesToGet.AddRange(new[]
                    {
                        IssDataCodes.context_staff_physician_performing,
                        IssDataCodes.recorder_active_video_duration,
                        IssDataCodes.recorder_channels,
                        IssDataCodes.recorder_images,
                        IssDataCodes.recorder_pms,
                        IssDataCodes.recorder_proc_clin_info,
                        IssDataCodes.recorder_videos
                    });
                }

                if (_rm.GetIssDataMap(codesToGet.ToArray(), out dataValues) != ESuccess.Ok || dataValues == null) // for this one, we really do need to retrieve everything
                    return false;
                return true;
            }
            catch (Exception e)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "GetActiveProcCodes err: " + e.Message, 3);
                dataValues = null;
                return false;
            }
        }

        public HttpStatusCode GetProcInfo(out DmagM procInfo, CultureInfo searchCulture, bool bFullRecord = true)
        {
            procInfo = null;
            try
            {
                if (_rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    _rm.LogEvent(EventLogEntryType.Information, 0, "GetProcInfo returns null because we are on EV", 4);
                    return HttpStatusCode.OK; // treat as kosher for now
                }

                if (!GetActiveProcCodes(true, out var dataValues))
                    return HttpStatusCode.ServiceUnavailable;
                _rm.LogEvent(EventLogEntryType.Information, 0, "Data retrieved from SS", 4);

                // recorder state
                eRecordState recordState = eRecordState.unknown;
                if (dataValues.TryGetValue(IssDataCodes.recorder_session_state, out var obj) && obj != null)
                    recordState = (eRecordState)obj;

                // procedure id
                ProcedureData procData = null;
                if (dataValues.TryGetValue(IssDataCodes.recorder_proc_info, out obj) && obj != null)
                    procData = (ProcedureData)obj;

                // get to mrn
                ClinInfoData clinInfo = null;
                if (dataValues.TryGetValue(IssDataCodes.recorder_proc_clin_info, out  obj) && obj != null)
                    clinInfo = (ClinInfoData)obj;

    

                // performing
                PersonNameData physician = null;
                if (dataValues.TryGetValue(IssDataCodes.context_staff_physician_performing, out obj) && obj != null)
                    physician = (PersonNameData)obj;

              

                // vido durations
                Dictionary<string, double> activeChannels = null;
                if (dataValues.TryGetValue(IssDataCodes.recorder_active_video_duration, out obj) && obj != null)
                    activeChannels = ((Dictionary<string, double>) obj).ToOrdinalIgnoreCaseDictionary();

                //images
                List<ImageData> images = null;
                if (dataValues.TryGetValue(IssDataCodes.recorder_images, out obj) && obj != null)
                    images = (List<ImageData>)obj;

                // channels
                Dictionary<string, RecChanInfo> channelInfos = new Dictionary<string, RecChanInfo>(StringComparer.OrdinalIgnoreCase);
                if (dataValues.TryGetValue(IssDataCodes.recorder_channels, out obj) && obj != null)
                    channelInfos = ((Dictionary<string, RecChanInfo>) obj).ToOrdinalIgnoreCaseDictionary();

                // video
                List<VideoData> allVideo = null;
                if (dataValues.TryGetValue(IssDataCodes.recorder_videos, out obj) && obj != null)
                    allVideo = (List<VideoData>)obj;

                // procMovies
                List<ProcMovieData> pmData = null;
                if (dataValues.TryGetValue(IssDataCodes.recorder_pms, out obj) && obj != null)
                    pmData = (List<ProcMovieData>)obj;

                // create the channels array to go into output
                var channels = GetChannelInfos(channelInfos, images, allVideo, activeChannels);

                // compare list  - et all procedures where MRN matches or is NORMAL
                var compareList = GetCompareList(recordState, clinInfo, procData, searchCulture);

                // not currently supported
                //GetAutolabelOverrides(out List<string> procTypeLabels, out List<string> commonLabels);

                procInfo = new DmagM();
                procInfo.InitActive(_rm, recordState, clinInfo, procData, physician);//, procTypeLabels, commonLabels);
                procInfo.SetActiveContent(_rm, channels, allVideo, images, pmData);
                procInfo.compare_list = DmagM.SearchResultsToWebFormat(compareList);

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "ActiveMgr.GetProcInfo err: " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode RegisterProc(ClinInfoDataExM clinInfo, CultureInfo searchCulture)
        {
            try
            {
                if (_rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    _rm.LogEvent(EventLogEntryType.Warning, 0, "ProcActive not supported on EasyView. RegisterProc fails!", 3);
                    return HttpStatusCode.BadRequest;
                }

                if (clinInfo == null)
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0, "RegisterProc got null obj passed in",3);
                    return HttpStatusCode.BadRequest;
                }

                var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);

                // if no patient data -> do quick reg
                var quickReg = false;
                if (clinInfo.m_ClinInfo == null)
                {
                    string strId = $"{DateTime.UtcNow:yyMMdd_hhmmss}_{ _rm.RoomId}";
                    clinInfo.m_ClinInfo = new ClinInfoDataExM.ClinInfoDataM
                    {
                        m_PatName = new PersonNameDataM { m_strLastName = strId, m_strLogin = strId }
                    };
                    quickReg = true;
                }
                // get original XML
                if (!GetOrigXml(clinInfo.m_ClinInfo, false == clinInfo.skipAccessionSearch, false == clinInfo.skipExternalIdSearch, searchCulture, accessInfo, out var elOrigXml))
                    return HttpStatusCode.InternalServerError;
                // deal with operator
                if (!AppendOperatorToOrigXml(ref elOrigXml, clinInfo.m_strUserId))
                    return HttpStatusCode.InternalServerError;

                // publish performing physician
                if (!PublishPhysician(clinInfo.m_strPhysician, elOrigXml))
                    return HttpStatusCode.InternalServerError;

                // add proc type to MW (if any)
                if (!string.IsNullOrEmpty(clinInfo.m_ClinInfo.m_strProcType))
                    _mwMgr.AddProcType(clinInfo.m_ClinInfo.m_strProcType, clinInfo.m_strPhysician);

                ProcedureId procId = null;

                // finally - call IsmRec!
                using (var ismRecProxy = _rm.GetIsmRecProxy())
                {
                    if (ismRecProxy == null)
                        return HttpStatusCode.ServiceUnavailable;

                    if (ismRecProxy.Proxy.Record_CreateRecordSession(clinInfo.m_ClinInfo.ToClinInfoData(), elOrigXml?.ToString(), accessInfo, out procId, out var strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "Record_CreateRecordSession err: " + strErr, 3);
                        return HttpStatusCode.InternalServerError;
                    }

                    if (clinInfo.startPM)
                    {
                        if (ismRecProxy.Proxy.Record_StartPM(out strErr) != 0)
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0, "Record_StartPM err: " + strErr, 3);
                            return HttpStatusCode.InternalServerError;
                        }
                    }

                }

                _rm.LogEvent(EventLogEntryType.Information, 0, "Procedure registered successfully", 3);

                //TODO do we want to log this here? this event was created, but not used
                //IsmRec will end up logging this anyway because it creates the procedure
                
                /*
                var personNameData = clinInfo?.m_ClinInfo?.m_PatName?.ToPersonNameData();
                var evt = new IsmAccessItem(AccessEventType.ProcedureCreated,
                                                clinInfo?.m_strUserId,
                                                clinInfo?.OriginatorIp,
                                                new Tuple<string, string>(personNameData?.m_strLogin, personNameData?.m_strLastName),
                                                procId?.m_strLibId,
                                                procId?.m_strLibName,
                                                $"Patient has been {(quickReg ? "quick" : "manually")} registered in nCare");
                */

                return HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "ActiveMgr.RegisterProc failed with err " + e.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode UpdateActiveProcedure(ClinInfoDataExM clinInfo, PersonNameData physician)
        {
            try
            {
                var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);
                // if physician is specified - publish it
                if (physician != null)
                    _rm.PublishPhysician(physician);

                using (var ismRecProxy = _rm.GetIsmRecProxy())
                {
                    if (ismRecProxy == null)
                        return HttpStatusCode.ServiceUnavailable;

                    // update patient part
                    if (clinInfo.m_ClinInfo != null)
                    {
                        if (ismRecProxy.Proxy.Record_UpdateClinInfo(clinInfo.m_ClinInfo.ToClinInfoData(), accessInfo, out string strErr) != 0)
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0, "Record_UpdateClinInfo err: " + strErr, 3);
                            return HttpStatusCode.InternalServerError;
                        }

                        ProcedureData data = new ProcedureData { m_strClinicalNote = clinInfo.m_strClinicalNote };
                        var additionalUsers = clinInfo.m_strSharing.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (ismRecProxy.Proxy.Record_UpdateRecordSession(data, additionalUsers, accessInfo, out strErr) != 0)
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0, "Record_UpdateRecordSession err: " + strErr, 3);
                            return HttpStatusCode.InternalServerError;
                        }
                    }
                }

                _rm.LogEvent(EventLogEntryType.Information, 0, "Procedure updated successfully", 3);
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "UpdateActiveProcedure ex: " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }


        public HttpStatusCode SaveRecorderScreenshot(ScreenshotInfo screenshotInfo, byte[] img)
        {
            try
            {
                var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);
                if (img == null && string.IsNullOrEmpty(screenshotInfo.file_name))
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0,
                        "SaveRecorderScreenshot must have either image or image name specified", 3);
                    return HttpStatusCode.BadRequest;
                }

                // get the active proc info
                var procInfo = (ProcedureData)_rm.GetIssData(IssDataCodes.recorder_proc_info);
                if (procInfo == null)
                    return HttpStatusCode.ServiceUnavailable;

                // transfer file
                if (img != null)
                {
                    var msg = new FileUploadMessage
                    {
                        Path = Path.Combine(procInfo.m_strProcPath, screenshotInfo.file_name),
                        LibId = procInfo.m_Id.m_strLibId,
                        StartOffset = 0,
                        ClientId = -1,
                        DataStream = new MemoryStream(img)
                    };

                    using (var transferProxy = _rm.GetLibFileRepProxy(true))
                    {
                        if (transferProxy == null)
                            return HttpStatusCode.ServiceUnavailable;

                        OperationReturn ret = transferProxy.Proxy.PutFile(msg);
                        if (ret.Result != eLibErr.OK)
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0, "Failed to transfer file", 3);
                            return HttpStatusCode.InternalServerError;
                        }
                    }
                }

                // now - tell recorder we have something to add to dmag
                using (var recProxy = _rm.GetIsmRecProxy())
                {
                    if (recProxy == null)
                        return HttpStatusCode.ServiceUnavailable;

                    //note that this will not log an access event. however, accessInfo is needed because this can trigger an autoPrint
                    if (recProxy.Proxy.Record_AddImage(screenshotInfo.file_name, screenshotInfo.movie, screenshotInfo.offset, accessInfo, out var strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "Record_AddImage failed with err " + strErr, 3);
                        return HttpStatusCode.InternalServerError;
                    }
                }

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "SaveLibraryScreenshot err: " + ex.Message,3);
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode FinishProc(Dictionary<string, bool> finishOptions)
        {
            try
            {
                if (_rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    _rm.LogEvent(EventLogEntryType.Warning, 0, "ProcActive not supported on EasyView. FinishProc fails!", 3);
                    return HttpStatusCode.BadRequest;
                }

                if (!finishOptions.TryGetValue("save", out var bSave) ||
                    !finishOptions.TryGetValue("usb", out var bUsbExport) ||
                    !finishOptions.TryGetValue("phi", out var bPhi))
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0, "right param not passed in", 3);
                    return HttpStatusCode.BadRequest;
                }

                using (var ismRecProxy = _rm.GetIsmRecProxy())
                {
                    if (ismRecProxy == null)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "Failed to finish because rec is offline",3);
                        return HttpStatusCode.ServiceUnavailable;
                    }

                    var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);

                    string strErr;
                    var nRes = bSave
                        ? ismRecProxy.Proxy.Record_Finish(bUsbExport, bPhi, false, accessInfo, out strErr)
                        : ismRecProxy.Proxy.Record_Discard(bUsbExport, bPhi, false, accessInfo, out strErr);

                    if (nRes != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "Failed to finish with err " + strErr, 3);
                        return HttpStatusCode.InternalServerError;
                    }
                }
                return HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "ActiveMgr.FinishProc failed with err " + e.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        // not currently supported
        //public HttpStatusCode UpdateAutolabelOverrides(List<string> procTypeLabels, List<string> commonLabels)
        //{
        //    try
        //    {
        //        SetAutolabelOverrides(procTypeLabels, commonLabels);

        //        return HttpStatusCode.OK;
        //    }
        //    catch (Exception ex)
        //    {
        //        _rm.LogEvent(EventLogEntryType.Error, 0, "ActiveMgr.UpdateAutolabelOverrides failed with err " + ex.Message, 3);
        //        return HttpStatusCode.InternalServerError;
        //    }
        //}

        private bool GetOrigXml(ClinInfoDataExM.ClinInfoDataM data, bool searchAccession, bool searchProcId, CultureInfo searchCulture, AccessInfo accessInfo, out XElement elOrigPat)
        {
            elOrigPat = null;
            try
            {
                if (string.IsNullOrEmpty(data.m_strScheduleId))
                    return true;

                // if schedule id is specified - get orig xml from PIE
                string strPatList;
                using (var pieProxy = _rm.GetPieProxy())
                {
                    if (pieProxy == null)
                    {
                        _rm.LogEvent(EventLogEntryType.Warning, 0, "GetOrigXml failure because PIE is offline", 3);
                        return false;
                    }

                    var attrTagsVals = new Dictionary<EPieFields, string> { [EPieFields.ScheduleId] = data.m_strScheduleId };

                    if (searchProcId)
                    {
                        attrTagsVals[EPieFields.Procedure_Id] = data.m_strExternalProcId;
                    }
                    if (searchAccession)
                    {
                        attrTagsVals[EPieFields.Accession] = data.m_strAccession;
                    }

                    if (pieProxy.Proxy.PatList_Search(attrTagsVals, 0, 2, searchCulture.Name, accessInfo, out strPatList, out var strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "PatList_Search failed with err " + strErr, 3);
                        return false;
                    }
                }
                // parse out result, make sure there's just 1 record returned
                XElement elPatList = XElement.Parse(strPatList);
                IEnumerable<XElement> pats = elPatList.Elements("patient");
                var xElements = pats as XElement[] ?? pats.ToArray();
                if (xElements.Length != 1)
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0, $"PIE search found {xElements.Length} records. Should always find 1!", 3);
                    return false;
                }
                // return
                elOrigPat = xElements.Single();
                return true;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, $"GetOrigXml faild for id {data.m_strScheduleId} with err {ex.Message}", 2);
                return false;
            }
        }

        List<DmagM.ChannelInfo> GetChannelInfos(Dictionary<string, RecChanInfo> channelInfos,
            List<ImageData> images, List<VideoData> allVideo, Dictionary<string, double> activeChannels)
        {
            try
            {
                if(allVideo.FirstOrDefault(a => string.Equals(a.m_strEncoderName, "M", StringComparison.OrdinalIgnoreCase))!= null 
                    || images.FirstOrDefault(i=> string.Equals(i.m_strEncoderName, "M", StringComparison.OrdinalIgnoreCase)) != null)
                {
                    channelInfos.Add("M", new RecChanInfo { m_bEnabled = true, m_nSelectedInput = -1 });
                }

                List<DmagM.ChannelInfo> channels = new List<DmagM.ChannelInfo>();
                foreach (var inputChannel in channelInfos)
                {
                    DmagM.ChannelInfo outputChannel = new DmagM.ChannelInfo
                    {
                        channel = inputChannel.Key,
                        enabled = inputChannel.Value.m_bEnabled,
                        m_nSelectedInput = inputChannel.Value.m_nSelectedInput,
                        images = images.Count(im => im.m_strEncoderName.Equals(inputChannel.Key)),
                        length = allVideo.Where(vidCh => vidCh.m_strEncoderName.Equals(inputChannel.Key))
                            .Select(vidCh => vidCh.m_dSeconds).Sum()
                    };
                    if (activeChannels.ContainsKey(inputChannel.Key))
                        outputChannel.length += activeChannels[inputChannel.Key];

                    channels.Add(outputChannel);
                }
                _rm.LogEvent(EventLogEntryType.Information, 0, "GetChannelInfos success", 4);
                return channels;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "GetChannelInfos err " + ex.Message, 3);
                return null;
            }
        }

        List<Dictionary<ESearchFields, SearchResultFieldBase>> GetCompareList(eRecordState recordState, ClinInfoData clinInfo, ProcedureData procData, CultureInfo searchCulture)
        {
            try
            {
                if (!RsUtils.IsActive(recordState))
                    return null;

                // compare list  - et all procedures where MRN matches or is NORMAL
                using (var lib = _rm.GetLibProxy())
                {
                    if (lib == null)
                        throw new Exception("Lib is not available");

                    var si = new SearchItems();

                    var strMrns = Newtonsoft.Json.JsonConvert.SerializeObject(new[] { clinInfo?.m_PatName.m_strLogin, "NORMAL" });
                    si.Add(new SearchItem(ESearchFields.mrn, ECriteria.EqualsOneOf, strMrns));
                    si.Add(new SearchItem(ESearchFields.libid, ECriteria.NoEquals, procData?.m_Id?.m_strLibId)); //FB15363, MM, 5/11/2015 - exclude "self" from the compare list
                    si.m_Clinical = eClinical.Clinical;
                    si.m_RequestedPageNumber = 1;
                    si.m_nLimitEntries = int.MaxValue;
                    ESearchFields[] retFields = { ESearchFields.libid, ESearchFields.libname, ESearchFields.mrn, ESearchFields.date };

                    var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);

                    if (lib.Proxy.Search(si, retFields, MwUtils.ADMIN_LOGIN, searchCulture.Name, accessInfo, out var results, out var strErr) != 0)
                        _rm.LogEvent(EventLogEntryType.Error, 0, "LibSearch_SearchAll err: " + strErr, 3);

                    return results?.Result;
                }
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "GetCompareList err: " + ex.Message, 3);
                return null;
            }
        }

        // not currently supported
        //private void SetAutolabelOverrides(List<string> procTypeLabels, List<string> commonLabels)
        //{
        //    using (var rec = _rm.GetIsmRecProxy())
        //    {
        //        if (rec == null)
        //        {
        //            throw new Exception("IsmRec is not available");
        //        }

        //        if (0 != rec.Proxy.Record_SetAutolabelOverrides(procTypeLabels, commonLabels, out string err))
        //        {
        //            throw new Exception("Record_SetAutolabelOverrides failed with error: " + err);
        //        }
        //    }
        //}

        //private void GetAutolabelOverrides(out List<string> procTypeLabels, out List<string> commonLabels)
        //{
        //    commonLabels = null;
        //    procTypeLabels = null;
        //    try
        //    {
        //        using (var rec = _rm.GetIsmRecProxy())
        //        {
        //            if (rec == null)
        //            {
        //                throw new Exception("IsmRec is not available");
        //            }

        //            if (0 != rec.Proxy.Record_GetAutolabelOverrides(out procTypeLabels, out commonLabels, out string err))
        //            {
        //                _rm.LogEvent(EventLogEntryType.Error, 0, "Record_GetAutolabelOverrides error: " + err, 1);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _rm.LogEvent(EventLogEntryType.Error, 0, "GetAutolabelOverrides error: " + ex.Message, 1);
        //    }
        //}

        private bool AppendOperatorToOrigXml(ref XElement elOrigPat, string strOperator)
        {
            try
            {
                if (string.IsNullOrEmpty(strOperator))
                    return true;

                // if operator is specified, make it part of "orig" now
                var elOperator = new XElement("ISM_OPERATOR", strOperator);
                if (elOrigPat == null)
                {
                    elOrigPat = new XElement("patient", new XElement("extendedinfo", elOperator));
                }
                else
                {
                    var elExtended = elOrigPat.Element("extendedinfo");
                    if (elExtended == null)
                    {
                        elExtended = new XElement("extendedinfo", elOperator);
                        elOrigPat.Add(elExtended);
                    }
                    else
                        elExtended.Add(elOperator);
                }

                return true;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, $"AppendOperatorToOrigXml faild for operator {strOperator} with err {ex.Message}", 2);
                return false;
            }
        }

        private bool PublishPhysician(string strLogin, XContainer elOrigXml)
        {
            try
            {
                if (string.IsNullOrEmpty(strLogin))
                    return true; // nothing to set

                // set physician if any
                PersonNameData user;
                if (!strLogin.Equals(MwUtils.VISIT_LOGIN))
                {
                    using (var mwProxy = _rm.GetMwProxy())
                    {
                        var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);

                        if (mwProxy == null)
                        {
                            _rm.LogEvent(EventLogEntryType.Warning, 0,
                                "PublishPhysician failure because IMiddleware2Si is offline", 3);
                            return false;
                        }
                        if (mwProxy.Proxy.Users_GetXml(strLogin, MwUtils.SystemCredentials, accessInfo, out var strXmlUser,
                                out var strErr) != 0)
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0,
                                $"Users_GetXml for {strLogin} failed with err {strErr}", 3);
                            return false;
                        }

                        XElement elUser = XElement.Parse(strXmlUser);
                        user = new PersonNameData
                        {
                            m_strFirstName = XmlUtils.El2String(elUser, "fname"),
                            m_strLastName = XmlUtils.El2String(elUser, "lname"),
                            m_strLogin = XmlUtils.El2String(elUser, "login")
                        };
                    }
                }
                else
                {
                    var elPhys = elOrigXml?.Element("extendedinfo")?.Element("physician");
                    if (elPhys == null)
                        return false;
                    user = new PersonNameData
                    {
                        m_strFirstName = elPhys.Attribute("firstname")?.Value,
                        m_strLastName = elPhys.Attribute("lastname")?.Value,
                        m_strLogin = elPhys.Value
                    };
                }

                return _rm.PublishPhysician(user);
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, $"PublishPhysician for {strLogin} failed with err {ex.Message}", 3);
                return false;
            }
        }
    }
}
