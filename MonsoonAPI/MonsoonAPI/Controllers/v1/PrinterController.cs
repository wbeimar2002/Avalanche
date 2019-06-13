using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using IsmStateServer;

namespace MonsoonAPI.Controllers.v1
{
    public class PrinterController : ControllerBase
    {
        public PrinterController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IConfiguration cfg) : 
            base("Printer", resMgr,  nodeMgr, cfg)
        { }

        [HttpGet]
        public async Task<IActionResult> GetPrinters()
        {
            const string method = "GetPrinters";
            try
            {
                LogEnter(method);
                List<string> printers = (List<string>)Rm.GetIssData(IssDataCodes.print_printers);
                if (printers == null)
                    return await ReturnError(method, "Failed to retrieve print_printers from SS");

                return await ReturnOk(method, printers);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }
    }
}