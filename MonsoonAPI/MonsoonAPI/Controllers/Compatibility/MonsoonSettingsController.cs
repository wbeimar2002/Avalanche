using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using IsmUtility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Configuration;

namespace MonsoonAPI.Controllers.Compatibility
{
    [Route("monsoonapi/settings.aspx")]
    public class MonsoonSettingsController : Controller
    {
        protected IMonsoonResMgr _rm;
        private readonly IMwMgr _mwMgr;

        public MonsoonSettingsController(IMonsoonResMgr resMgr, IMwMgr mwMgr, INodeMgr nodeMgr, IConfiguration cfg)
        {
            _rm = resMgr;
            _rm.Init(cfg, nodeMgr);
            _mwMgr = mwMgr;
        }

        [HttpGet]
        public IActionResult Index()
        {
            string command = Request.Query["command"].ToString();
            if (string.IsNullOrWhiteSpace(command))
            {
                var message = "MonsoonSettingsController - No command header";
                _rm.LogEvent(EventLogEntryType.Error, 0, message, 1);
                return StatusCode(StatusCodes.Status501NotImplemented);
            }
            _rm.LogEvent(EventLogEntryType.Information, 0, $"MonsoonSettingsControler Service entered with command: {command}", 5);

            string strResponse = string.Empty;
            int statusCode;
            var defaultBreak = "";

            switch (command)
            {
                case "get_setting":
                    statusCode = (int)GetSetting(out strResponse);
                    break;
                default:
                    statusCode = 0;
                    defaultBreak = "Uncrecognized command.";
                    _rm.LogEvent(EventLogEntryType.Error, 0, "MonsoonSettingsController Unrecognized command " + command, 1);
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

        private HttpStatusCode GetSetting(out string strResponse)
        {
            strResponse = string.Empty;
            try
            {
                HttpStatusCode retStatus = HttpStatusCode.OK;

                strResponse = string.Empty;
                string strTag = Request.Query["tag"].ToString();
                if (string.IsNullOrEmpty(strTag))
                {
                    strResponse = string.Empty;
                    _rm.LogEvent(EventLogEntryType.Error, 0, "MonsoonSettingsController.GetSetting failed because tag is not passed in", 1);
                    return HttpStatusCode.OK;
                }

                // go retrieve
                switch (strTag)
                {
                    case "define_user_annotations":
                        var setting = false;
                        retStatus = _mwMgr.GetDefineUserLabels(out setting);
                        strResponse = setting.ToString();
                        break;
                    default:
                        _rm.LogEvent(EventLogEntryType.Error, 0, "MonsoonSettingsController.GetSetting Unrecognized tag " + strTag, 1);
                        retStatus = HttpStatusCode.NotFound;
                        break;
                }
                return retStatus;
            }
            catch (Exception ex)
            {
                strResponse = "MonsoonSettingsController.GetSetting failed with err " + ex.Message;
                _rm.LogEvent(EventLogEntryType.Error, 0, strResponse, 1);
                return HttpStatusCode.InternalServerError;
            }
        }
    }
}