using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using ISM.Library.Types;
using MsntSi.Types;

namespace MonsoonAPI.Controllers.v1
{
    public class PrintImgTemplateController : ControllerBase
    {
        public PrintImgTemplateController(IMonsoonResMgr rm, INodeMgr nodeMgr, IConfiguration cfg) : 
            base("PrintImgTemplate", rm,  nodeMgr, cfg)
        { }

        [HttpGet("{orientation}/{layout}")]
        public async Task<IActionResult> GetPrintTemplateImg(string orientation, string layout)
        {
            var method = $"GetPrintTemplateImg/{orientation}/{layout}";
            try
            {
                LogEnter(method);

                // create layout info
                PrintLayoutInfo layoutInfo = new PrintLayoutInfo();
                if (!Enum.TryParse(orientation, true, out layoutInfo.m_Orientation))
                    return await ReturnBadRequest(method, "Orientation does not parse");


                layoutInfo.m_strLayout = layout;

                // get thumb from lib
                byte[] thumb;
                using (var libProxy = Rm.GetLibProxy(true)) // no need to make round trip to remote for this one
                {
                    if (libProxy == null)
                        return await ReturnOffline(eSystemType.library, method);

                    if (libProxy.Proxy.PrintServ_GetTemplateImage(layoutInfo, out thumb, out var strErr) != 0)
                        return await ReturnError(method, "PrintServ_GetTemplateImage err: " + strErr);
                }

                // to base 64 * return
                string strThumb = Convert.ToBase64String(thumb);
                return await ReturnOk(method, strThumb);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }
    }
}