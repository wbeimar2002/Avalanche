using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using IsmLogCommon;
using IsmRec.Types;
using IsmStateServer;
using IsmStateServer.Types;
using IsmUtility;
using ISM.Library.Types;
using MonsoonAPI.models;
using ISM.LibrarySi;
using Microsoft.AspNetCore.Http;
using IsmDmag;

namespace MonsoonAPI
{
    public interface IPrintMgr
    {
        void Init(IMonsoonResMgr rm,  ISettingsMgr settingsMgr);
        HttpStatusCode GeneratePrintReport(PrintReportGenerateInfo printRequest);
    }

    public class PrintMgr : IPrintMgr
    {
        private  IMonsoonResMgr _rm;
        private  ISettingsMgr _settingsMgr;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PrintMgr(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // todo - why? this breaks dependency injection.
        public void Init(IMonsoonResMgr rm, ISettingsMgr settingsMgr)
        {
            _rm = rm;
            _settingsMgr = settingsMgr;
        }

        public HttpStatusCode GeneratePrintReport(PrintReportGenerateInfo printRequest)
        {
            try
            {
                // do sanity check 
                _rm.LogEvent(EventLogEntryType.Information, 0, "PrintMgr.GeneratePrintReport entered", 4);
                if (printRequest == null)
                    return HttpStatusCode.BadRequest;

                // figure out what kind of request this is
                var bActive = printRequest.m_ProcId == null;
                var bPrintExisting = !string.IsNullOrEmpty(printRequest.m_strReportName);

                // save presets; for active printing from preview, everything has already been saved
                if (!bPrintExisting || !bActive)
                {
                    var ret = SaveMwPresets(printRequest, bActive);
                    if (ret != HttpStatusCode.OK) return ret;
                }

                // reprint existing - same for active & submitted
                if (bPrintExisting)
                {
                    var procId = printRequest.m_ProcId?.ToProcId();
                    var printSettings = printRequest.m_Settings?.ToPrintSettings();
                    return PrintExistingReport(procId, printRequest.m_strReportName, printSettings);
                }

                // process printing request - active or submitted
                return bActive ? 
                    ProcessActivePrintRequest(printRequest) : 
                    ProcessSubmittedPrintRequest(printRequest);

            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0,
                    $"GenerateReportSubmitted failed for proc {printRequest?.m_ProcId} with err {ex.Message}", 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Generate and optionall print a report for submitted procedure
        /// </summary>
        private HttpStatusCode ProcessSubmittedPrintRequest(PrintReportGenerateInfo printRequest)
        {
            try
            {
                var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);
                using (var libProxy = _rm.GetLibProxy())
                {
                    if (libProxy == null)
                        return HttpStatusCode.ServiceUnavailable;

                    var procId = printRequest.m_ProcId.ToProcId();
                    var printSettings = printRequest.m_Settings.ToPrintSettings();
                    var images = printRequest.m_strImageList
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    // if I'm EasyView, get culture from call; else, use published one
                    var cultureName = _rm.MonsoonCfg == EMonsoonConfig.EasyView ? 
                        CultureInfo.CurrentCulture.Name : 
                        _rm.CultureName;

                    // generate report
                    libProxy.Proxy.PrintServ_RenderReportSubmitted(procId, printSettings, images, cultureName, accessInfo, out var jobId, out var _);

                    // start polling for 
                    var dtStartPoll = DateTime.UtcNow;
                    eStatus jobStatus = eStatus.unknown;
                    LibraryJobData data = null;
                    while ((DateTime.UtcNow - dtStartPoll).TotalSeconds < 90 &&
                           jobStatus == eStatus.unknown || jobStatus == eStatus.inprogress)
                    {
                        if (libProxy.Proxy.GetJobStatus(jobId, out jobStatus, out data) != 0)
                            throw  new Exception("GetJob failed");
                        if (jobStatus == eStatus.unknown)
                            System.Threading.Thread.Sleep(2000);
                    }

                    // have we just timed out?
                    if (jobStatus == eStatus.unknown || jobStatus == eStatus.inprogress)
                        throw  new Exception("Report generation has timed out at 90 seconds");

                    // failure?
                    if (jobStatus == eStatus.error)
                    {
                        IsmLog.LogEvent(EventLogEntryType.Error, 0, "Print generation failed", 1);
                        return HttpStatusCode.InternalServerError;
                    }

                    string jobMessage = data?.Message;

                    // set out param
                    _rm.LogEvent(EventLogEntryType.Information, 0, $"Report '{jobMessage}' generated",3);
                    printRequest.m_strReportName = jobMessage;

                    // if not printing - we're done!
                    if (!printRequest.m_bDoPrint)
                        return HttpStatusCode.OK;

                    // if asked to print  - do so now
                    return PrintExistingReport(procId, printRequest.m_strReportName, printSettings);
                }
            }
            catch (Exception e)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "ProcessSubmittedPrintRequest err: " + e.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }


        /// <summary>
        /// Process print request for active procedure - generate or print
        /// </summary>
        private HttpStatusCode ProcessActivePrintRequest(PrintReportGenerateInfo printRequest)
        {
            try
            {
                var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);
                var ret = SaveActiveImages(printRequest.m_strImageList);
                if (ret != HttpStatusCode.OK) return ret;
                using (var libProxy = _rm.GetLibProxy(true))
                {
                    if (libProxy == null)
                    {
                        _rm.LogEvent(EventLogEntryType.Warning, 0, "Lib proxy offline", 3);
                        return HttpStatusCode.ServiceUnavailable;
                    }

                    if (printRequest.m_bDoPrint)
                    {
                        if (libProxy.Proxy.PrintServ_PrintActive(accessInfo, out var err) != 0)
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0, "PrintServ_PrintActive err: " + err, 3);
                            return HttpStatusCode.InternalServerError;
                        }
                    }
                    else
                    {
                        if (libProxy.Proxy.PrintServ_RenderActive(accessInfo, out printRequest.m_strReportName, out var err) != 0)
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0,
                                $"PrintServ_RenderActive err failed for {printRequest.m_strReportName} with err {err}",
                                3);
                            return HttpStatusCode.InternalServerError;
                        }
                    }
                    _rm.LogEvent(EventLogEntryType.Information, 0, "ProcessActivePrintRequest succeeded", 3);
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception e)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "ProcessActivePrintRequest err: " + e.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Update which active images are to be printed
        /// </summary>
        /// <param name="strImagesToPrint">comma separated list of image names</param>
        private HttpStatusCode SaveActiveImages(string strImagesToPrint)
        {
            try
            {
                var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);
                // what images are there in procedure?
                var objProcImages = _rm.GetIssData(IssDataCodes.recorder_images);
                if (objProcImages == null) throw new Exception("Failed to retrieve recorder_images");
                var procImages = (List<ImageData>) objProcImages;

                // what are we askedto print?
                var images = strImagesToPrint.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

                // get items that are marked "to be printed" but are not in our "to print" list
                var ucheckImages = procImages.Where(img =>
                    img.m_ImgPrintState == EImgPrintState.ToBePrinted &&
                    !images.Contains(img.m_strPath, StringComparer.OrdinalIgnoreCase));
                var imgsSetNoPrint = ucheckImages.Select(img => img.m_strPath);

                using (var rxProxy = _rm.GetIsmRecProxy())
                {
                    if (rxProxy == null)
                        return HttpStatusCode.ServiceUnavailable;

                    // if ther are images un-checked, do so ow
                    var setNoPrint = imgsSetNoPrint as string[] ?? imgsSetNoPrint.ToArray();
                    string strErr;
                    if (setNoPrint.Any())
                    {
                        if (rxProxy.Proxy.Record_SetImagesPrintState(setNoPrint.ToArray(), EImgPrintState.DoNotPrint, accessInfo, out strErr) != 0)
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0, "Record_SetImagesPrintState err setting to no-print: " + strErr, 3);
                            return HttpStatusCode.InternalServerError;
                        }
                    }

