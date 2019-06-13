using System;
using System.Diagnostics;
using System.Collections.Generic;
using  System.Linq;
using System.Threading.Tasks;
using IsmUtility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MonsoonAPI.models;
using MsntSi.Types;

namespace MonsoonAPI.Controllers.v1
{
    public class LabelsController : ControllerBase
    {
        private readonly IActiveMgr _activeMgr;
        private readonly ILibMgr _libMgr;

        public LabelsController(IMonsoonResMgr resMgr, IActiveMgr activeMgr,
            IMwMgr mwMgr, ILibMgr libMgr, INodeMgr nodeMgr, IConfiguration cfg) :
            base("Labels", resMgr,  nodeMgr, cfg, activeMgr)
        {
            _activeMgr = activeMgr;
            _libMgr = libMgr;
        }

        /// <summary>
        /// Get a list of annotations based on user name and procedure type
        /// </summary>
        [HttpGet("{login}/{proc}")]
        public async Task<IActionResult> GetLabels(string login, string proc)
        {
            proc = proc.UrlDecode();
            var method = $"GetLabels/{login}/{proc}";
            LogEnter(method);
            return await DoGetLabels(method, login, proc);
        }

        [HttpGet("{login}")]
        public async Task<IActionResult> GetLabels(string login)
        {
            var method = $"GetLabels/{login}";
            LogEnter(method);
            return await DoGetLabels(method, login, string.Empty);
        }


        /// <summary>
        /// Delete label based on user name and procedure type
        /// </summary>
        [HttpDelete("{login}/{proc}/{label}")]
        public async Task<IActionResult> DeleteLabel(string login, string proc, string label)
        {
            proc = proc.UrlDecode();
            label = label.UrlDecode();
            var method = $"DeleteLabel/{login}/{proc}/{label}";
            LogEnter(method);
            return await DoDeleteLabel(method, login, proc, label);
          

        }

        /// <summary>
        /// Delete label based on user name and procedure type
        /// </summary>
        [HttpDelete("{login}/{label}")]
        public async Task<IActionResult> DeleteLabel(string login, string label)
        {
            label = label.UrlDecode();
            var method = $"DeleteLabel/{login}/{label}";
            LogEnter(method);
            return await DoDeleteLabel(method, login, string.Empty, label);
        }

        [HttpPost]
        public async Task<IActionResult> SetContentLabel([FromBody]LabelInfo labelInfo)
        {
            var method = $"SetContentLabels - {labelInfo?.proc_id?.m_strLibId}";
            try
            {
                LogEnter(method);
                if (string.IsNullOrEmpty(labelInfo?.content_item) || labelInfo.label == null)
                    return await ReturnBadRequest(method, "content_item or label is missing");

                var res = labelInfo.proc_id?.m_strLibId != null
                    ? _libMgr.SetLibraryLabel(labelInfo)
                    : _activeMgr.SetLabel(labelInfo.content_item, labelInfo.label);

                return await ReturnCode(res, null, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }

        }
       

        private async Task<IActionResult> DoGetLabels(string method, string login, string proc)
        {
            try
            {
                List<string> labels;
                using (var mwProxy = Rm.GetMwProxy())
                {
                    if (mwProxy == null )
                        return await ReturnOffline(eSystemType.middleware, method);

                    if (mwProxy.Proxy.Department_GetLabels(login, proc,  out labels, out var strErr) != 0)
                        return await ReturnError(method,"Department_GetLabels errored: " + strErr);
                }

                var labelsUnique = labels.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                Rm.LogEvent(EventLogEntryType.Information, 0, $"DoGetLabels returning {labelsUnique.Length} labels", 4);
                return await ReturnOk(method, labelsUnique);

            }
            catch (Exception ex)
            {
                return await  ReturnError(method, ex.Message);
            }
        }

        private async Task<IActionResult> DoDeleteLabel(string method, string login, string proc, string label)
        {
            try
            {
                if (string.IsNullOrEmpty(label))
                    return await ReturnBadRequest(method, "Label is blank");

                using (var mwProxy = Rm.GetMwProxy())
                {
                    if (mwProxy == null )
                        return await ReturnOffline(eSystemType.middleware, method);

                    if (mwProxy.Proxy.Department_DeleteLabel(login, proc,  label, out var strErr) != 0)
                        return await ReturnError(method, $"Department_DeleteLabel returned error {strErr}");
                }
                return await ReturnOk(method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

    }
}