using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ISM.Middleware2Si;
using MsntSi.Types;
using MonsoonAPI.Models;

namespace MonsoonAPI.Controllers.v1
{
    public class ProcTypesController : ControllerBase
    {
        private readonly IMwMgr _mwMgr;

        public ProcTypesController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IMwMgr mwMgr, IConfiguration cfg) :
            base("ProcTypes", resMgr, nodeMgr, cfg)
        {
            _mwMgr = mwMgr;
        }

        [HttpGet]
        public async Task<IActionResult> GetProcTypes()
        {
            const string method = "GetProcTypes";
            LogEnter(method);
            return await DoGetProcTypes(method, string.Empty);
        }
        [HttpGet("{login}")]
        public async Task<IActionResult> GetProcTypes(string login)
        {
            var method = $"GetProcTypes/{login}";
            LogEnter(method);
            return await DoGetProcTypes(method, login);
        }

        [HttpPost("DeleteProcType")]
        public async Task<IActionResult> DeleteProcType([FromBody] ProcTypeInfoM procType)
        {
            var method = $"DeleteProcType";
            try
            {
                LogEnter(method);

                string proc = procType?.Name;
                string department = procType?.Department;

                if (string.IsNullOrEmpty(proc))
                {
                    throw new Exception("DeleteProcType: No procedure type specified");
                }

                if (string.IsNullOrEmpty(department))
                {
                    department = "*";
                }

                using (var mwProxy = Rm.GetMwProxy())
                {
                    if (mwProxy == null)
                        return await ReturnOffline(eSystemType.middleware, method);

                    // call mw
                    string strErr;
                    if (mwProxy.Proxy.Department_DeleteProcType(department, proc, out strErr) != 0)
                        return await ReturnError(method, "Department_DeleteProcType err: " + strErr);

                    return await ReturnOk(method);
                }
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPost("AddProcType")]
        public async Task<IActionResult> AddProcType([FromBody] ProcTypeInfoM procType)
        {
            var method = $"AddProcType";
            try
            {
                LogEnter(method);
                HttpStatusCode ret = _mwMgr.AddDepartmentProcType(procType.Name, procType.Department);
                return await ReturnCode(ret, null, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        private async Task<IActionResult> DoGetProcTypes(string method, string strLogin)
        {
            try
            {
                if (string.IsNullOrEmpty(strLogin))
                    strLogin = MwUtils.ADMIN_LOGIN; // if user not specified, get everything

                List<string> procTypes;
                using (var mwProxy = Rm.GetMwProxy())
                {
                    if (mwProxy == null )
                        return await ReturnOffline(eSystemType.middleware, method);

                    string strErr;
                    if (mwProxy.Proxy.Department_GetUserProcTypes(strLogin, out procTypes, out strErr) != 0)
                        return await ReturnError(method,"DoGetProcTypes failed. Department_GetUserProcTypes error: " + strErr);
                }

                // FB16247 - remove *
                procTypes.Remove("*");

                // return
                Rm.LogEvent(EventLogEntryType.Information, 0, "#procs returned: " + procTypes.Count, 3);
                return await ReturnOk(method, new { procTypes });
            }
            catch (Exception ex)
            {
                return  await ReturnError(method, ex.Message);
            }
        }

    }
}