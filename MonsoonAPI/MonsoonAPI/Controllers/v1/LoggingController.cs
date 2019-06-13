using IsmLogCommon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MonsoonAPI.models;

namespace MonsoonAPI.Controllers.v1
{
    public class LoggingController : ControllerBase
    {
        public LoggingController(IMonsoonResMgr rm, INodeMgr nodeMgr, IConfiguration cfg) : 
            base("Logging", rm,  nodeMgr, cfg)
        { }

        //TODO does this need an access log endpoint?
        [HttpPost]
        public IActionResult LogEvent([FromBody]LogMsg logMsg)
        {
            if (logMsg == null)
                return BadRequest();

            //TODO determine time stamping
            Rm.LogEventWeb(logMsg.type, 0, logMsg.msg, logMsg.verbosity);
            return Ok();
        }

    }
}