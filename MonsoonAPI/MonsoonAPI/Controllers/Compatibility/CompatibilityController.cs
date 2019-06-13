using ISM.LibrarySi;
using ISM.Middleware2Si;
using IsmLogCommon;
using IsmStateServer.Types;
using IsmUtility;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace MonsoonAPI.Controllers.Compatibility
{
    [Route("vip/webutils.aspx")]
    public class CompatibilityController : Controller
    {
        protected IMonsoonResMgr m_Rm;
        private UploadUtil m_UploadUtil;
        private CompatibilityGlobal m_Global;
        private IConfiguration _cfg;
        private ILibMgr _libMgr;

        public CompatibilityController(IMonsoonResMgr resMgr, IConfiguration cfg, ILibMgr libMgr, CompatibilityGlobal global,
             INodeMgr ndMgr, IMwMgr mwMgr)
        {
            _cfg = cfg;
            m_Rm = resMgr;
            m_Rm.Init(cfg, ndMgr);
            m_UploadUtil = new UploadUtil(m_Rm, global);
            m_Global = global;
            _libMgr = libMgr;
        }

        [HttpPost("")]
        [HttpGet]
        public IActionResult CompatibilityUploadCalls()
        {
            m_UploadUtil.SetContext(HttpContext);

            string command = Request.Headers["command"];
            m_Rm.LogEvent(EventLogEntryType.Information, 0, $"Compatibility Service entered with command: {command}", 5);

            if (string.IsNullOrWhiteSpace(command))
            {
                var message = "CompatibilityController - No command header";
                m_Rm.LogEvent(EventLogEntryType.Error, 0, message, 1);
                return StatusCode(StatusCodes.Status501NotImplemented);
            }

            string strResponse = string.Empty;
            int statusCode;
            var defaultBreak = "";
            switch (command)
            {
                case "ping":
                    statusCode = StatusCodes.Status204NoContent;
                    break;
                case "login":
                    statusCode = (int)LogIn();
                    break;
                case "get_lib_config":
                    statusCode = (int)GetLibConfig(out strResponse);
                    break;
                case "searchpatients":
                    statusCode = (int)SearchForPatients(CultureInfo.CurrentCulture, out strResponse);
                    break;
                case "get_proc_info":
                    statusCode = (int)GetProcInfo(out strResponse);
                    break;
                case "get_proc_thumbs":
                    statusCode = (int)GetProcThumbs(out strResponse);
                    break;
                case "upload":
                    statusCode = (int)m_UploadUtil.Upload(Request, out strResponse);
                    break;
                case "finish_write":
                    statusCode = (int)m_UploadUtil.FinishWrite(Request, out strResponse);
                    break;
                case "check_space":
                    statusCode = (int)m_UploadUtil.CheckSpace(Request, out strResponse);
                    break;
                case "get_checksum":
                    statusCode = (int)m_UploadUtil.GetChecksum(Request, out strResponse);
                    break;
                case "get_video":
                    statusCode = (int)GetVideo(out strResponse);
                    break;
                case "get_image":
                    statusCode = (int)GetImage(out strResponse);
                    break;
                case "modify_proc":
                    statusCode = (int)ModifyProc();
                    break;
                case "createprocedure":
                    statusCode = (int)CreateProcedure(out strResponse);
                    break;
                case "checkin":
                    statusCode = (int)Checkin();
                    break;
                case "discard_proc":
                    statusCode = (int)DiscardProc();
                    break;
                default:
                    statusCode = StatusCodes.Status501NotImplemented;
                    defaultBreak = "Uncrecognized command.";
                    m_Rm.LogEvent(EventLogEntryType.Error, 0, "Unrecognized command " + command, 3);
                    break;
            }

            m_Rm.LogEvent(EventLogEntryType.Information, 0, $"For command {command}, returning code {statusCode}", 5);


            if (statusCode == StatusCodes.Status200OK && !string.IsNullOrWhiteSpace(strResponse))
            {
                return StatusCode(statusCode, strResponse);
            }

            if (!string.IsNullOrWhiteSpace(defaultBreak))
            {
                return StatusCode(statusCode, defaultBreak);
            }

            return StatusCode(statusCode);
        }

        public HttpStatusCode LogIn()
        {
            try
            {
                m_Rm.LogEvent(EventLogEntryType.Information, 0, "Entering LogIn", 3);
                string userCredentials = string.Empty, strUserId = string.Empty, strPwd = null, strErr;

                string strLogin = Request.Headers["login"];
                if (!string.IsNullOrEmpty(strLogin))
                {


                    m_Rm.LogEvent(EventLogEntryType.Information, 0, "Entering LogIn method for " + strLogin, 3);
                    // get encode key & decode the passed-in parameters
                    var key = _cfg.GetChildren().Single(cfg => cfg.Key.Equals("redirect_encyrpt_key"));
                    var strKey = key.Value;

                    var lstArgs = new Dictionary<UCDRC4.ELoginFields, string>
                    {
                        [UCDRC4.ELoginFields.user_id] = string.Empty,
                        [UCDRC4.ELoginFields.pwd] = string.Empty
                    };

                    if (!UCDRC4.GetURLArgs(strLogin, ref lstArgs, new KeyValuePair<string, bool>(strKey, false), out strErr))
                        throw new Exception(strErr);

                    m_Rm.LogEvent(EventLogEntryType.Information, 0, "GetURLArgs successful for user " + lstArgs[UCDRC4.ELoginFields.user_id], 3);

                    strUserId = lstArgs[UCDRC4.ELoginFields.user_id];
                    strPwd = lstArgs[UCDRC4.ELoginFields.pwd];
                }
                else
                {
                    userCredentials = Request.Headers["credentials"];
                    if (string.IsNullOrEmpty(userCredentials))
                    {
                        m_Rm.LogEvent(EventLogEntryType.Error, 0, "No user login information or credentials passed in", 1);
                        return HttpStatusCode.BadRequest;
                    }
                }

                m_Rm.LogEvent(EventLogEntryType.Information, 0, "LogIn: Credentials Retreived from Request.Headers", 3);

                // if credentials are already set, and it's the same - don't change anything
                string strStoredCredentials = HttpContext.Session.GetString(m_Global.SESSION_CREDENTIALS);
                if (!string.IsNullOrEmpty(strStoredCredentials))
                {
                    if (string.IsNullOrEmpty(userCredentials))
                        userCredentials = MwUtils.Credentials(strUserId, strPwd);

                    if (string.Equals(strStoredCredentials, userCredentials, StringComparison.Ordinal))
                    {
                        m_Rm.LogEvent(EventLogEntryType.Information, 0, "Credentials have not changed.", 8);
                        return HttpStatusCode.Accepted;
                    }
                }

                m_Rm.LogEvent(EventLogEntryType.Information, 0, "LogIn: Credentials not stored in session", 3);

                // log in
                HttpStatusCode ret = m_Global.DoLogIn(ref strUserId, strPwd, Request.GetUri().Host, ref userCredentials, out strErr);
                if (ret == HttpStatusCode.Accepted)
                {
                    // set variables & return
                    m_Global.SetCredentials(userCredentials, strUserId, HttpContext);
                    m_Rm.LogEvent(EventLogEntryType.Information, 0, "LogIn successful for user " + strUserId, 4);
                }
                else
                {
                    m_Rm.LogEvent(EventLogEntryType.Warning, 0, "Login failed with err " + strErr, 1);
                }

                return ret;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "Error logging in - " + ex.Message, 1);
                return HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Top level method to get lib config
        /// </summary>
        private HttpStatusCode GetLibConfig(out string strStream)
        {
            strStream = string.Empty;
            try
            {
                m_Rm.LogEvent(EventLogEntryType.Information, 0, "WebUtils.GetLibConfig", 3);

                HttpStatusCode ret = m_UploadUtil.CheckSystemReadiness(true, false, out _);
                if (ret != HttpStatusCode.OK)
                    return ret;

                using (var libProxy = m_Rm.GetLibProxy())
                {
                    if (libProxy == null)
                        throw new Exception("Library is offline");

                    var user = HttpContext.Session.GetString(m_Global.SESSION_USER_NAME);

                    if (libProxy.Proxy.GetLibrariesForUser(user, out var libs2Clinical, out string strError) != 0)
                        throw new Exception("GetLibrariesForUser failed with err " + strError);


                    // if clinical has been specified, remove nodes that do not fit
                    bool? bSearchClinical = SearchClinical();
                    if (bSearchClinical != null)
                    {
                        libs2Clinical = libs2Clinical.Where(lib => lib.Value == bSearchClinical).ToDictionary(l => l.Key, l => l.Value, StringComparer.OrdinalIgnoreCase);
                    }
                    var libElements = libs2Clinical.Select(lib => new XElement("library",
                        new XAttribute("clinical", lib.Value.ToString()), lib.Key));
                    XElement docResults = new XElement("libraries", libElements);

                    // convert to JSON
                    m_Rm.LogEvent(EventLogEntryType.Information, 0, "About to convert string to JSON", 5);
                    strStream = Xml2Json(docResults);
                }
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "WebUtils.GetLibConfig error " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        private string Xml2Json(XmlDocument doc)
        {

            string strJson = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc.DocumentElement);
            // for now, clients of WebUtils cannot handle attributes; replace all "@XXX": with "XXX":
            strJson = Regex.Replace(strJson, "\"[@]\\b(?<word1>\\w+)\\b\":", "\"${word1}\":", RegexOptions.IgnoreCase);


            return strJson;
        }
        private string Xml2Json(XElement doc)
        {

            string strJson = Newtonsoft.Json.JsonConvert.SerializeXNode(doc);
            // for now, clients of WebUtils cannot handle attributes; replace all "@XXX": with "XXX":
            strJson = Regex.Replace(strJson, "\"[@]\\b(?<word1>\\w+)\\b\":", "\"${word1}\":", RegexOptions.IgnoreCase);


            return strJson;
        }

        /// <summary>
        /// Reader passed-in header to determine if clinical or non-clinical is requested
        /// </summary>
        /// <returns>true for clinical, false for non-clnical, null for non-specified</returns>
        bool? SearchClinical()
        {
            string strLibType = Request.Headers["libtype"];
            if (string.IsNullOrEmpty(strLibType))
                return null;
            else
                return string.Equals(strLibType, "clinical", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Top level method - search for patients
        /// </summary>
        private HttpStatusCode SearchForPatients(CultureInfo searchCulture, out string strStream)
        {
            strStream = string.Empty;
            try
            {
                m_Rm.LogEvent(EventLogEntryType.Information, 0, "WebUtils.SearchForPatients; " + Request.Headers["no_text_in_json"], 3);

                HttpStatusCode ret = m_UploadUtil.CheckSystemReadiness(true, false, out _);
                if (ret != HttpStatusCode.OK)
                    return ret;

                // create search structure
                SearchItems searchItems = GetSearchItemsForQuery();
                string strStoredUser = HttpContext.Session.GetString(m_Global.SESSION_USER_NAME);

                ESearchFields[] searchFields =  {ESearchFields.image, ESearchFields.video_thumb, ESearchFields.title, ESearchFields.desc,
                        ESearchFields.date,  ESearchFields.libid, ESearchFields.libname, ESearchFields.image_count, ESearchFields.video_count, ESearchFields.volumename,
                        ESearchFields.lastname, ESearchFields.firstname, ESearchFields.mrn,                     ESearchFields.proc_type                    };
                m_Rm.LogEvent(EventLogEntryType.Information, 0, "SearchForPatients - all libraries with structure " + searchItems.Description, 3);

                ret = _libMgr.DoSearchLib(ref searchItems, strStoredUser, searchFields, searchCulture, out var searchResults);

                if (ret != HttpStatusCode.OK)
                    return ret;

                // create output
                XElement elRes = new XElement("searchresults", new XElement("count", searchItems.m_nLimitEntries.ToString()), new XElement("SEARCH_ID", "1"));

                foreach (var entry in searchResults.Result)
                {

                    var entryElements = entry.Select(dict => new XElement(dict.Key.ToString(), dict.Value?.ToDataString()));
                    XElement elEntry = new XElement("libentry", entryElements);
                    elRes.Add(elEntry);
                }

                // conver to JSON
                strStream = Newtonsoft.Json.JsonConvert.SerializeXNode(elRes);

                return HttpStatusCode.OK;

            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "WebUtils.SearchForPatients error " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }
        private SearchItems GetSearchItemsForQuery()
        {
            SearchItems searchItems = new SearchItems();

            string strLastName = Request.Headers["lastname"];
            if (!string.IsNullOrEmpty(strLastName))
                searchItems.Add(new SearchItem(ESearchFields.lastname, ECriteria.StartsWith, strLastName));

            string strMrn = Request.Headers["mrn"];
            if (!string.IsNullOrEmpty(strMrn))
                searchItems.Add(new SearchItem(ESearchFields.mrn, ECriteria.StartsWith, strMrn));

            string strKeyword = Request.Headers["keyword"];
            if (!string.IsNullOrEmpty(strKeyword))
                searchItems.Add(new SearchItem(ESearchFields.keyword, ECriteria.Contains, strKeyword));

            string strTitle = Request.Headers["title"];
            if (!string.IsNullOrEmpty(strTitle))
                searchItems.Add(new SearchItem(ESearchFields.title, ECriteria.Equals, strTitle));

            string strStartDate = Request.Headers["startdate"];
            if (!string.IsNullOrEmpty(strStartDate))
            {
                DateTime dt = XmlConvert.ToDateTime(strStartDate, "yyyyMMdd");
                searchItems.Add(new SearchItem(ESearchFields.date, ECriteria.GreaterThan, dt.Date));
            }

            string strEndDate = Request.Headers["enddate"];
            if (!string.IsNullOrEmpty(strEndDate))
            {
                DateTime dt = XmlConvert.ToDateTime(strEndDate, "yyyyMMdd");
				dt = dt.AddDays(1); //to include items from endDate, search for less than midnight the following day
				// todo - ideally the above logic should be handled by caller and we should just search based on the parameters given...but in this case the caller is easycut
                searchItems.Add(new SearchItem(ESearchFields.date, ECriteria.LessThan, dt.Date));
            }

            string strLibName = Request.Headers["libname"];
            if (!string.IsNullOrEmpty(strLibName))
            {
                searchItems.Add(new SearchItem(ESearchFields.libname, ECriteria.Equals, strLibName));
                searchItems.m_strLibName = strLibName;
            }

            string strStartRec = Request.Headers["startrec"];
            if (!string.IsNullOrEmpty(strStartRec))
                searchItems.m_nStartIndex = int.Parse(strStartRec);

            string strNumRecs = Request.Headers["numrecs"];
            if (!string.IsNullOrEmpty(strNumRecs))
                searchItems.m_nLimitEntries = int.Parse(strNumRecs);


            string strExistingId = Request.Headers["existingid"];
            if (!string.IsNullOrEmpty(strExistingId) && !string.Equals(strExistingId, "0", StringComparison.OrdinalIgnoreCase))
                m_Rm.LogEvent(EventLogEntryType.Warning, 0, "Ignoring existing id of " + strExistingId, 3);
            /*    searchItems.m_nSearchId = long.Parse(strExistingId);*/

            string strSortField = Request.Headers["sortfield"];
            string strSortAscending = Request.Headers["sortasc"];
            if (!string.IsNullOrEmpty(strSortField))
            {
                ESearchFields sortField;
                if (Enum.TryParse(strSortField, true, out sortField))
                {
                    bool bSortAsc = !string.IsNullOrEmpty(strSortAscending) ? bool.Parse(strSortAscending) : true;
                    searchItems.m_Sort2Asc = new KeyValuePair<ESearchFields, bool>(sortField, bSortAsc);
                }
            }

            return searchItems;
        }

        /// <summary>
        ///  Given a pic, find corresponding movie; if none found - return null (there doesn't have to be a movie corresponding to the pic)
        /// </summary>
        private IsmDmag.clsMovie GetSourceMovie(IsmDmag.DMAGFileMgr dmag, IsmDmag.clsPicture pic)
        {
            try
            {
                var thisStreamMovies = dmag.Content.Movies.Where(mov => mov.Stream.Equals(pic.Stream, StringComparison.CurrentCultureIgnoreCase));
                foreach (IsmDmag.clsMovie movie in thisStreamMovies)
                {
                    // if this pic has been taken during the time frame of this movie - return this movie
                    if (movie.Created <= pic.Created && (movie.Created + new TimeSpan(0, 0, (int)movie.Length)) >= pic.Created)
                        return movie;
                }
                // none found
                return null;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "GetSourceMovie failed with err " + ex.Message, 3);
                return null;
            }
        }

        /// <summary>
        /// top level method - return all thumbs for current procedure
        /// </summary>
        private HttpStatusCode GetProcInfo(out string strResponse)
        {
            strResponse = string.Empty;
            try
            {
                // get params
                string strLibId = Request.Headers["libid"];
                string strLibName = Request.Headers["libname"];

                m_Rm.LogEvent(EventLogEntryType.Information, 0, string.Format("WebUtils.GetProcInfo for id {0}, lib {1}", strLibId, strLibName), 3);
                if (string.IsNullOrEmpty(strLibId) || string.IsNullOrEmpty(strLibName))
                {
                    m_Rm.LogEvent(EventLogEntryType.Warning, 0, "Lib id, lib name not passed in", 3);
                    return HttpStatusCode.BadRequest;
                }

                string strCredentials;
                HttpStatusCode ret = m_UploadUtil.CheckSystemReadiness(true, false, out strCredentials);
                if (ret != HttpStatusCode.OK)
                    return ret;

                ProcedureId procId = new ProcedureId(strLibId, strLibName);
                _libMgr.GetProcPath(procId, out string strProcPath);

                IsmDmag.DMAGFileMgr dmag = new IsmDmag.DMAGFileMgr(strProcPath);
                if (dmag.m_Status != ISM.Library.Types.eStatus.success)
                    throw new Exception("Failed tro load dmag properly");


                XElement elProcInfo = new XElement("proc_info",
                                        new XElement("libid", strLibId),
                                        new XElement("libname", strLibName),
                                        new XElement("data_path", strProcPath));


                foreach (IsmDmag.clsPicture pic in dmag.Content.Pictures)
                {

                    XElement elImage = new XElement("image", pic.RelativePath);
                    elImage.Add(new XAttribute("time", pic.Created.ToString("yyyy-MM-ddTHH:mm:ss.fff")));
                    elProcInfo.Add(elImage);
                }


                foreach (IsmDmag.clsMovie mov in dmag.Content.Movies)
                {
                    XElement elMovie = new XElement("movie", mov.RelativePath,
                                            new XAttribute("stream", mov.Stream),
                                            new XAttribute("time", mov.Created.ToString("yyyy-MM-ddTHH:mm:ss.fff")));
                    elProcInfo.Add(elMovie);
                }


                for (int nMarker = 0; nMarker < dmag.Content.Markers.Count; nMarker++)
                {

                    IsmDmag.clsMarker marker = dmag.Content.Markers[nMarker];
                    // FB 14612: we now no longer add a marker per each channel recording image; rather, adding one marker for each "capture" event. Therefore, for backwards compatibility, do NOT 
                    //  include image bookrmark if previous image bookmark in the list is less than 2 seconds before this one
                    if (marker.MarkerType == IsmDmag.clsMarker.enumMarkerType.eImage && nMarker != 0 &&
                        dmag.Content.Markers[nMarker - 1].MarkerType == IsmDmag.clsMarker.enumMarkerType.eImage &&
                        (marker.Created - dmag.Content.Markers[nMarker - 1].Created).TotalSeconds < 2)
                        continue;


                    XElement elMarker = new XElement("marker",
                            new XAttribute("type", marker.MarkerType.ToString()),
                            new XAttribute("time", marker.Created.ToString("yyyyMMddTHHmmss")),
                            new XAttribute("text", marker.Annotations));

                    elProcInfo.Add(elMarker);
                }


                //strResponse as JSON 
                strResponse = Xml2Json(elProcInfo);
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "GetProcInfo failed with err " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// top level method - return all thumbs for current procedure
        /// </summary>
        private HttpStatusCode GetProcThumbs(out string strResponse)
        {
            strResponse = string.Empty;
            try
            {
                // get params
                string strLibId = Request.Headers["libid"];
                string strLibName = Request.Headers["libname"];
                if (string.IsNullOrEmpty(strLibId) || string.IsNullOrEmpty(strLibName))
                {
                    m_Rm.LogEvent(EventLogEntryType.Warning, 0, "Lib id, lib name not passed in", 3);
                    return HttpStatusCode.BadRequest;
                }
                string strType = Request.Headers["type"];
                bool bIncludeImages = false;
                if (Request.Headers.ContainsKey("images"))
                    bIncludeImages = bool.Parse(Request.Headers["images"]);


                ProcedureId procId = new ProcedureId(strLibId, strLibName);
                // type is video, include images is false


                m_Rm.LogEvent(EventLogEntryType.Information, 0, $"WebUtils.GetProcThumbs for id {procId}", 3);

                HttpStatusCode ret = m_UploadUtil.CheckSystemReadiness(true, false, out _);
                if (ret != HttpStatusCode.OK)
                    return ret;

                ret = _libMgr.GetProcPath(procId, out string strProcPath);
                if (ret != HttpStatusCode.OK)
                    return ret;

                // load dmag, get names
                IsmDmag.DMAGFileMgr dmag = new IsmDmag.DMAGFileMgr(strProcPath);

                XElement elReturn = new XElement("lib_thumbs",
                                        new XElement("libid", strLibId),
                                        new XElement("libname", strLibName));


                // get a list of all objects - pics & movies
                List<IsmDmag.clsIsmDmagPath> dmagContent = new List<IsmDmag.clsIsmDmagPath>();
                if (string.IsNullOrEmpty(strType) || string.Equals(strType, "pics", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (IsmDmag.clsPicture pic in dmag.Content.Pictures)
                        dmagContent.Add(pic);
                }
                if (string.IsNullOrEmpty(strType) || string.Equals(strType, "video", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (IsmDmag.clsMovie mov in dmag.Content.Movies)
                        dmagContent.Add(mov);
                }


                foreach (IsmDmag.clsIsmDmagPath obj in dmagContent)
                {
                    bool bPic = obj is IsmDmag.clsPicture;
                    string strStream;

                    // if video, re-direct to thumb
                    var procName = bPic ? obj.RelativePath : ((IsmDmag.clsMovie)obj).ThumbName;
                    string strAbsPath = Path.Combine(strProcPath, procName);
                    if (GetImageAsString(strAbsPath, bPic, out strStream) != HttpStatusCode.OK)
                    {
                        m_Rm.LogEvent(EventLogEntryType.Warning, 0,
                            "Failed to get thumb for procedure: " + obj.RelativePath, 3);
                        continue;
                    }


                    // write to output
                    string strTag = bPic ? "pic" : "video_thumb";
                    XElement elItem = new XElement(strTag,
                        new XElement("name", obj.RelativePath),
                        new XElement("time", obj.Created.ToString("yyyy-MM-ddTHH:mm:ss.fff")),
                        new XElement("stream", strStream));


                    elReturn.Add(elItem);
                }


                //strResponse as JSON
                strResponse = Newtonsoft.Json.JsonConvert.SerializeXNode(elReturn);


                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "GetProcThumbs failed with err " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        private HttpStatusCode GetImageAsString(string strPath, bool bThumb, out string strResponse)
        {
            m_Rm.LogEvent(EventLogEntryType.Information, 0,
                $"GetImageAsString entered for {(bThumb ? "thumbnail" : "full")} {strPath}", 3);
            strResponse = string.Empty;
            try
            {
                if (bThumb)
                {
                    string strThumbName = IsmDmag.clsPicture.GetThumbnailFileName(strPath);
                    string strThumbPath = Path.Combine(Path.GetDirectoryName(strPath), strThumbName);
                    if (System.IO.File.Exists(strThumbPath))
                        strPath = strThumbPath;
                }
                if (!System.IO.File.Exists(strPath)) // no image...
                {
                    m_Rm.LogEvent(EventLogEntryType.Warning, 0, "Failed to retrieve image " + strPath, 3);
                    return HttpStatusCode.Gone;
                }

                System.Drawing.Image img = System.Drawing.Image.FromFile(strPath);
                MemoryStream ms = new MemoryStream();
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = ms.ToArray();
                strResponse = Convert.ToBase64String(arr);
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0,
                    $"GetImageAsString failed for {strPath} with err {ex.Message}", 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Top level method - get image based on lib id/name nad image name
        /// </summary>
        private HttpStatusCode GetImage(out string strResponse)
        {
            strResponse = string.Empty;
            try
            {
                // authorize
                string strCredentials;
                HttpStatusCode ret = m_UploadUtil.CheckSystemReadiness(true, false, out strCredentials);
                if (ret != HttpStatusCode.OK)
                    return ret;

                // parameters
                string strLibId = Request.Headers["libid"];
                string strLibName = Request.Headers["libname"];
                string strImageName = Request.Headers["imagename"];
                if (string.IsNullOrEmpty(strLibId) || string.IsNullOrEmpty(strLibName) || string.IsNullOrEmpty(strImageName))
                {
                    m_Rm.LogEvent(EventLogEntryType.Warning, 0, "parameters not passed into GetImage request", 3);
                    return HttpStatusCode.BadRequest;
                }

                m_Rm.LogEvent(EventLogEntryType.Information, 0,
                    $"GetImageAsString entered for lib id {strLibId}, lib name {strLibName}, image {strImageName}", 3);

                ProcedureId procId = new ProcedureId(strLibId, strLibName);
                ret = _libMgr.GetProcPath(procId, out string strProcPath);
                if (ret != HttpStatusCode.OK)
                {
                    m_Rm.LogEvent(EventLogEntryType.Error, 0, $"Failed to get path for {procId}.", 3);
                    return HttpStatusCode.InternalServerError;
                }

                // load & return
                string strImage = Path.Combine(strProcPath, strImageName);
                return GetImageAsString(strImage, false, out strResponse);
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "GetImage failed with err " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Top level method - get video based on lib id/name and video name
        /// </summary>
        private HttpStatusCode GetVideo(out string strResponse)
        {
            strResponse = string.Empty;
            try
            {
                string strLibId = Request.Headers["libid"];
                string strLibName = Request.Headers["libname"];
                string strVideoName = Request.Headers["videoname"];
                if (string.IsNullOrEmpty(strLibId) || string.IsNullOrEmpty(strLibName) || string.IsNullOrEmpty(strVideoName))
                {
                    m_Rm.LogEvent(EventLogEntryType.Warning, 0, "parameters not passed into GetImage request", 3);
                    return HttpStatusCode.BadRequest;
                }

                ProcedureId procId = new ProcedureId(strLibId, strLibName);
                HttpStatusCode ret = _libMgr.GetProcPath(procId, out string procPath);
                if (ret != HttpStatusCode.OK)

                    return ret;

                strResponse = Path.Combine(procPath, strVideoName);
                return HttpStatusCode.OK;
            }

            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "GetImage failed with err " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;

            }
        }
        private HttpStatusCode ModifyProc()
        {
            try
            {
                // is lib online? if not, don't bother
                string strCredentials;
                HttpStatusCode ret = m_UploadUtil.CheckSystemReadiness(true, false, out strCredentials);
                if (ret != HttpStatusCode.OK)
                    return ret;

                // get parameters & validate
                string strLibId = Request.Headers["libid"];
                string strLibName = Request.Headers["libname"];
                string strProcPath = Request.Headers["path"];
                if (string.IsNullOrEmpty(strLibId) || string.IsNullOrEmpty(strLibName))
                {
                    m_Rm.LogEvent(EventLogEntryType.Warning, 0, "parameters not passed into ModifyProc request", 3);
                    return HttpStatusCode.BadRequest;
                }
                ProcedureId procId = new ProcedureId(strLibId, strLibName);

                // get path & dmag
                if (string.IsNullOrEmpty(strProcPath))
                {
                    ret = _libMgr.GetProcPath(procId, out strProcPath);
                    if (ret != HttpStatusCode.OK)
                        return HttpStatusCode.NotFound;
                }

                // get dmag
                IsmDmag.DMAGFileMgr dmag = new IsmDmag.DMAGFileMgr(strProcPath);

                // FB20028: EasyView will show procedures as dual stream (with all content coming from one channel) unless
                //  there is one and only one entry under 'channels', and each content item is stamped with that channel. So,
                //  put a blank 'Rec1' channel into dmag. 
                if (dmag.Content.Channels == null || dmag.Content.Channels.Count < 1)
                    dmag.Content.Channels = new[] { new IsmDmag.ChannelInfo("Rec1", string.Empty) }.ToList();

                var chan = dmag.Content.Channels[0].Channel;

                //attempt to determine why this procedure is being modified
                var updateReasons = new List<string>();

                // title & description?
                string strTitle = Request.Headers["title"];
                if (!string.IsNullOrEmpty(strTitle))
                {
                    dmag.Title = strTitle;
                    updateReasons.Add("Title");
                }

                string strDescription = Request.Headers["description"];
                if (!string.IsNullOrEmpty(strDescription))
                {
                    dmag.Description = strDescription;
                    updateReasons.Add("Description");
                }

                // images?
                string strPicsJson = Request.Headers["pictures"];
                if (!string.IsNullOrEmpty(strPicsJson))
                {
                    updateReasons.Add("Pictures");
                    try
                    {
                        XDocument docPics = Newtonsoft.Json.JsonConvert.DeserializeXNode(strPicsJson);
                        var pics = docPics.Root.Elements("picture");
                        dmag.Content.Pictures = new IsmDmag.clsPictureList();
                        foreach (var pic in pics)
                        {
                            var newPic = new IsmDmag.clsPicture(pic.Element("path")?.Value)
                            {
                                Created = XmlDateTimeOffset.ParseConvertOld(pic.Element("created")?.Value),
                                Stream = chan //FB23102 The php app doesn't know how to handle "EasyCut" as a channel
                            };
                            dmag.Content.Pictures.Add(newPic);
                        }
                        m_Rm.LogEvent(EventLogEntryType.Information, 0, "Images set. Num images: " + dmag.Content.Pictures.Count, 3);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Setting new movies content failed with err " + ex.Message);
                    }
                }

                // movies?
                string strMoviesJson = Request.Headers["movies"];
                if (!string.IsNullOrEmpty(strMoviesJson))
                {
                    updateReasons.Add("Movies");
                    try
                    {
                        XDocument docMovies = Newtonsoft.Json.JsonConvert.DeserializeXNode(strMoviesJson);
                        if (docMovies.Root == null) throw new Exception("Can't serialize movie json to xml");
                        var movies = docMovies.Root.Elements("movie");

                        foreach (var mov in movies)
                        {
                            var newMovie = new IsmDmag.clsMovie(mov.Element("path")?.Value)
                            {
                                Created = XmlDateTimeOffset.ParseConvertOld(mov.Element("created")?.Value),
                                Length = double.Parse(mov.Element("len")?.Value ?? throw new InvalidOperationException()),
                                ThumbName = mov.Element("posterframe")?.Element("path")?.Value,
                                Stream = chan //FB23102 The php app doesn't know how to handle "EasyCut" as a channel
                            };
                            dmag.Content.Movies.Add(newMovie);
                        }

                        m_Rm.LogEvent(EventLogEntryType.Information, 0, "Movies set. Num movies: " + dmag.Content.Movies.Count.ToString(), 3);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Setting new movies content failed with err " + ex.Message);
                    }
                }

                //no username here
                var accessInfo = new AccessInfo(Communications.GetFirstLocalAdapterIPv4Address(Communications.DefaultNetworkInterfaceName), Environment.UserName, "MonsoonWebCompatibility", Environment.MachineName, nameof(ModifyProc));

                using (var libMgr = m_Rm.GetLibProxy())
                {
                    if (libMgr.Proxy.UpdateDmagFile(dmag.GetDmagAsXml(), MwUtils.ADMIN_LOGIN, updateReasons.Count > 0 ? string.Join(", ", updateReasons) : string.Empty, accessInfo, out string strErr) != 0)
                    {
                        m_Rm.LogEvent(EventLogEntryType.Error, 0, "UpdateDmagFile err: " + strErr, 3);
                        return HttpStatusCode.InternalServerError;
                    }
                }

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "ModifyProc failed with err " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Top level method - search for patients
        /// </summary>
        private HttpStatusCode CreateProcedure(out string strRecordingFolder)
        {
            strRecordingFolder = string.Empty;
            try
            {
                m_Rm.LogEvent(EventLogEntryType.Information, 0, "WebUtils.CreateProcedure", 3);

                string strCredentials;
                HttpStatusCode ret = m_UploadUtil.CheckSystemReadiness(true, true, out strCredentials);
                if (ret != HttpStatusCode.OK)
                    return ret;

                //parameters
                string strLibName = Request.Headers["libname"];
                string strTitle = Request.Headers["title"];
                string strDescription = Request.Headers["desc"];
                string strUser = Request.Headers["user"];
                if (string.IsNullOrEmpty(strUser))
                    strUser = HttpContext.Session.GetString(m_Global.SESSION_USER_NAME);
                if (string.IsNullOrEmpty(strLibName) || string.IsNullOrEmpty(strTitle))
                {
                    m_Rm.LogEvent(EventLogEntryType.Warning, 0, "CreateProcedure failed because lib name or title not passed in", 3);
                    return HttpStatusCode.BadRequest;
                }

                //have to make it manually
                var accessInfo = new AccessInfo(Communications.GetFirstLocalAdapterIPv4Address(Communications.DefaultNetworkInterfaceName), strUser, "MonsoonWebCompatibility", Environment.MachineName, nameof(CreateProcedure));

                PersonNameData user;
                using (var mwMgr = m_Rm.GetMwProxy())
                {
                    if (mwMgr.Proxy.Users_GetXml(strUser, MwUtils.SystemCredentials, accessInfo, out string userXml, out string strErr) != 0)
                        throw new Exception(string.Format("GetUser failed for {0} with err {1}", strUser, strErr));
                    user = new PersonNameData(IsmXmlDoc.FromXml(userXml).DocumentElement);
                }


                // space
                string strSpaceRequiredMB = Request.Headers["space_mb"];
                double dSpaceMB;
                if (!double.TryParse(strSpaceRequiredMB, out dSpaceMB))
                {
                    m_Rm.LogEvent(EventLogEntryType.Warning, 0, "Legitimate space required not passed in", 3);
                    return HttpStatusCode.BadRequest;
                }

                // make id & create
                ProcedureId procId = new ProcedureId
                {
                    m_strLibName = strLibName,
                    m_strLibId = $"{XmlConvert.ToString(DateTime.UtcNow, "yyyy_MM_ddTHH_mm_ss")}_EasyCut"
                };
                using (var libMgr = m_Rm.GetLibProxy())
                {
                    if (libMgr.Proxy.ReserveSpace(ref procId, dSpaceMB, out strRecordingFolder, accessInfo, out string strErr) != 0)
                    {
                        m_Rm.LogEvent(EventLogEntryType.Error, 0, "ReserveSpace failed with err " + strErr, 3);
                        return HttpStatusCode.InternalServerError;
                    }

                    if (!Directory.Exists(strRecordingFolder))
                        Directory.CreateDirectory(strRecordingFolder);

                    m_Rm.LogEvent(EventLogEntryType.Information, 0, $"CreateProcedure successfully created procedure {procId} at path {strRecordingFolder}", 5);


                    IsmDmag.DMAGFileMgr dmag = new IsmDmag.DMAGFileMgr
                    {
                        IsClinical = false,
                        ProcId = procId,
                        History = { Created = DateTimeOffset.Now },
                        SafePhysician = user
                    };
                    dmag.SetNonClinInfo(strTitle, strDescription);
                    dmag.Procedure = null;
                    dmag.XmlFileName = IsmDmag.DMAGFileMgr.GetDmagPath(strRecordingFolder);
                    dmag.WriteXmlFile();
                }
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "WebUtils.CreateProcedure error " + ex.Message, 1);
                strRecordingFolder = null;
                return HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Top level method - search for patients
        /// </summary>
        private HttpStatusCode Checkin()
        {
            try
            {
                m_Rm.LogEvent(EventLogEntryType.Information, 0, "WebUtils.Checkin", 3);

                string strCredentials;
                HttpStatusCode ret = m_UploadUtil.CheckSystemReadiness(true, false, out strCredentials);
                if (ret != HttpStatusCode.OK)
                    return ret;

                string strPath = Request.Headers["path"];
                if (string.IsNullOrEmpty(strPath))
                {
                    m_Rm.LogEvent(EventLogEntryType.Error, 0, "Path not passed in", 3);
                    return HttpStatusCode.BadRequest;
                }

                // read dmag to get lib id & name
                IsmDmag.DMAGFileMgr dmag = new IsmDmag.DMAGFileMgr(strPath);
                if (dmag.m_Status != ISM.Library.Types.eStatus.success)
                    throw new Exception("Unable to read dmag at " + strPath);

                using (var libMgr = m_Rm.GetLibProxy())
                {
                    if (libMgr.Proxy.CompleteCheckIn(dmag.ProcId, false, out string strErr) != 0)
                        throw new Exception("Library.CompleteCheckIn failed with err " + strErr);
                }

                m_Rm.LogEvent(EventLogEntryType.Information, 0, $"Checkin succeeded for path {strPath}", 3);
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "WebUtils.Checkin error " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Discard procedure THAT HAS NOT YET BEEN CHECKED IN
        /// </summary>
        private HttpStatusCode DiscardProc()
        {
            try
            {
                // is lib online? if not, don't bother
                HttpStatusCode ret = m_UploadUtil.CheckSystemReadiness(true, false, out _);
                if (ret != HttpStatusCode.OK)
                    return ret;

                // get parameters & validate
                string strLibId = Request.Headers["libid"];
                string strLibName = Request.Headers["libname"];
                //string strPrimaryPath = Request.Headers["path"];
                if ((string.IsNullOrEmpty(strLibId) || string.IsNullOrEmpty(strLibName)) /*&& string.IsNullOrEmpty(strPrimaryPath)*/)
                {
                    m_Rm.LogEvent(EventLogEntryType.Warning, 0, "parameters not passed into DiscardProc request", 3);
                    return HttpStatusCode.BadRequest;
                }

                // get dmag
                ProcedureId procId = new ProcedureId(strLibId, strLibName);
                using (var lib = m_Rm.GetLibProxy())
                {
                    if (lib.Proxy.DeleteLibFolder(procId, false, out var strErr) != 0)
                        throw new Exception("DeleteLibFolder failed with err " + strErr);
                }

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "DiscardProc failed with err " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

    }
}
