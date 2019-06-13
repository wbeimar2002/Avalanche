using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MsntSi.Types;

namespace MonsoonAPI.Controllers.v1
{
    [Route("v1/[controller]")]
    public  class ControllerBase : Controller
    {
        private readonly IMonsoonResMgr _rm;
        private readonly string _controllerName;

        protected ControllerBase(string controllerName, IMonsoonResMgr resMgr, INodeMgr nodeMgr,
            IConfiguration cfg, IActiveMgr activeMgr = null)
        {
            _controllerName = controllerName;
            nodeMgr.Init(resMgr, activeMgr);

            _rm = resMgr;
            _rm.Init(cfg, nodeMgr);
        }

        protected IMonsoonResMgr Rm => _rm;
        

        protected Task<StatusCodeResult> ReturnOffline(eSystemType serv, string strMethod)
        {
            _rm.LogEvent(EventLogEntryType.Warning, 0, $"{strMethod} failure because {serv} is offline", 3);
            LogExit(strMethod);
            return Task.FromResult(new StatusCodeResult(500));
        }

        protected Task<StatusCodeResult> ReturnBadRequest(string method, string msg = null)
        {
            Rm.LogEvent(EventLogEntryType.Warning, 0, $"{method} returning BadRequest. {msg}", 3);
            LogExit(method);
            return Task.FromResult(new StatusCodeResult((int)HttpStatusCode.BadRequest));
        }
        
        protected Task<StatusCodeResult> ReturnError(string method, string strErr)
        {
            _rm.LogEvent(EventLogEntryType.Error, 0, strErr, 3);
            LogExit(method);
            return Task.FromResult(new StatusCodeResult(500));
        }
   
        protected Task<IActionResult> ReturnOk(string method, object retObj = null)
        {
            return ReturnCode(HttpStatusCode.OK, retObj, method);

        }

        protected async Task<IActionResult> ReturnCode(HttpStatusCode retCode, object retObj, string method)
        {
            LogExit(method);
            if (retCode != HttpStatusCode.OK || retObj == null)
                return await Task.FromResult(new StatusCodeResult((int) retCode));
            else
                return await Task.FromResult(Ok(retObj));
        }

        protected void LogEnter( string method)
        {
            _rm.LogEvent(EventLogEntryType.Information, 0, $"-> {_controllerName}Controller.{method} entered", 4);
        }

        private void LogExit(string method)
        {
            _rm.LogEvent(EventLogEntryType.Information, 0, $"<- {_controllerName}Controller.{method} exiting", 4);
        }

        
    }
}
