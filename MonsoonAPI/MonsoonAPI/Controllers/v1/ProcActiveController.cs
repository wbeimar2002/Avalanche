using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MsntSi.Types;
using MonsoonAPI.models;
using MonsoonAPI.Models;

namespace MonsoonAPI.Controllers.v1
{
    public class ProcActiveController : ControllerBase
    {
        private readonly IActiveMgr _activeMgr;

        public ProcActiveController(IMonsoonResMgr resMgr, IActiveMgr activeMgr, INodeMgr nodeMgr, 
            IMwMgr mwMgr, IConfiguration cfg) :
            base("ProcActive", resMgr,nodeMgr, cfg, activeMgr)
        {
            _activeMgr = activeMgr;
        }

        [HttpGet]
        public async Task<IActionResult> GetProcInfo()
        {
            const string method = "GetProcInfo";
            try
            {
                LogEnter(method);
                HttpStatusCode retCode = _activeMgr.GetProcInfo(out DmagM procInfo, CultureInfo.CurrentCulture);
                return await ReturnCode(retCode, procInfo, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> AcknowledgeProc()
        {
            const string method = "AcknowledgeProc";
            try
            {
                LogEnter(method);
                if (Rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    return await ReturnBadRequest(method,"ProcActive not supported on EasyView. AcknowledgeProc fails");
                }

                using (var ismRecProxy = Rm.GetIsmRecProxy())
                {
                    if (ismRecProxy == null)
                        return await ReturnOffline(eSystemType.recorder, method);

                    if (ismRecProxy.Proxy.Record_Acknowledge(out var strErr) != 0)
                        return await ReturnError(method, "Record_Acknowledge err: " + strErr);
                }
                return await ReturnOk( method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }

        }

        [HttpPost]
        public async Task<IActionResult> RegisterProcedure([FromBody] ClinInfoDataExM clinInfo)
        {
            const string method = "RegisterProcedure";
            try
            {
                LogEnter(method);

                clinInfo.OriginatorIp = HttpContextUtilities.With(HttpContext).GetRequestIP();

                HttpStatusCode retCode = _activeMgr.RegisterProc(clinInfo, CultureInfo.CurrentCulture);
                return await ReturnCode(retCode, null, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        // Not currently supported...
        //[HttpPost("setAutolabelOverrides")]
        //public async Task<IActionResult> SetAutolabelOverrides([FromBody] AutolabelOverrideInfo data)
        //{
        //    const string method = "SetAutolabelOverrides";
        //    try
        //    {
        //        LogEnter(method);

        //        HttpStatusCode retCode = _activeMgr.UpdateAutolabelOverrides(data?.ProcTypeLabelOverrides, data?.CommonLabelOverrides);
        //        return await ReturnCode(retCode, null, method);
        //    }
        //    catch (Exception ex)
        //    {
        //        return await ReturnError(method, ex.Message);
        //    }
        //}

        [HttpDelete]
        public async Task<IActionResult> FinishProcedure([FromBody] Dictionary<string, bool> finishOptions)
        {
            const string method = "FinishProcedure";
            try
            {
                LogEnter(method);
                HttpStatusCode retCode = _activeMgr.FinishProc(finishOptions);
                return await ReturnCode(retCode, null, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }
    }
}