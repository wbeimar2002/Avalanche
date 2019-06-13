using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ISM.Library.Types;
using MsntSi.Types;
using MonsoonAPI.models;

namespace MonsoonAPI.Controllers.v1
{
    public class PrintController : ControllerBase
    {
        private readonly IPrintMgr _printMgr;

        public PrintController(IMonsoonResMgr rm, INodeMgr nodeMgr, IPrintMgr printMgr, ISettingsMgr settingsMgr,IConfiguration cfg) : 
            base("Print", rm,  nodeMgr, cfg)
        {
            settingsMgr.Init(rm);
            _printMgr = printMgr;
            printMgr.Init(rm, settingsMgr);
        }

        [HttpGet("{room}/{login}")]
        public async Task<IActionResult> GetPrintSettings(string room, string login)
        {
            var method = $"GetPrintSettings/{room}/{login}";
            try
            {
                LogEnter(method);

                // create user stack
                var userStack = ResMgr.GetOwnerStack(room, login);
               
                // retrieve
                PrintSettings printSettings;
                using (var mwProxy = Rm.GetMwProxy())
                {
                    if (mwProxy == null )
                        return await ReturnOffline(eSystemType.middleware, method);

                    if (mwProxy.Proxy.Presets_GetPrintPresets(userStack, out printSettings, out var strErr) != 0)
                    {
                        return await ReturnError(method, "Presets_GetPrintPresets err: " + strErr);
                    }
                }

                // return
                return await ReturnOk(method, printSettings);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePrintReport([FromBody]PrintReportGenerateInfo printRequest)
        {
            const string method = "GeneratePrintReport";
            try
            {
                LogEnter(method);
                HttpStatusCode res = _printMgr.GeneratePrintReport(printRequest);
                object objRet = null;
                if (res == HttpStatusCode.OK)
                    objRet = new {report_path = Path.GetFileName(printRequest.m_strReportName)};
                return await ReturnCode(res, objRet, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }
        
    }
}