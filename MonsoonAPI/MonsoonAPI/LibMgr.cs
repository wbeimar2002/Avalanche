using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Configuration;
using ISM.LibrarySi;
using ISM.Middleware2Si;
using IsmStateServer.Types;
using MonsoonAPI.models;
using IsmDmag;
using IsmLogCommon;
using IsmStateServer;
using ISM.Library.Types;
using IsmUtility;
using MonsoonAPI.Models;
using Microsoft.AspNetCore.Http;

namespace MonsoonAPI
{
    public interface ILibMgr
    {
        HttpStatusCode SearchLibrary(SearchCriteria criteria, out object res, CultureInfo searchCulture);
        HttpStatusCode GetLibRecord(ProcedureId procId, string login, bool compare, out DmagM record, CultureInfo searchCulture);
        HttpStatusCode SaveLibraryScreenshot(ScreenshotInfo screenshotInfo, byte[] img);
        HttpStatusCode UpdateLibraryPatient(ClinInfoDataExM clinInfo, PersonNameData physician);
        HttpStatusCode ProcessRedirect(string strArgs, out object obj, CultureInfo searchCulture);
        HttpStatusCode DoGetTasks(eStatus? status, ePayloadType? taskType, out IEnumerable<LibWorkItemBase> tasks);
        HttpStatusCode SetLibraryLabel(LabelInfo labelInfo);
        HttpStatusCode GetProcPath(ProcedureId procId, out string path);
        HttpStatusCode DoSearchLib(ref SearchItems search, string login, ESearchFields[] searchFields, CultureInfo searchCulture, out SearchResults searchResults);
    }

    public class LibMgr : ILibMgr
    {
        static readonly ESearchFields[] KLibSearchFields = { ESearchFields.mrn, ESearchFields.lastname, ESearchFields.firstname, ESearchFields.date,
            ESearchFields.image, ESearchFields.video_thumb, ESearchFields.libid, ESearchFields.libname, ESearchFields.volumename, ESearchFields.video_count, ESearchFields.video_length,
            ESearchFields.image_count, ESearchFields.physician_display, ESearchFields.clinical_note, ESearchFields.proc_type, ESearchFields.title, ESearchFields.desc,
            ESearchFields.shared_by_searcher, ESearchFields.is_clinical, ESearchFields.auto_edit_status, ESearchFields.proc_movie_exists};


        readonly IMonsoonResMgr _rm;
        readonly IMwMgr _mwMgr;
        readonly IHttpContextAccessor _httpContextAccessor;

        public LibMgr(IMonsoonResMgr rm, IMwMgr mwMgr, IHttpContextAccessor accessor) {
            _rm = rm;
            _mwMgr = mwMgr;
            _httpContextAccessor = accessor;
        }

        public HttpStatusCode SearchLibrary(SearchCriteria criteria, out object res, CultureInfo searchCulture)
        {
            res = null;

            try
            {
                return DoSearch(criteria, out res, searchCulture);
            }
            catch
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, $"Error when searching library for criteria: {criteria}.", 1);
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode GetProcPath(ProcedureId procId, out string path)
        {
            path = null;
            try
            {
                using (var libProxy = _rm.GetLibProxy())
                {
                    if (libProxy == null)
                        return HttpStatusCode.ServiceUnavailable;

                    if (libProxy.Proxy.GetPrimaryProcPath(procId,  out path, out string strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, $"GetPrimaryProcPath failed for {procId} with err {strErr}", 3);
                        return HttpStatusCode.InternalServerError;
                    }
                }

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, $"GetProcPath failed for {procId} with err {ex.Message}", 3);
                return HttpStatusCode.InternalServerError;
            }
        }
        