                    // check requested
                    if (rxProxy.Proxy.Record_SetImagesPrintState(images, EImgPrintState.ToBePrinted, accessInfo, out strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "Record_SetImagesPrintState err setting to no-print: " + strErr, 3);
                        return HttpStatusCode.InternalServerError;
                    }
                }
                return HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "SaveActiveImages err: " + e.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Print existing report, either active or submitted
        /// </summary>
        private HttpStatusCode PrintExistingReport(ProcedureId procId, string report, PrintSettings printSettings)
        {
            try
            {
                var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext);
                // if no proc id, it's an active proc, get its id now
                var localLib = _rm.MonsoonCfg != EMonsoonConfig.Connected; 
                if (procId == null)
                {
                    localLib = true;
                    var objProcData = _rm.GetIssData(IssDataCodes.recorder_proc_info);
                    if (objProcData == null) throw new Exception("Failed to get active proc data from SS");
                    procId = ((ProcedureData) objProcData).m_Id;
                }

                // if report was generated on remote but printing will be local - special handling
                //  essentially, this will send command to LOCAL library, but tell it to download report from remote
                if (!localLib)
                {
                    var libSettings = _settingsMgr.GetLibSettings() ?? throw new Exception("Failed to retrieve lib settings");
                    if (libSettings.TryGetValue(ESettings.remote_printing, out var val) && !bool.Parse(val))
                    {
                        procId.m_strVolName = "REMOTE";
                        localLib = true;
                    }
                }

                PrintWorkItemAccessDetailsInfo patInfo = null;
                try
                {
                    using (var proxy = _rm.GetLibProxy())
                    {
                        if (proxy.Proxy.GetDmag(procId, accessInfo, out var dmagJson, out var err) != 0)
                            throw new Exception(err);

                        var dmagData = Newtonsoft.Json.JsonConvert.DeserializeObject<clsIsmDmag>(dmagJson);
                        var dmag = new DMAGFileMgr(dmagData);

                        //we currently do not "know" if existing reports contain PHI. To air on the side of caution, if the patient ino setting is not never, log that we did print PHI
                        //looking at it a different way, the images themselves could be seen as PHI even without the patient name on the report
                        patInfo = new PrintWorkItemAccessDetailsInfo(dmag.IsClinical, dmag.MrnOrTitle, dmag.LastNameOrDescription, printSettings.m_PatDataOptions != EPatientDataOptions.Never);
                    }
                }
                catch (Exception ex)
                {
                    //we want to log an error if we will not be able to properly access log
                    _rm.LogEvent(EventLogEntryType.Error, 0, $"Failed to GetDmag while printing an existing procedure. Access log will be incomplete. Error: {ex}", 1);
                    patInfo = new PrintWorkItemAccessDetailsInfo(true, procId.ToString(), report, printSettings.m_PatDataOptions != EPatientDataOptions.Never); //log procid and report name, it's better than nothing
                }

                // tell library to print
                using (var libProxy = _rm.GetLibProxy(localLib))
                {
                    if (libProxy.Proxy.PrintServ_PrintReport(procId, report, printSettings, patInfo, accessInfo, out string err) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0,
                            $"PrintExistingReport: PrintServ_PrintReport for {report} err {err}", 3);
                        return HttpStatusCode.InternalServerError;
                    }
                    _rm.LogEvent(EventLogEntryType.Information, 0, $"PrintServ_PrintReport for {report} succeeded", 3);
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception e)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, $"PrintMgr.PrintExistingReport for {report} err: {e.Message}",
                    3);
                return HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Save the provided print settings to presets, either active or based on user
        /// </summary>
        private HttpStatusCode SaveMwPresets(PrintReportGenerateInfo printRequest, bool bActive)
        {
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                        return HttpStatusCode.ServiceUnavailable;

                    if (bActive)
                    {
                        if (mwProxy.Proxy.Presets_SaveActivePrintPresets(printRequest.m_Settings.ToPrintSettings(),
                                out string strErr) != 0)
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0, "Presets_SaveActivePrintPresets err: " + strErr,
                                3);
                            return HttpStatusCode.InternalServerError;
                        }
                    }
                    else if (!string.IsNullOrEmpty(printRequest.m_strUser))
                    {
                        if (mwProxy.Proxy.Presets_SavePrintPresets(printRequest.m_strUser,
                                printRequest.m_Settings.ToPrintSettings(), out string _) != 0)
                            return HttpStatusCode.InternalServerError;
                    }
                }
                return HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "PrintMgr.SaveMwPreses err: " + e.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }
    }
}