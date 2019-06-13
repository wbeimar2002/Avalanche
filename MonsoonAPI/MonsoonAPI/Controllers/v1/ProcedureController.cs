using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using IsmLogCommon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using IsmUtility;
using ISM.Middleware2Si;
using MonsoonAPI.models;
using IsmStateServer.Types;
using PatInfoEngine.Types;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MonsoonAPI.Controllers.v1
{
    /// <summary>
    /// This controller represents a procedure, either active or not
    /// </summary>
    public class ProcedureController : ControllerBase
    {
        private readonly ILibMgr _libMgr;
        private readonly IActiveMgr _activeMgr;

        public ProcedureController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, ILibMgr libMgr, IMwMgr mwMgr, 
            IActiveMgr activeMgr, IConfiguration cfg) : 
            base("Procedure", resMgr,  nodeMgr, cfg, activeMgr)
        {
            _libMgr = libMgr;

            _activeMgr = activeMgr;
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePatient([FromBody] ClinInfoDataExM clinInfo)
        {
            //TODO for application and access logging, it would be useful to know "why" this patient was updated
            const string method = "UpdatePatient";
            try
            {
                LogEnter(method);
                if (clinInfo.m_ClinInfo == null)
                {
                    return await ReturnBadRequest(method, "UpdatePatient failed because of bad input");
                }

                // deal with middleware first
                GetAndSetMwData(clinInfo.m_strPhysician, clinInfo.m_ClinInfo.m_strProcType, clinInfo.m_ClinInfo.m_strScheduleId, CultureInfo.CurrentCulture, out var physician);

                clinInfo.OriginatorIp = HttpContextUtilities.With(HttpContext).GetRequestIP();

                // are we dealing with an active or library procedure?
                var res = clinInfo.m_ProcId?.m_strLibId == null
                    ? _activeMgr.UpdateActiveProcedure(clinInfo, physician)
                    : _libMgr.UpdateLibraryPatient(clinInfo, physician);

                return await ReturnOk(method, res);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }

        }

        [HttpPost]
        public async Task<IActionResult> SaveScreenshot([FromBody] ScreenshotInfo screenshotInfo)
        {
            const string method = "SaveScreenshot";
            try
            {
                LogEnter(method);
                byte[] img = null;
                if (string.IsNullOrEmpty(screenshotInfo.file_name))
                {
                    if (string.IsNullOrEmpty(screenshotInfo.base64_image))
                    {
                        return await ReturnBadRequest(method, "DoSaveScreenshot failed because some input params are null");
                    }


                    // get body
                    img = Convert.FromBase64String(screenshotInfo.base64_image);

                    // FB 17116, MM, 5/3/2016 verify image is not all-black
                    if (!IsValidImage(img))
                    {
                        IsmLog.LogEvent(EventLogEntryType.Warning, 0, "Failed to save screenshot because all black image has been passed in",3);
                        return await ReturnOk(method, HttpStatusCode.NotAcceptable);
                    }
                }

                screenshotInfo.file_name = screenshotInfo.file_name ??
                                           $"Screenshot_{DateTime.UtcNow:yyyy_MM_ddTHH_mm_ss_fff}.jpg";
                var res = !string.IsNullOrEmpty(screenshotInfo.proc_id?.m_strLibId) ? 
                    _libMgr.SaveLibraryScreenshot(screenshotInfo, img) :
                    _activeMgr.SaveRecorderScreenshot(screenshotInfo, img);

                return await ReturnOk(method, res);

            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }

        }
        
      
        private void GetAndSetMwData(string strPhysicianLogin, string strProcType, string strScheduleId, CultureInfo searchCulture, out PersonNameData physician)
        {
            physician = null;
            try
            {
                using (var mwProxy = Rm.GetMwProxy())
                {
                    if (mwProxy == null )
                    {
                        Rm.LogEvent(EventLogEntryType.Warning, 0, "GetAndSetMwData failure because IMiddleware2Si is offline", 3);
                        return;
                    }

                    var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(HttpContext);

                    // physician
                    string strErr;
                    if (!string.IsNullOrEmpty(strPhysicianLogin) && !strPhysicianLogin.Equals(MwUtils.VISIT_LOGIN))
                    {
                        if (mwProxy.Proxy.Users_GetXml(strPhysicianLogin, MwUtils.SystemCredentials, accessInfo, out var xml, out strErr) == 0)
                            physician = new PersonNameData(IsmXmlDoc.FromXml(xml).DocumentElement);
                    }
                    else if (strPhysicianLogin != null && (strPhysicianLogin.Equals(MwUtils.VISIT_LOGIN) && !string.IsNullOrEmpty(strScheduleId)))
                    {
                        using (var pieProxy = Rm.GetPieProxy())
                        {
                            if (pieProxy == null) // it's OK to go on here even 
                            {
                                Rm.LogEvent(EventLogEntryType.Warning, 0, "GetAndSetMwData failure because PIE is offline", 3);
                                return;
                            }

                            var pieSearchFields =
                                new Dictionary<EPieFields, string> {[EPieFields.ScheduleId] = strScheduleId};
                            if (pieProxy.Proxy.PatList_Search(pieSearchFields, 0, 1, searchCulture.Name, accessInfo, out _, out strErr) != 0)
                            {
                                Rm.LogEvent(EventLogEntryType.Error, 0, "PatList_Search failed with err " + strErr,3);
                                return;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(strPhysicianLogin) && !strPhysicianLogin.Equals(MwUtils.VISIT_LOGIN))
                    {
                        if (mwProxy.Proxy.Users_GetXml(strPhysicianLogin, MwUtils.SystemCredentials, accessInfo, out var xml, out strErr) == 0)
                            physician = new PersonNameData(IsmXmlDoc.FromXml(xml).DocumentElement);
                        else
                            Rm.LogEvent(EventLogEntryType.Warning, 0, $"UpdatePatient ignoring physician because {strPhysicianLogin} is not a valid user", 3);
                    }

                    // proc type
                    if (!string.IsNullOrEmpty(strProcType))
                    {
                        mwProxy.Proxy.Department_CreateUserProcType(strPhysicianLogin, strProcType, out strErr);
                    }
                }
                
            }
            catch (Exception ex)
            {
                Rm.LogEvent(EventLogEntryType.Error, 0, "GetAndSetMwData err: " + ex.Message, 3);
            }
        }


        /// <summary>
        /// FB 17116, MM, 5/3/2016 verify iamge is not all-black
        /// </summary>
        /// <returns>false if image is all-black, true otherwise</returns>
        private static readonly int KBlack = Color.Black.ToArgb();
        private bool IsValidImage(byte[] imgStream)
        {
            using (MemoryStream msImg = new MemoryStream(imgStream))
            {
                using (Image imgFull = Image.FromStream(msImg))
                {
                    using (Bitmap imgThumb = (Bitmap)imgFull.GetThumbnailImage(240, 200, null, IntPtr.Zero))
                    {
                        for (int x = 0; x < imgThumb.Width; x++)
                        {
                            for (int y = 0; y < imgThumb.Height; y++)
                            {
                                if (imgThumb.GetPixel(x, y).ToArgb() != KBlack)
                                    return true;
                            }
                        }
                    }
                }
            }
            // haven't found 1 non-black pixel, so returning false
            return false;
        }

    }
}