        public HttpStatusCode DoSearchLib(ref SearchItems search, string login, ESearchFields[] searchFields, CultureInfo searchCulture, out SearchResults results)
        {
            results = null;
            try
            {
                // go search!
                using (var libProxy = _rm.GetLibProxy())
                {
                    if (libProxy == null)
                    {
                        _rm.LogEvent(EventLogEntryType.Warning, 0, "Library is offline", 3);
                        return HttpStatusCode.ServiceUnavailable;
                    }

                    var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);

                    if (libProxy.Proxy.Search(search, searchFields, login, searchCulture.Name, accessInfo, out results, out var strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "DoSearch failed because LibSearch_SearchAll returned err " + strErr, 3);
                        return HttpStatusCode.InternalServerError;
                    }
                }
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "DoSearchLib err: " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }
        /// <summary>
        /// Process redirect request; return actual code (Timeout, Multiples, OK, etc.); calling method will translate response into properly mapped one
        /// </summary>
        public HttpStatusCode ProcessRedirect(string strArgs, out object ret, CultureInfo searchCulture)
        {
            ret = null;
            try
            {
                strArgs = strArgs.Replace(" ", "+");
                _rm.LogEvent(EventLogEntryType.Information, 0,
                    "ProcessRedirect entered; args after replace: " + strArgs, 4);
                // get encode key & decode the passed-in parameters
                string strEncodeKey = _rm.Cfg.GetValue<string>("redirect_encyrpt_key");
                bool bEncodeMd5 = _rm.Cfg.GetValue<bool>("redirect_encrypt_md5");
                Dictionary<UCDRC4.ELoginFields, string> lstArgs = null;
                if (!UCDRC4.GetURLArgs(strArgs, ref lstArgs, new KeyValuePair<string, bool>(strEncodeKey, bEncodeMd5), out var err))
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0, "GetURLArgs failed with err " + err, 3);
                    return HttpStatusCode.BadRequest;
                }
                if (lstArgs.Count != Enum.GetValues(typeof(UCDRC4.ELoginFields)).Length)
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0, "Some fields are missing after parsing URL arguments",3);
                    return HttpStatusCode.BadRequest;
                }
                _rm.LogEvent(EventLogEntryType.Information, 0, "ProcessRedirect entered parsed credentials", 4);

                // make sure time hasn't expired
                int nTimeoutSecs = _rm.Cfg.GetValue<int>("redirect_timeout_secs");

                string timeString;
                DateTime now;
                if (lstArgs.ContainsKey(UCDRC4.ELoginFields.utc_time) && !string.IsNullOrWhiteSpace(lstArgs[UCDRC4.ELoginFields.utc_time]))
                {
                    //if here, the provided time is a utc time so we should compare it to utc now
                    timeString = lstArgs[UCDRC4.ELoginFields.utc_time];
                    now = DateTime.UtcNow;
                }
                else
                {
                    //if here, the provided time is a local time so we should compare it to now
                    timeString = lstArgs[UCDRC4.ELoginFields.date_time];
                    now = DateTime.Now;
                }

                //parse the datetime
                var dt = DateTime.ParseExact(timeString, timeString.ToUpperInvariant().Contains("T") ? "yyyyMMdd'T'HHmmss" : "yyyyMMddHHmmss", null);

                if (now.Subtract(dt).TotalSeconds > nTimeoutSecs)
                    return HttpStatusCode.RequestTimeout;

                _rm.LogEvent(EventLogEntryType.Information, 0, "ProcessRedirect verified time", 4);

                // authenticate
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        _rm.LogEvent(EventLogEntryType.Warning, 0, "ProcessRedirect failure because mw is unavailable",
                            3);
                        return HttpStatusCode.ServiceUnavailable;
                    }

                    AuthInfo authInfo = new AuthInfo
                    {
                        Username = lstArgs[UCDRC4.ELoginFields.user_id],
                        Password = lstArgs[UCDRC4.ELoginFields.pwd]
                    };

                    HttpStatusCode code = _mwMgr.Authenticate(authInfo);
                    if (code != HttpStatusCode.OK)
                        return code;

                    _rm.LogEvent(EventLogEntryType.Information, 0,
                        "ProcessRedirect validated user " + authInfo.Username, 4);

                    // start search
                    var accession = lstArgs[UCDRC4.ELoginFields.order_id];
                    SearchItems si = new SearchItems
                    {
                        new SearchItem(ESearchFields.accession, ECriteria.Equals, accession)
                    };

                    // do we need to restrict the search to a particular library?
                    var lib = _rm.Cfg.GetValue<string>("redirect_lib_restrict");
                    if (!string.IsNullOrEmpty(lib))
                        si.m_strLibName = lib;
                    else
                        si.m_Clinical = eClinical.Clinical;

                    var userid = lstArgs[UCDRC4.ELoginFields.auth_user_id] ?? MwUtils.VISIT_LOGIN;
                    _rm.LogEvent(EventLogEntryType.Information, 0, $"Process Redirect will use user " + userid, 4);

                    // find out if this came from LSS. If it did - do not search for a procedure, just return logged-in user
                    var source = lstArgs[UCDRC4.ELoginFields.source];
                    if (!string.IsNullOrEmpty(source) && source.Equals("lss", StringComparison.OrdinalIgnoreCase))
                    {
                        _rm.LogEvent(EventLogEntryType.Information, 0, "Request coming from LSS, not searching for specific record",3);

                        ret = new
                        {
                            res = HttpStatusCode.Accepted,
                            userid
                        };
                        return HttpStatusCode.OK;
                    }

                    // non-LSS redirect, search for procedure
                    code = DoSearchLib(si, userid, new[] {ESearchFields.libid, ESearchFields.libname}, searchCulture, out var result);
                    if (code != HttpStatusCode.OK)
                        return code;

                    // not fond
                    if (result?.Result != null && result.Result.Count == 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Information, 0, "ProcessRedirect did not find record with accession " + accession, 4);
                        return HttpStatusCode.OK;
                    }

                    // multiples
                    if (result.Result.Count > 1)
                        _rm.LogEvent(EventLogEntryType.Warning, 0, $"ProcessRedirect found {result.Result.Count} records. The first one only will be returned", 1);

                    // return proc
                    var rec = result.Result.First();
                    ret = new
                    {
                        res = HttpStatusCode.Found,
                        userid ,
                        LibId = rec[ESearchFields.libid],
                        LibName = rec[ESearchFields.libname]
                    };

                    _rm.LogEvent(EventLogEntryType.Information, 0, "ProcessRedirect returning dmag", 4);
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "ProcessRedirect err: " + ex.Message, 1);
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode DoGetTasks(eStatus? status, ePayloadType? taskType, out IEnumerable<LibWorkItemBase> tasks)
        {
            tasks = null;
            try
            {
                _rm.LogEvent(EventLogEntryType.Information, 0, $"DoGetTasks entered for {status}, {taskType}", 4);
                tasks = (List<LibWorkItemBase>)_rm.GetIssData(IssDataCodes.lib_tasks);
                if (tasks == null)
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0, "Failed to get lib_tasks", 3);
                    return HttpStatusCode.InternalServerError;
                }

                if (taskType != null)
                    tasks = tasks.Where(task => task.PayloadType == taskType);

                if (status != null && status != eStatus.unknown)
                    tasks = tasks.Where(task => task.Status == status);

                _rm.LogEvent(EventLogEntryType.Information, 0, $"{tasks.Count()} items found for {taskType}, {status}",5);
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "DoGetTasks err: " + ex.Message,3);
                return HttpStatusCode.InternalServerError;
            }
        }
        public HttpStatusCode SetLibraryLabel(LabelInfo labelInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(labelInfo.proc_id?.m_strLibId))
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0,
                        "SetLibraryLabel failed because one or more of input items is blank", 3);
                    return HttpStatusCode.BadRequest;
                }

                // find the procedure
                using (var libProxy = _rm.GetLibProxy())
                {
                    if (libProxy == null)
                        return HttpStatusCode.ServiceUnavailable;

                    var procId = labelInfo.proc_id.ToProcId();
                    if (libProxy.Proxy.UpdateDmagSetLabel(procId, labelInfo.login, labelInfo.content_item,
                            labelInfo.label, out string strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0,
                            "DoSetContentLabel failed because UpdateDmagSetLabel returend error" + strErr, 3);
                        return HttpStatusCode.InternalServerError;
                    }
                }
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "LabelsController.SetRecorderLabel err: " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode UpdateLibraryPatient(ClinInfoDataExM clinInfo, PersonNameData physician)
        {
            try
            {
                // now - on to library!
                using (var libProxy = _rm.GetLibProxy())
                {
                    // get DMAG
                    if (libProxy == null)
                        return HttpStatusCode.ServiceUnavailable;

                    var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);

                    if (libProxy.Proxy.GetDmag(clinInfo.m_ProcId.ToProcId(), accessInfo, out var strDmagDataJson, out var strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "UpdatePatient fails because GetDmag returned error " + strErr, 3);
                        return HttpStatusCode.InternalServerError;
                    }

                    clsIsmDmag dmagData = (clsIsmDmag)Newtonsoft.Json.JsonConvert.DeserializeObject(strDmagDataJson, typeof(clsIsmDmag));
                    DMAGFileMgr dmag = new DMAGFileMgr(dmagData);


                    //this was copied out of IsmRec::RecordMgr::Record_UpdateClinInfo
                    //it's useful to log why a procedure was modified
                    var updateReasons = new List<string>();
                    bool wasChanged = false;

                    var clinData = clinInfo.m_ClinInfo; //the "new" information
                    var dmagProc = dmag.Procedure; //the "old" information

                    if (clinData.m_PatName != null &&
                    (!clinData.m_PatName.m_strLastName.Equals(dmagProc.m_PatName.m_strLastName) ||
                    !clinData.m_PatName.m_strFirstName.Equals(dmagProc.m_PatName.m_strFirstName) ||
                    !clinData.m_PatName.m_strLogin.Equals(dmagProc.m_PatName.m_strLogin)))
                    {
                        wasChanged = true;
                        updateReasons.Add("Name/MRN");
                    }

                    if (dmagProc.m_dtDob != clinData.m_dtDob)
                    {
                        wasChanged = true;
                        updateReasons.Add("Birthday");
                    }

                    //these enums are different types but with the same name... this is a bit ugly
                    //the enums don't match, IsmRec declares M, F, U. Monsoon declares M, F, U, O
                    if (!string.Equals(dmagProc.m_Sex.ToString(), clinData.m_Sex.ToString(), StringComparison.Ordinal))
                    {
                        wasChanged = true;
                        updateReasons.Add("Sex");
                    }

                    // procedure type
                    //empty is a valid procedure type, null isn't (this getter should never return null anyway)
                    if (clinData.m_strProcType != null && !string.Equals(dmagProc.m_strProcType, clinData.m_strProcType, StringComparison.Ordinal))
                    {
                        wasChanged = true;
                        updateReasons.Add("Procedure Type");
                    }

                    // accession
                    if (!string.IsNullOrEmpty(clinData.m_strAccession) && !string.Equals(dmagProc.m_strAccession, clinData.m_strAccession, StringComparison.Ordinal))
                    {
                        wasChanged = true;
                        updateReasons.Add("Accession");
                    }

                    // procedure id
                    if (!string.IsNullOrEmpty(clinData.m_strExternalProcId) && !string.Equals(dmagProc.m_strExternalProcId, clinData.m_strExternalProcId, StringComparison.Ordinal)) 
                    {
                        wasChanged = true;
                        updateReasons.Add("Procedure ID");
                    }

                    //additional viewer permissions
                    //the OrderBy's make this comparison check easier
                    var newUsers = clinInfo.m_strSharing.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x).ToList();
                    var oldUsers = dmag.m_DmagData.AdditionalUsers.OrderBy(x => x).ToList();
                    if (!oldUsers.SequenceEqual(newUsers, StringComparer.Ordinal))
                    {
                        wasChanged = true;
                        updateReasons.Add("Additional Users");
                    }


                    // modify!
                    dmag.SetClinInfo(clinInfo.m_ClinInfo.m_PatName.m_strLogin, clinInfo.m_ClinInfo.m_PatName.m_strLastName, clinInfo.m_ClinInfo.m_PatName.m_strFirstName);
                    dmag.Procedure.m_Sex = (eSex)Enum.Parse(typeof(eSex), clinInfo.m_ClinInfo.m_Sex.ToString());
                    dmag.Procedure.m_dtDob = clinInfo.m_ClinInfo.m_dtDob.Date;
                    dmag.Procedure.m_strAccession = clinInfo.m_ClinInfo.m_strAccession;
                    dmag.m_DmagData.AdditionalUsers = newUsers;
                    dmag.ClinicalNote = clinInfo.m_strClinicalNote;
                    dmag.Procedure.m_strProcType = clinInfo.m_ClinInfo.m_strProcType;
                    if (physician != null)
                        dmag.SafePhysician = physician;

                    // do update!
                    string strXmlDmag = dmag.GetDmagAsXml();

                    //TODO where to log access... here or library?
                    if (libProxy.Proxy.UpdateDmagFile(strXmlDmag, clinInfo.m_strUserId, wasChanged ? string.Join(", ", updateReasons) : string.Empty, accessInfo, out strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "UpdatePatient fails because UpdateDmagFile returned error " + strErr, 3);
                        return HttpStatusCode.InternalServerError;
                    }
                }
                _rm.LogEvent(EventLogEntryType.Information, 0, "Dmag updated successfully", 3);
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "UpdateLibraryPatient err: " + ex.Message,3);
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode SaveLibraryScreenshot(ScreenshotInfo screenshotInfo, byte[] img)
        {
            HttpStatusCode ret = HttpStatusCode.Unused;
            try
            {
                if (string.IsNullOrEmpty(screenshotInfo.proc_id.m_strLibId) || img == null)
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0, "SaveLibraryScreenshot must have img specified", 3);
                    return ret = HttpStatusCode.BadRequest;
                }

                using (var libProxy = _rm.GetLibProxy())
                {
                    if (libProxy == null)
                        return HttpStatusCode.ServiceUnavailable;

                    // Get location
                    if (libProxy.Proxy.GetPrimaryProcPath(screenshotInfo.proc_id.ToProcId(), out string strPath,
                            out string strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "GetPrimaryProcPath err: " + strErr, 3);
                        return ret = HttpStatusCode.InternalServerError;
                    }


                    // transfer file
                    var msg = new FileUploadMessage
                    {
                        Path = Path.Combine(strPath, screenshotInfo.file_name),
                        LibId = screenshotInfo.proc_id.m_strLibId,
                        StartOffset = 0,
                        ClientId = -1,
                        DataStream = new MemoryStream(img)
                    };

                    using (var transferProxy = _rm.GetLibFileRepProxy())
                    {
                        if (transferProxy == null)
                            return ret = HttpStatusCode.ServiceUnavailable;

                        transferProxy.Proxy.PutFile(msg);
                    }

                    var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);

                    // add to dmag etc.
                    if (libProxy.Proxy.AddImage(screenshotInfo.proc_id.ToProcId(), screenshotInfo.file_name,
                            screenshotInfo.movie, screenshotInfo.offset, accessInfo, out strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "AddImage err: " + strErr, 3);
                        return ret = HttpStatusCode.InternalServerError;
                    }
                }
                return ret = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "SaveLibraryScreenshot exception: " + ex.Message, 3);
                return ret = HttpStatusCode.InternalServerError;
            }
            finally
            {
                // if we are NOT on an EasySuite, play a sound now
                if (_rm.MonsoonCfg != EMonsoonConfig.EasyView)
                {
                    using (var ismRecProxy = _rm.GetIsmRecProxy())
                    {
                        if (ismRecProxy != null)
                        {
                            var sound = ret == HttpStatusCode.OK ? "capture_image" : "capture_failed";
                            ismRecProxy.Proxy.Sounds_Play("record", sound, out _);
                        }
                        else
                            _rm.LogEvent(EventLogEntryType.Warning, 0, "Can't play sound because ismRec is offline",
                                3);
                    }
                }
            }
        }

        public HttpStatusCode GetLibRecord(ProcedureId procId, string login, bool compare, out DmagM procInfo, CultureInfo searchCulture)
        {
            procInfo = null;
            try
            {
                _rm.LogEvent(EventLogEntryType.Information, 0, $"DoSearch entered for {procId} from user {login}, compare is {compare}", 3);
                eLibErr libRes;
                string strErr;
                clsIsmDmag dmagData;
                Dictionary<ePayloadType, TaskInfo> tasks = null;
                List<Dictionary<ESearchFields, SearchResultFieldBase>> compareList = new List<Dictionary<ESearchFields, SearchResultFieldBase>>();
                EUlac ulac = EUlac.Private;
                bool bModify = false;
                using (var libProxy = _rm.GetLibProxy())
                {
                    if (libProxy == null)
                    {
                        _rm.LogEvent(EventLogEntryType.Warning, 0, "GetLibRecord failde because library is offline", 3);
                        return HttpStatusCode.ServiceUnavailable;
                    }

                    var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);

                    // get the dmag content
                    string strDmagDataJson;
                    if (compare)
                        libRes = (eLibErr)libProxy.Proxy.GetDmagFullRec(procId, login, "Accessed from Monsoon", searchCulture.Name, accessInfo, out strDmagDataJson, out compareList, out tasks, out ulac, out bModify, out strErr);
                    else
                        libRes = (eLibErr)libProxy.Proxy.GetDmag(procId, accessInfo, out strDmagDataJson, out strErr);
                    dmagData = (clsIsmDmag)Newtonsoft.Json.JsonConvert.DeserializeObject(strDmagDataJson, typeof(clsIsmDmag));
                }
                _rm.LogEvent(EventLogEntryType.Information, 0, $"GetDmag returned code {libRes}, err {strErr}", 4);
                if (libRes != eLibErr.OK)
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0, "DoSearch failed because GetDmag returned err " + strErr, 3);
                    return HttpStatusCode.InternalServerError;
                }

                // a bit of buisness logic... If DICOM is one of our tasks, but procedure does not have any images with modality - remove dicom
                if (tasks != null && tasks.ContainsKey(ePayloadType.dicomexporttask))
                {

                    int nPicsWithModality = dmagData.Content.Pictures.Count(pic => !string.IsNullOrEmpty(pic.Modality));
                    if (nPicsWithModality == 0)
                        tasks.Remove(ePayloadType.dicomexporttask);
                }


                // transform to dmag monsoon & return 
                DMAGFileMgr dmag = new DMAGFileMgr( dmagData);
                procInfo = new DmagM();
                procInfo.InitLib(_rm, dmag, tasks, ulac, bModify, compareList);

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, $"GetLibRecord for {procId} failed with err {ex.Message}", 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        HttpStatusCode DoSearch(SearchCriteria criteria, out object res, CultureInfo searchCulture)
        {
            res = null;
            try
            {
                // get credentials to pass
                if (string.IsNullOrEmpty(criteria.Login))
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0, "DoSearch failed because valid login not passed in", 3);
                    return HttpStatusCode.BadRequest;
                }

                // build search items
                var search = new SearchItems();
                if (!string.IsNullOrEmpty(criteria.Keyword))
                    AppendKeywordToSearch(search, criteria.Keyword);

                var orderByString = string.IsNullOrWhiteSpace(criteria.OrderBy) ? ESearchFields.date.ToString() : criteria.OrderBy;
                if (!Enum.TryParse<ESearchFields>(orderByString, out var orderBy)) {
                    orderBy = ESearchFields.date;
                }

                // initialize search query, putting in parameters on the way
                search.m_nLimitEntries = criteria.NumberOfItems; //_rm.Cfg.GetValue<int>("procs_per_lib_page");
                search.m_RequestedPageNumber = criteria.RequestedPageNumber;
                search.m_Sort2Asc = new KeyValuePair<ESearchFields, bool>(orderBy, criteria.SortAscending);
                if (criteria.ShowRawOnly)
                {
                    search.Add(ESearchFields.auto_edit_status, ECriteria.EqualsOneOf, new string[] { "EditedWaitingForTimer", "Unedited" });
                    search.Add(ESearchFields.proc_movie_exists, ECriteria.Equals, true);
                }

                // date if any
                // todo - this seems incorrect? can we not have a start without a stop, or vice versa?
                if ((criteria.StartDt.HasValue && criteria.StartDt.Value != DateTimeOffset.MinValue)
                    && (criteria.EndDt.HasValue && criteria.EndDt.Value != DateTimeOffset.MinValue))
                {
                    search.AddDateSearch(criteria.StartDt.Value, criteria.EndDt.Value.AddDays(1));
                }

                // go search!
                var ret = DoSearchLib(search, criteria.Login, KLibSearchFields, searchCulture, out var searchResults);

                if (searchResults == null) {
                    searchResults = new SearchResults { TotalItems = 0, ItemCount = 0, Result = new List<Dictionary<ESearchFields, SearchResultFieldBase>>(), CurrentPage = search.m_RequestedPageNumber };
                }
                MonsoonSearchResults monsoonResults = new MonsoonSearchResults(searchResults.TotalItems, searchResults.CurrentPage, searchResults.ItemCount, new List<Dictionary<ESearchFields, string>>());

                foreach (var searchResult in searchResults.Result)
                {
                    if (null == searchResult)
                        continue;

                    Dictionary<ESearchFields, string> curResults = new Dictionary<ESearchFields, string>();
                    foreach (var curItem in searchResult)
                    {
                        if (curItem.Key != ESearchFields.auto_edit_status)
                        {
                            curResults.Add(curItem.Key, curItem.Value?.ToDataString());
                        }
                        else
                        {
                            AutoEditStatusM monsoonStatus;
                            // TODO - enum search result?
                            if (Enum.TryParse<AutoEditSettings.EVideoEditStatus>(curItem.Value?.ToDataString(), true, out var ismStatus))
                            {
                                monsoonStatus = VideoEditSettingsM.ToEditStatusM(ismStatus);
                            }
                            else
                            {
                                IsmLog.LogEvent(EventLogEntryType.Warning, 0,
                                    $"String {curItem.Value?.ToDataString()} does not parse to monsoon status",
                                    3);
                                monsoonStatus = AutoEditStatusM.other;
                            }

                            curResults.Add(ESearchFields.auto_edit_status, monsoonStatus.ToString());
                        }
                    }

                    monsoonResults.Result.Add(curResults);
                }
                
                // return
                if (ret == HttpStatusCode.OK)
                    res = new
                    {
                        host = _rm.MonsoonLibHost,
                        search_results = monsoonResults
                    };
                return ret;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "LibraryController.Search err: " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }   

        HttpStatusCode DoSearchLib(SearchItems search, string login, ESearchFields[] searchFields, CultureInfo searchCulture, out SearchResults searchResults)
        {
            try
            {
                var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);
                using (var libProxy = _rm.GetLibProxy())
                {
                    if (libProxy == null)
                    {
                        searchResults = null;

                        _rm.LogEvent(EventLogEntryType.Warning, 0, "Library is offline", 3);
                        return HttpStatusCode.ServiceUnavailable;
                    }

                    if (libProxy.Proxy.Search(search, searchFields, login, searchCulture.Name, accessInfo, out searchResults, out var strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "DoSearch failed because LibSearch_SearchAll returned err " + strErr, 1);
                        return HttpStatusCode.InternalServerError;
                    }
                }
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                searchResults = null;

                _rm.LogEvent(EventLogEntryType.Error, 0, "DoSearchLib err: " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        private void AppendKeywordToSearch(SearchItems search, string keyword)
        {
            if (string.Equals(keyword, "-", StringComparison.OrdinalIgnoreCase)) // means no keyword
                return;

            string[] keys = keyword.Split(new[] { ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string strAKey in keys)
            {
                // special backdoor handling for name/mrn/physician
                string[] keyParts = strAKey.Split(':');
                if (keyParts.Length == 2)
                {
                    switch (keyParts[0].ToLower())
                    {
                        case "patientname":
                            search.Add(ESearchFields.patient_name, ECriteria.Equals, keyParts[1]);
                            continue;
                        case "patientid":
                            search.Add(ESearchFields.mrn, ECriteria.Equals, keyParts[1]);
                            continue;
                        case "physician":
                            search.Add(ESearchFields.physician_display, ECriteria.Contains, keyParts[1]);
                            continue;
                        case "accession":
                            search.Add(ESearchFields.accession, ECriteria.Equals, keyParts[1]);
                            continue;
                    }
                }

                // just add key :)
                search.Add(ESearchFields.keyword, ECriteria.Contains, strAKey);
            }
        }

    }
}
 