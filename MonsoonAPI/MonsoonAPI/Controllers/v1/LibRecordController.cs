using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MonsoonAPI.models;
using IsmStateServer.Types;
using MsntSi.Types;

using System.Net;

namespace MonsoonAPI.Controllers.v1
{
    public class LibRecordController : ControllerBase
    {
        private readonly ILibMgr _libMgr;

        public LibRecordController(IMonsoonResMgr rm, IConfiguration cfg, ILibMgr libMgr, INodeMgr nodeMgr,
            IMwMgr mwMgr) :
            base("LibRecord", rm, nodeMgr, cfg)
        {
            _libMgr = libMgr;
        }

        [HttpGet("{libName}/{libId}/{login}")]
        public async Task<IActionResult> GetRecord(string libName, string libId, string login)
        {
            var method = $"GetRecord/{libName}/{libId}/{login}";
            try
            {
                LogEnter(method);
                var procId = new ProcedureId {m_strLibId = libId, m_strLibName = libName};
                HttpStatusCode retCode = _libMgr.GetLibRecord(procId, login, false, out var dmag, CultureInfo.CurrentCulture);
                return await ReturnCode(retCode, dmag, method);
            }
            catch (Exception e)
            {
                return await ReturnError(method, e.Message);
            }
        }

        [HttpGet("{libName}/{libId}/{login}/{compare}")]
        public async Task<IActionResult> GetRecord(string libName, string libId, string login, bool compare)
        {
            var method = $"GetRecord/{libName}/{libId}/{login}/{compare}";
            try
            {
                LogEnter(method);
                var procId = new ProcedureId {m_strLibId = libId, m_strLibName = libName};
                HttpStatusCode retCode = _libMgr.GetLibRecord(procId, login, compare, out var dmag, CultureInfo.CurrentCulture);
                return await ReturnCode(retCode, dmag, method);
            }
            catch (Exception e)
            {
                return await ReturnError(method, e.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRecAccess([FromBody] ProcedureIdM procId)
        {
            var method = $"UpdateRecAccess:{procId?.m_strLibId}";
            try
            {
                LogEnter(method);
                if (string.IsNullOrEmpty(procId?.m_strLibId))
                    return await ReturnBadRequest(method, "lib id is null");

                // now - on to library!
                using (var libProxy = Rm.GetLibProxy())
                {
                    // get DMAG
                    if (libProxy == null)
                        return await ReturnOffline(eSystemType.library, method);

                    if (libProxy.Proxy.UpdateDmagLastUpdate(procId.ToProcId(), "Accessed from Monsoon",
                            out var strErr) != 0)
                        return await ReturnError(method, "UpdateDmagLastUpdate failed with err " + strErr);
                }

                Rm.LogEvent(EventLogEntryType.Information, 0, "Dmag updated successfully", 3);
                return await ReturnOk(method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }

        }
    }
}