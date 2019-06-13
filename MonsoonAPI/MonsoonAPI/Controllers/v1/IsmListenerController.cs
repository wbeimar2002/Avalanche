using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using IsmStateServer;

namespace MonsoonAPI.Controllers.v1
{
    public class IsmListenerController : ControllerBase
    {
        private readonly INodeMgr _nodeMgr;

        public IsmListenerController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IConfiguration cfg,
            IActiveMgr activeMgr) :
            base("IsmListener", resMgr, nodeMgr, cfg)
        {
            _nodeMgr = nodeMgr;
            _nodeMgr.Init(resMgr, activeMgr);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessEvent([FromBody] SubscriberInfoWeb.EventMonsoon evt)
        {
            var method = $"ProcessEvent:{evt?.Code}";
            try
            {
                LogEnter(method);

                if (Rm.MonsoonCfg == EMonsoonConfig.EasyView)
                    return await ReturnBadRequest(method, "IsmListener not supported on EasyView");

                if (evt == null)
                    return await ReturnBadRequest(method, "evt is null");

                _nodeMgr.ProcessIsmEvent(evt);
                return await ReturnOk(method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }
    }
}
