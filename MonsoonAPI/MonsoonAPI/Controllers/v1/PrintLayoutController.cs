using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ISM.Library.Types;
using MonsoonAPI.models;
using MsntSi.Types;

namespace MonsoonAPI.Controllers.v1
{
    public class PrintLayoutController : ControllerBase
    {
        public PrintLayoutController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IConfiguration cfg) 
            : base("PrintLayout", resMgr, nodeMgr, cfg)
        { }

        /// <summary>
        /// Get all layouts available for the default orientation for this room/login
        /// </summary>
        [HttpGet("{room}/{login}")]
        public async Task<IActionResult> GetPrintLayouts(string room, string login)
        {
            var method = $"GetPrintLayouts/{room}/{login}";
            LogEnter(method);
            return await DoGetPrintLayouts(method, room, login);
        }

        /// <summary>
        /// Get all layouts available for the default orientation for this room/login
        /// </summary>
        [HttpGet("{room}/{login}/{orientation}")]
        public async Task<IActionResult> GetPrintLayouts(string room, string login, string orientation)
        {
            var method = $"GetPrintLayouts/{room}/{login}/{orientation}";
            LogEnter(method);
            return await DoGetPrintLayouts(method, room, login);
        }

        /// <summary>
        /// Get all layouts available for the default orientation for this room/login
        /// </summary>
        [HttpGet("{orientation}")]
        public async Task<IActionResult> GetPrintLayouts(string orientation)
        {
            var method = $"GetPrintLayouts/{orientation}";
            try
            {
                LogEnter(method);
                LayoutsInfo layoutInfo = DoGetPrintLayouts(orientation);
                if (layoutInfo == null)
                    return await ReturnError(method, "Failed to get print layouts");
                return await ReturnOk(method, layoutInfo);
            }
            catch (Exception e)
            {
                return await ReturnError(method, e.Message);
            }

        }

        private async Task<IActionResult> DoGetPrintLayouts(string method, string room, string login)
        {
            try
            {
                // create the stack
                Stack<string> ownerStack = new Stack<string>();
                ownerStack.Push("SYSTEM");
                if (!string.IsNullOrEmpty(room))
                    ownerStack.Push(room);
                if (!string.IsNullOrEmpty(login) && !string.Equals(login, "null", StringComparison.OrdinalIgnoreCase))
                    ownerStack.Push(login);

                // call mw to figure out the orientation
                Dictionary<string, string> presetVals =
                    new Dictionary<string, string>
                    {
                        ["PrintOrientation"] = string.Empty,
                        ["PrintLayout"] = string.Empty
                    };
                using (var mwProxy = Rm.GetMwProxy())
                {
                    if (mwProxy == null )
                        return await ReturnOffline(eSystemType.middleware, method);

                    string strErr;
                    if (mwProxy.Proxy.Presets_GetValues(ownerStack, ref presetVals, out strErr) != 0)
                        return await ReturnError(method, "Presets_GetValues failed with err " + strErr);
                }


                // put together output
                LayoutsInfo layoutInfo = DoGetPrintLayouts(presetVals["PrintOrientation"]);
                if (layoutInfo == null)
                    return await ReturnError(method, "DGetPrintLayouts failed");

                layoutInfo.m_strDefaultLayout = presetVals["PrintLayout"];
                return await ReturnOk(method, new { layoutInfo});

            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        private LayoutsInfo DoGetPrintLayouts(string strPrintOrientaiton)
        {
            try
            {
                EPrintOritentation printOrientation;
                if (!Enum.TryParse(strPrintOrientaiton, out printOrientation))
                {
                    Rm.LogEvent(EventLogEntryType.Error, 0, $"{strPrintOrientaiton} is not a valid EPrintOritentation", 3);
                    return null;
                }

                // get print layouts from State Server (cached)
                List<PrintLayoutInfo> allLayouts = Rm.PrintLayouts;
                if (allLayouts == null)
                {
                    Rm.LogEvent(EventLogEntryType.Error, 0, "Layout information is not available",3);
                    return null;
                }

                IEnumerable<PrintLayoutInfo> layouts = allLayouts.Where(layout => layout.m_Orientation == printOrientation);

                // put together output
                LayoutsInfo layoutInfo = new LayoutsInfo
                {
                    m_strOrientation = printOrientation.ToString(),
                    m_Layouts = layouts.Select(layout => layout.m_strLayout).ToList()
                };
                return layoutInfo;
            }
            catch (Exception ex)
            {
                Rm.LogEvent(EventLogEntryType.Error, 0, $"DoGetPrintLayouts failed for {strPrintOrientaiton} with err {ex.Message}", 3);
                return null;
            }
        }
    }
}
 