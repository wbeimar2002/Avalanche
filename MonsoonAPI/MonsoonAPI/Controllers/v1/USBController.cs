using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using IsmStateServer;
using IsmRec.Types;
using MonsoonAPI.models;
using MsntSi.Types;
using System.Text;

namespace MonsoonAPI.Controllers.v1
{
    public class UsbController : ControllerBase
    {
        public UsbController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IConfiguration cfg) : 
            base("Usb", resMgr, nodeMgr, cfg)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetUsbStatus()
        {
            const string method = "GetUsbStatus";
            try
            {
                LogEnter(method);
                object objUsbState = Rm.GetIssData(IssDataCodes.recorder_usb_drive_state);
                if (objUsbState == null)
                    return await ReturnError(method, "Failed to retrieve duration from state server");

                EUsbState usbState = ((KeyValuePair<string, EUsbState>)objUsbState).Value;
                return await ReturnOk(method, usbState == EUsbState.Connected);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }           
        }

        [HttpPost]
        public async Task<IActionResult> ExportToUsb([FromBody]UsbExportInfo usbExp)
        {
            const string method = "ExportToUsb";
            try
            {
                LogEnter(method);
                // is this local or not?
                bool local = Rm.MonsoonCfg != EMonsoonConfig.Connected;

                // save preset
                using (var mw = Rm.GetMwProxy())
                {
                    if (mw == null)
                        return await ReturnOffline(eSystemType.middleware, method);
                    if (mw.Proxy.Presets_SetPresetValue(usbExp.Login, "IncludePatientData", usbExp.IncludePhi.ToString(), out var err) != 0)
                        return await ReturnError(method, "Presets_SetPresetValue failed with err " + err);
                }

                // go export!
                using (var lib = Rm.GetLibProxy(true)) // LOCAL library is responsible for this
                {
                    var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(HttpContext);
                    if (lib == null)
                        return await ReturnOffline(eSystemType.library, method);
                    if (lib.Proxy.ExportToUsb(usbExp.ProcedureId.ToProcId(), local, accessInfo, usbExp.IncludePhi, out var err) != 0)
                        return await ReturnError(method, "ExportToUsb failed with err " + err);
                }

                return await ReturnOk( method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPost("unlockBitlocker")]
        public async Task<IActionResult> UnlockBitlocker([FromBody]string base64Password)
        {
            const string method = nameof(UnlockBitlocker);
            try
            {
                LogEnter(method);

                using (var lib = Rm.GetLibProxy(true)) // true = local
                {
                    if (lib == null)
                    {
                        return await ReturnOffline(eSystemType.library, method);
                    }

                    string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64Password));
                    if (lib.Proxy.UnlockBitlockedUsb(decoded, out string err) != 0)
                    {
                        return await ReturnError(method, $"UnlockBitlockedUsb failed with error: {err}");
                    }

                    return await ReturnOk(method);
                }
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> CancelUsbExport()
        {
            const string method = "CancelUsbExport";
            try
            {
                LogEnter(method);
                using (var lib = Rm.GetLibProxy(true)) // LOCAL library is responsible for this
                {
                    if (lib.Proxy.CancelUsbExport() != 0)
                        return await ReturnError(method, "USBController.CancelUSBExport failed");
                }

                return await ReturnOk( method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }
    }
}