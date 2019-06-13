using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MsntSi.Types;

namespace MonsoonAPI.Controllers.v1
{
    public class MaintenanceController : ControllerBase
    {
        public MaintenanceController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IConfiguration cfg) :
            base("Maintenance", resMgr,  nodeMgr, cfg)
        { }

        [HttpPost("item")]
        public async Task<IActionResult> Sync(string item)
        {
            var method = $"Sync/{item}";
            try
            {
                LogEnter(method);
                switch (item)
                {
                    case "middleware":
                        using (var mwProxy = Rm.GetMwProxy())
                        {
                            if (mwProxy == null)
                                return await ReturnOffline(eSystemType.middleware, method);

                            var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(HttpContext);

                            if (mwProxy.Proxy.Sync(ISM.Middleware2Si.MwUtils.SystemCredentials, accessInfo) == 0)
                            {
                                Rm.LogEvent(EventLogEntryType.Information, 0, "Synced successfully", 3);
                                return await ReturnOk(method);
                            }

                            return await ReturnError(method, "Failed to sync");
                        }
                    default:
                        return await ReturnBadRequest("Unknown item " + item);
                }
            }
            catch (Exception e)
            {
                return await ReturnError(method, e.Message);
            }

        }
    }
}