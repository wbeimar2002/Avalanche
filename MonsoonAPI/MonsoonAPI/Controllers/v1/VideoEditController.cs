using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ISM.LibrarySi;
using MonsoonAPI.models;
using MsntSi.Types;
using ISM.Library.Types;
using IsmRec.Types;

namespace MonsoonAPI.Controllers.v1
{
    public class VideoEditController : ControllerBase
    {

        public VideoEditController(IMonsoonResMgr rm, 
            INodeMgr nodeMgr, IConfiguration cfg) :
            base("VideoEdit", rm,  nodeMgr, cfg)
        {
        }


        [HttpPost]
        public async Task<IActionResult> SaveEdits([FromBody]ProcMovieInfoM pmInfo)
        {
            var method = $"VideoEdit.SaveEdits: {pmInfo?.ProcId}";
            try
            {
                LogEnter(method);
                if (pmInfo?.ProcId == null)
                {
                    return await ReturnBadRequest(method, "null ProcMovieInfoM.ProcId passed in");
                }

                var procId = pmInfo.ProcId?.ToProcId();
                var pms = pmInfo.TranslateProcMovies();
                if (procId == null || pms == null)
                    return await ReturnBadRequest(method);

                // actually save the edits
                using (var libProxy = Rm.GetLibProxy())
                {
                    // get DMAG
                    if (libProxy == null)
                        return await ReturnOffline(eSystemType.library, method);

                    var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(HttpContext);

                    eRecording_Mode recMode = ProcMovieInfoM.ToRecMode(pmInfo.generate_clips_or_movies);
                    var libRes = libProxy.Proxy.SaveEdits(procId, pms, recMode, accessInfo);
                    if (libRes != eLibErr.OK || !pmInfo.EditNow)
                        return await ReturnOk(method, libRes);

                    var wi = new LibWorkItemNonPrint(ePayloadType.autoedit_task, procId, true, accessInfo);
                    if (libProxy.Proxy.QueueTask(wi, out var strErr) != 0)
                        return await ReturnError(method, "QueueTask err - " + strErr);
                }
                return await ReturnOk(method, eLibErr.OK);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }
    }
}
 