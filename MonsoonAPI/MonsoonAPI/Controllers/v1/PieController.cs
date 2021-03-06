using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using IsmLogCommon;
using IsmStateServer;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PatInfoEngine.Types;
using PatInfoEngineSi;

namespace MonsoonAPI.Controllers.v1
{
    public class PieController : ControllerBase
    {
        private readonly int _maxRecs ;

        public PieController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IConfiguration cfg) :
            base("Pie", resMgr, nodeMgr, cfg)
        {
            _maxRecs = Rm.Cfg.GetValue<int>("max_pie_records");
            if (_maxRecs > 100)
                IsmLog.LogEvent(EventLogEntryType.Warning, 0, $"System is configured to get {_maxRecs} records. This is not recommended as it can slow down the system considerably", 3);

        }

        [HttpGet("{lastName}/{mrn}")]
        public async Task<IActionResult> GetPatList(string lastName, string mrn)
        {
            lastName = lastName.UrlDecode();
            mrn = mrn.UrlDecode();

            var method = $"GetPatList/{lastName}/{mrn}";
            return await DoGetPatList(method, lastName, mrn, CultureInfo.CurrentCulture);
        }

        [HttpGet("{lastName}/{mrn}/{accession}/{procId}")]
        public async Task<IActionResult> GetPatList(string lastName, string mrn, string accession, string procId)
        {
            lastName = lastName.UrlDecode();
            mrn = mrn.UrlDecode();
            accession = accession.UrlDecode();
            procId = procId.UrlDecode();

            var method = $"GetPatList/{lastName}/{mrn}/{accession}/{procId}";
            return await DoGetPatList(method, lastName, mrn, CultureInfo.CurrentCulture, accession, procId);
        }

        [HttpGet("{lastName}/{mrn}/{accession}/{procId}/{department}")]
        public async Task<IActionResult> GetPatList(string lastName, string mrn, string accession, string procId,
            string department)
        {
            lastName = lastName.UrlDecode();
            mrn = mrn.UrlDecode();
            accession = accession.UrlDecode();
            procId = procId.UrlDecode();
            department = department.UrlDecode();

            var method = $"GetPatList/{lastName}/{mrn}/{accession}/{procId}/{department}";
            return await DoGetPatList(method, lastName, mrn, CultureInfo.CurrentCulture, accession, procId, department);
           
        }

        private async Task<IActionResult> DoGetPatList(string method, string lastName, string mrn, CultureInfo searchCulture, string accession = null, string procId = null, string department = null)
        {
            try
            {
                var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(HttpContext);
                //TODO should these be currentcultureignroecase?
                // put together paramas
                var searchParams = new Dictionary<EPieFields, string>();
                if (!string.IsNullOrEmpty(lastName) && !string.Equals(lastName, "-", StringComparison.OrdinalIgnoreCase))
                    searchParams[EPieFields.LastName] = lastName;
                if (!string.IsNullOrEmpty(mrn) && !string.Equals(mrn, "-", StringComparison.OrdinalIgnoreCase))
                    searchParams[EPieFields.MRN] = mrn;
                if (!string.IsNullOrEmpty(accession) && !string.Equals(accession, "-", StringComparison.OrdinalIgnoreCase))
                    searchParams[EPieFields.Accession] = accession;
                if (!string.IsNullOrEmpty(procId) && !string.Equals(procId, "-", StringComparison.OrdinalIgnoreCase))
                    searchParams[EPieFields.Procedure_Id] = procId;
                if (!string.IsNullOrEmpty(department) && !string.Equals(department, "-", StringComparison.OrdinalIgnoreCase) && !string.Equals(department, "*", StringComparison.OrdinalIgnoreCase))
                    searchParams[EPieFields.Department] = department;

                bool bSearchByRoom = bool.Parse(Rm.Cfg[ESettings.filter_search_by_room.ToString()]);
                if (bSearchByRoom)
                    searchParams[EPieFields.RoomName] = Rm.RecorderSettings.m_strAdtRoomName;

                // search
                string strList;
                using (var pieProxy = Rm.GetPieProxy())
                {
                    if (pieProxy == null)
                        return await ReturnOffline(MsntSi.Types.eSystemType.patinfo_engine, method);


                    if (pieProxy.Proxy.PatList_Search(searchParams, 0, _maxRecs, searchCulture.Name, accessInfo, out strList, out var strErr) != 0)
                        return await ReturnError(method, "PatList_Search err: " + strErr);
                }

                var patList = XElement.Parse(strList);

                // massage the list
                var patients = patList.Elements("patient");
                patients.AsParallel().ForAll(pat =>
                {
                    bool modifiable = false;
                    string scheduleId = pat.Attribute("scheduleid")?.Value;
                    if (string.IsNullOrEmpty(scheduleId))
                    {
                        modifiable = true;
                    }
                    else if (scheduleId.StartsWith(HL7Common.IsmPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        modifiable = true;
                    }

                    pat.Add(new XAttribute("AllowModification", modifiable));

                });

                return await ReturnOk(method, new { pat_list = patList });
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }
    }
}