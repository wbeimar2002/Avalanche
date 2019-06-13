using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using IsmStateServer.Types;
using ISM.Library.Types;
using MonsoonAPI.models;
using ISM.LibrarySi;

namespace MonsoonAPI.Controllers.v1
{
    public class ImageZipController : ControllerBase
    {
        public ImageZipController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IConfiguration cfg) : 
            base("ImageZip", resMgr, nodeMgr, cfg)
        { }

        [HttpPost]
        public async Task<IActionResult> ZipImages([FromBody]WorkItemExportM zipInfo)
        {
            var method = $"ZipImages:{zipInfo.m_ProcId?.m_strLibId}";
            try
            {
                LogEnter(method);
                if (zipInfo.m_ItemsToExport == null || string.IsNullOrEmpty(zipInfo.m_ProcId?.m_strLibId))
                    return await ReturnBadRequest(method);


                string zipfile;
                using (var libProxy = Rm.GetLibProxy())
                {
                    if (libProxy == null )
                        return await ReturnOffline(MsntSi.Types.eSystemType.library, method);

                    IEnumerable<string> imgsToExport = zipInfo.m_ItemsToExport.Where(img => img != null);
                    imgsToExport = imgsToExport.Select(img => img.Trim());
                    imgsToExport = imgsToExport.Where(img => !string.IsNullOrEmpty(img));

                    var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(HttpContext);

                    if (libProxy.Proxy.ZipImages(zipInfo.m_ProcId.ToProcId(), imgsToExport.ToList(),
                        accessInfo, out var jobId, out var strErr) != 0)
                        return await ReturnError(method, "ZipImages failed with err " + strErr);

                    // start polling for 
                    var dtStartPoll = DateTime.UtcNow;
                    eStatus jobStatus = eStatus.unknown;
                    LibraryJobData data = null;
                    while ((DateTime.UtcNow - dtStartPoll).TotalSeconds < 90 && 
                        jobStatus == eStatus.unknown || jobStatus == eStatus.inprogress)
                    {
                        if (libProxy.Proxy.GetJobStatus(jobId, out jobStatus, out data) != 0)
                            throw new Exception("GetJob failed");
                        if (jobStatus == eStatus.unknown)
                            System.Threading.Thread.Sleep(2000);
                    }

                    // have we just timed out?
                    if (jobStatus == eStatus.unknown || jobStatus == eStatus.inprogress)
                        throw new Exception("Zipping has timed out at 90 seconds");
                    zipfile = data?.Message;
                }

                Rm.LogEvent(EventLogEntryType.Information, 0, "ZipImages returning file " + zipfile, 3);
                return await ReturnOk(method, zipfile);
            }
            catch (Exception ex)
            {
                return await ReturnError(method,  ex.Message);
            }

        }

        [HttpDelete("{libName}/{libId}/{zipFile}")]
        public async Task<IActionResult> DeleteImages(string libName, string libId, string zipFile)
        {
            var method = $"DeleteImages/{libName}/{libId}/{zipFile}";
            try
            {
                LogEnter(method);
                ProcedureId procId = new ProcedureId(libId, libName);

                using (var libProxy = Rm.GetLibProxy())
                {
                    if (libProxy == null )
                        return await ReturnOffline(MsntSi.Types.eSystemType.library, method);

                    if (libProxy.Proxy.DeleteProcFile(procId, zipFile, out var strErr) != 0)
                        return await ReturnError(method,  strErr);
                }

                Rm.LogEvent(EventLogEntryType.Information, 0, $"Successfully deleted {zipFile} from {procId} ", 3);
                return await ReturnOk(method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
         
        }
    }
}
 