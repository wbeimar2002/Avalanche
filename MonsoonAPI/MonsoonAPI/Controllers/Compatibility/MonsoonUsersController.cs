using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MonsoonAPI.Controllers.Compatibility
{
    public class MonsoonUsersController : Controller
    {
        protected IMonsoonResMgr _rm;

        public MonsoonUsersController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IConfiguration cfg)
        {
            _rm = resMgr;
            _rm.Init(cfg, nodeMgr);
        }
        public IActionResult Index()
        {
            string command = Request.Query["command"].ToString();
            if (string.IsNullOrWhiteSpace(command))
            {
                var message = "MonsoonUsersController - No command header";
                _rm.LogEvent(EventLogEntryType.Error, 0, message, 1);
                return StatusCode(StatusCodes.Status501NotImplemented);
            }
            _rm.LogEvent(EventLogEntryType.Information, 0, $"MonsoonUsersController Service entered with command: {command}", 5);

            string strResponse = string.Empty;
            int statusCode;
            var defaultBreak = "";

            switch (command)
            {
                case "ping":
                    statusCode = (int)HttpStatusCode.NoContent;
                    break;
                default:
                    statusCode = 0;
                    defaultBreak = "Uncrecognized command.";
                    _rm.LogEvent(EventLogEntryType.Error, 0, "MonsoonUsersController Unrecognized command " + command, 1);
                    break;
            }

            if (statusCode == StatusCodes.Status200OK && !string.IsNullOrWhiteSpace(strResponse))
            {
                return StatusCode(statusCode, strResponse);
            }

            if (!string.IsNullOrWhiteSpace(defaultBreak))
            {
                return StatusCode(statusCode, defaultBreak);
            }

            return StatusCode(statusCode);
        }
    }
}