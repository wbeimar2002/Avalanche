using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ISM.LibrarySi;
using MsntSi.Types;
using ISM.Library.Types;
using MonsoonAPI.models;

namespace MonsoonAPI.Controllers.v1
{
    public class LibTasksController : ControllerBase
    {
        private readonly ILibMgr _libMgr;

        public LibTasksController(IMonsoonResMgr rm, ILibMgr libMgr, IMwMgr mwMgr, INodeMgr nodeMgr, IConfiguration cfg)
            : base("LibTasks", rm, nodeMgr, cfg)
        {
            _libMgr = libMgr;
        }

        [HttpGet("{status}/{taskType}")]
        public async Task<IActionResult> GetTasks(eStatus status, ePayloadType taskType)
        {
            var method = $"GetTasks/{status}/{taskType}";
            try
            {
                LogEnter(method);
                HttpStatusCode ret = _libMgr.DoGetTasks(status, taskType, out IEnumerable<LibWorkItemBase> tasks);
                return await ReturnCode(ret, tasks, method);
            }
            catch (Exception e)
            {
                return await ReturnError(method, e.Message);
            }
        }

        [HttpGet("{status}")]
        public async Task<IActionResult> GetTasks(eStatus status)
        {
            var method = $"GetTasks/{status}";
            try
            {
                LogEnter(method);
                HttpStatusCode ret = _libMgr.DoGetTasks(status, null, out IEnumerable<LibWorkItemBase> tasks);
                return await ReturnCode(ret, tasks, method);
            }
            catch (Exception e)
            {
                return await ReturnError(method, e.Message);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            const string method = "GetTasks";
            try
            {
                LogEnter(method);
                HttpStatusCode ret = _libMgr.DoGetTasks(null, null, out IEnumerable<LibWorkItemBase> tasks);
                return await ReturnCode(ret, tasks, method);
            }
            catch (Exception e)
            {
                return await ReturnError(method, e.Message);
            }

        }

        [HttpPost]
        public async Task<IActionResult> LaunchTask([FromBody]WorkItemExportM taskInfo)
        {
            var method = $"LaunchTask:{taskInfo?.m_ProcId?.m_strLibId}";
            try
            {
                var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(HttpContext);
                LogEnter(method);
                // parse input
                if (string.IsNullOrEmpty(taskInfo?.m_ProcId?.m_strLibId) ||
                    taskInfo.m_PayloadType == ePayloadType.other)
                    return await ReturnBadRequest(method);

                // tell library to launch
                eLibErr ret;
                using (var libProxy = Rm.GetLibProxy())
                {
                    if (libProxy == null )
                        return await ReturnOffline(eSystemType.library, method);

                    // add task
                    ret = (eLibErr) libProxy.Proxy.QueueTask(taskInfo.ToWorkItemExport(accessInfo), out _);
                }
                return await ReturnOk(method, ret);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }
    }
}