using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using IsmRec.Types;
using IsmStateServer;
using MsntSi.Types;

namespace MonsoonAPI.Controllers.v1
{
    public class ProcActiveImgController : ControllerBase
    {
        public ProcActiveImgController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IConfiguration cfg) :
            base("ProcActive", resMgr,  nodeMgr, cfg)
        {
          
        }

        [HttpGet]
        public async Task<IActionResult> GetImages()
        {
            const string method = "GetImages";
            try
            {
                LogEnter(method);
                if (Rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    return await ReturnBadRequest(method, "ProcActiveImg not supported on EasyView");
                }

                IssDataCodes[] codesToGet = {
                        IssDataCodes.recorder_images,
                        IssDataCodes.recorder_proc_info
                     };
                if (Rm.GetIssDataMap(codesToGet, out var dataValues) !=  ESuccess.Ok || dataValues == null)
                    return await ReturnError(method,"GetIssDataMap failed.");

                //images
                List<ImageData> images = null;
                if (dataValues.TryGetValue(IssDataCodes.recorder_images, out var obj) && obj != null)
                    images = (List<ImageData>)obj;

                // procId
                ProcedureData procData = null;
                if (dataValues.TryGetValue(IssDataCodes.recorder_proc_info, out obj) && obj != null)
                    procData = (ProcedureData)obj;

                // put together the list & return
                List<models.DmagM.DmagPictureM> pics = models.DmagM.GetPictures(images);

                return await ReturnOk(method, new
                {
                    images = pics ?? new List<models.DmagM.DmagPictureM>(),
                    proc_id = procData?.m_Id
                });
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteImg([FromBody]string image)
        {
            var method = $"DeleteImg - {image}";
            try
            {
                var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(HttpContext);
                LogEnter(method);
                if (Rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    return await ReturnBadRequest(method, "ProcActiveImg not supported on EasyView");
                }

                using (var recProxy = Rm.GetIsmRecProxy())
                {
                    if (recProxy == null)
                        return await ReturnOffline(eSystemType.recorder, method);
                    if (recProxy.Proxy.Record_DeleteImage(image, accessInfo, out var strErr) != 0)
                        return await ReturnError(method,"Record_DeleteImage err: " + strErr);

                    return await ReturnOk( method);
                }

            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
  
        }

        [HttpPost]
        public async Task<IActionResult> CaptureImage()
        {
            const string method = "CaptureImage";
            try
            {
                var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(HttpContext);
                LogEnter(method);
                if (Rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    return await ReturnBadRequest(method, "ProcActiveImg not supported on EasyView");
                }

                using (var recProxy = Rm.GetIsmRecProxy())
                {
                    if (recProxy == null )
                        return await ReturnOffline(eSystemType.recorder, method);

                    if (recProxy.Proxy.Record_CaptureImage(accessInfo, out var strErr) != 0)
                        return await ReturnError(method,"Record_CaptureImage err: " + strErr);

                    return await ReturnOk( method);
                }

            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
  
        }
    }
}