using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MonsoonAPI.models;
using MonsoonAPI.Models;
using MonsoonAPI.Services;
using ISM.Middleware2.Types;
using System.IO;
using ISM.Middleware2Si;

namespace MonsoonAPI.Controllers.v1 {


    public class SettingsController : ControllerBase {
        
        readonly ISettingsMgr _settingMgr;
        readonly IMwMgr _mwMgr;
        readonly INetworkingUtilities _networkingUtilities;

        public SettingsController(IMonsoonResMgr rm, INodeMgr nodeMgr, IConfiguration cfg, ISettingsMgr setMgr, IMwMgr mwMgr, INetworkingUtilities networkingUtilities)
            : base("Settings", rm, nodeMgr, cfg) {
            _settingMgr = setMgr;
            _settingMgr.Init(rm);

            _mwMgr = mwMgr;
            _networkingUtilities = networkingUtilities;

        }

        [HttpGet("{settingType}/{room}/{user}")]
        public async Task<IActionResult> GetSetting(string settingType, string room, string user) {
            var method = $"GetSetting - {settingType}, {room}, {user}";
            try {
                LogEnter(method);
                switch (settingType) {
                    case "presets":
                        HttpStatusCode ret = _settingMgr.GetPresets(room, user, out var class2Val);
                        return await ReturnCode(ret, class2Val, method);

                    default:
                        return await ReturnBadRequest(method, "Unrecognized settings request for " + settingType);
                }
            }
            catch (Exception ex) {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpGet("{settingType}/{room}")]
        public async Task<IActionResult> GetSetting(string settingType, string room) {
            var method = $"GetSettings/{settingType}/{room}";
            try {
                if (!settingType.Equals("presets", StringComparison.OrdinalIgnoreCase))
                    return await ReturnBadRequest(method, "This method is only supported for 'presets'");

                LogEnter(method);
                HttpStatusCode ret = _settingMgr.GetPresets(room, "X", out var class2Val);
                return await ReturnCode(ret, class2Val, method);
            }
            catch (Exception e) {
                return await ReturnError(method, e.Message);
            }

        }

        [HttpGet("{settingType}")]
        public async Task<IActionResult> GetSetting(string settingType) {
            Dictionary<string, string> class2Val = null;
            var method = $"GetSetting - {settingType}";
            try {
                LogEnter(method);
                HttpStatusCode ret;
                switch (settingType) {
                    case "recorder":
                        ret = _settingMgr.GetAllSettings(out var settings);
                        class2Val = settings.ToDictionary(set => set.Key.ToString(), set => set.Value, StringComparer.OrdinalIgnoreCase);
                        break;
                    case "license":
                        if ((ret = _settingMgr.GetFriendlyLicenses(out var friendlyLicenses)) == HttpStatusCode.OK)
                            return await ReturnOk(method, friendlyLicenses);
                        break;
                    case "rights":
                        if ((ret = _mwMgr.GetRights(out var rights)) == HttpStatusCode.OK)
                            return await ReturnOk(method, rights);
                        break;
                    case "presets":
                        ret = _settingMgr.GetPresets(null, null, out class2Val);
                        break;
                    default:
                        return await ReturnBadRequest(method, "Unrecognized settings request for " + settingType);
                }
                return await ReturnCode(ret, class2Val, method);
            }
            catch (Exception ex) {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPost("networking")]
        public async Task<IActionResult> ValidateVssIp([FromBody] IpAddressValidationRequest req) {
            try {
                LogEnter(nameof(ValidateVssIp));

                var result = await _networkingUtilities.IsIpValid(req);
                return Ok(new { IsValid = result });
            }
            catch {
                return Ok(new { IsValid = false });
            }
        }

        [HttpPost("getAutolabels")]
        public async Task<IActionResult> GetAutolabels([FromBody] GetAutolabelsRequest req)
        {
            var method = nameof(GetAutolabels);
            try
            {
                LogEnter(method);
                if (null == req)
                {
                    return await ReturnBadRequest(method, "request is null");
                }

                HttpStatusCode response;
                List<AutoLabelInfo> labels;
                switch (req.OwnerType)
                {
                    case AutolabelOwnerType.Department:
                        response = _mwMgr.GetDepartmentAutoLabels(req.OwnerName, req.ProcedureType, out labels);
                        break;
                    case AutolabelOwnerType.User:
                        response = _mwMgr.GetUserAutoLabels(req.OwnerName, req.ProcedureType, out labels);
                        break;
                    default:
                        return await ReturnBadRequest(method, $"Invalid owner type: {req.OwnerType}");
                }

                return await ReturnCode(response, labels, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpGet("activeAutolabels/{username}/{procedureType}")]
        public async Task<IActionResult> GetActiveAutolabels(string username, string procedureType)
        {
            var method = nameof(GetActiveAutolabels);
            try
            {
                LogEnter(method);

                HttpStatusCode response = _mwMgr.GetActiveAutoLabels(username, procedureType, out List<AutoLabelInfo> labels, out List<string> commonLabels);
                return await ReturnCode(response, new ActiveAutoLabelInfo(labels, commonLabels), method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpGet("allLabels")]
        public async Task<IActionResult> GetAllLabels()
        {
            var method = nameof(GetAllLabels);
            try
            {
                LogEnter(method);

                HttpStatusCode response = _mwMgr.GetAllLabels(out List<string> labels);
                return await ReturnCode(response, labels, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpGet("allProcTypes")]
        public async Task<IActionResult> GetAllProcTypes()
        {
            var method = nameof(GetAllLabels);
            try
            {
                LogEnter(method);

                HttpStatusCode response = _mwMgr.GetAllProcTypes(out List<ProcedureTypeInfo> procTypes);
                return await ReturnCode(response, procTypes, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPost("addAutolabel")]
        public async Task<IActionResult> AddAutolabel([FromBody] AddAutolabelRequest req)
        {
            var method = nameof(AddAutolabel);
            try
            {
                LogEnter(method);
                if (null == req)
                {
                    return await ReturnBadRequest(method, "request is null");
                }

                HttpStatusCode response;
                switch (req.OwnerType)
                {
                    case AutolabelOwnerType.Department:
                        response = _mwMgr.AddDepartmentAutolabel(req.OwnerName, req.ProcedureType, req.Label, req.AutolabelPosition, req.AutolabelColor);
                        break;
                    case AutolabelOwnerType.User:
                        response = _mwMgr.AddUserAutolabel(req.OwnerName, req.ProcedureType, req.Label, req.AutolabelPosition, req.AutolabelColor);
                        break;
                    default:
                        return await ReturnBadRequest(method, $"Invalid owner type: {req.OwnerType}");
                }

                return await ReturnCode(response, null, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPost("editLabel")]
        public async Task<IActionResult> EditLabel([FromBody] EditLabelRequest req)
        {
            var method = nameof(EditLabel);
            try
            {
                LogEnter(method);
                if (null == req)
                {
                    return await ReturnBadRequest(method, "request is null");
                }

                HttpStatusCode response;
                switch (req.OwnerType)
                {
                    case AutolabelOwnerType.Department:
                        response = _mwMgr.EditDepartmentLabel(req.OwnerName, req.ProcedureType, req.OldLabel, req.NewLabel, req.NewIsAutolabel, req.NewAutolabelPosition, req.NewAutolabelColor);
                        break;
                    case AutolabelOwnerType.User:
                        response = _mwMgr.EditUserLabel(req.OwnerName, req.ProcedureType, req.OldLabel, req.NewLabel, req.NewIsAutolabel, req.NewAutolabelPosition, req.NewAutolabelColor);
                        break;
                    default:
                        return await ReturnBadRequest(method, $"Invalid owner type: {req.OwnerType}");
                }

                return await ReturnCode(response, null, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPost("deleteLabel")]
        public async Task<IActionResult> DeleteLabel([FromBody] DeleteLabelRequest req)
        {
            var method = nameof(DeleteLabel);
            try
            {
                LogEnter(method);
                if (null == req)
                {
                    return await ReturnBadRequest(method, "request is null");
                }

                HttpStatusCode response;
                switch (req.OwnerType)
                {
                    case AutolabelOwnerType.Department:
                        response = _mwMgr.DeleteDepartmentLabel(req.OwnerName, req.ProcedureType, req.Label);
                        break;
                    case AutolabelOwnerType.User:
                        response = _mwMgr.DeleteUserLabel(req.OwnerName, req.ProcedureType, req.Label);
                        break;
                    default:
                        return await ReturnBadRequest(method, $"Invalid owner type: {req.OwnerType}");
                }

                return await ReturnCode(response, null, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPost("deleteLabelGlobal")]
        public async Task<IActionResult> DeleteLabelGlobal([FromBody] string label)
        {
            var method = nameof(DeleteLabelGlobal);
            try
            {
                LogEnter(method);
                if (null == label)
                {
                    return await ReturnBadRequest(method, "request is null");
                }

                HttpStatusCode response = _mwMgr.DeleteLabelGlobal(label);

                return await ReturnCode(response, null, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPost("setAutolabels")]
        public async Task<IActionResult> SetAutolabels([FromBody] SetAutolabelsRequest req)
        {
            var method = nameof(SetAutolabels);
            try
            {
                LogEnter(method);

                if (null == req)
                {
                    return await ReturnBadRequest(method, "request is null");
                }

                HttpStatusCode response;
                switch (req.OwnerType)
                {
                    case AutolabelOwnerType.Department:
                        response = _mwMgr.SetDepartmentAutoLabels(req.OwnerName, req.ProcedureType, req.Labels);
                        break;
                    case AutolabelOwnerType.User:
                        response = _mwMgr.SetUserAutoLabels(req.OwnerName, req.ProcedureType, req.Labels);
                        break;
                    default:
                        return await ReturnBadRequest(method, $"Invalid owner type: {req.OwnerType}");
                }

                return await ReturnCode(response, null, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPost("setCommonAutoLabels")]
        public async Task<IActionResult> SetCommonAutoLabels([FromBody] SetCommonAutolabelsRequest req)
        {
            var method = nameof(SetCommonAutoLabels);
            try
            {
                LogEnter(method);

                if (null == req)
                {
                    return await ReturnBadRequest(method, "request is null");
                }

                var response = _mwMgr.SetCommonAutoLabels(req.Department, req.Labels);
                return await ReturnCode(response, null, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPost("uploadLabels")]
        public async Task<IActionResult> UploadLabels([FromForm] LabelUploadRequest req)
        {
            var method = nameof(UploadLabels);
            try
            {
                LogEnter(method);

                if (null == req)
                {
                    return await ReturnBadRequest(method, "request is null");
                }
                if (null == req.LabelsCsv)
                {
                    return await ReturnBadRequest(method, "request filestream is null");
                }

                using (Stream csvStream = req.LabelsCsv.OpenReadStream())
                {
                    HttpStatusCode response = _mwMgr.UploadLabelsCsv(csvStream, req.Replace);
                    return await ReturnCode(response, null, method);
                }
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpGet("commonautolabels/{department}")]
        public async Task<IActionResult> GetCommonAutolabels(string department)
        {
            var method = nameof(GetCommonAutolabels);
            try
            {
                LogEnter(method);

                HttpStatusCode response = _mwMgr.GetCommonAutoLabels(department, out List<string> labels);
                return await ReturnCode(response, labels, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpGet("activecommonautolabels/{username}")]
        public async Task<IActionResult> GetActiveCommonAutoLabels(string username)
        {
            var method = nameof(GetActiveCommonAutoLabels);
            try
            {
                LogEnter(method);

                HttpStatusCode response = _mwMgr.GetActiveCommonAutoLabels(username, out List<string> labels);
                return await ReturnCode(response, labels, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetSettings([FromBody] SettingsInfo settingsInfo) {
            const string method = "SetSettings";

            try {
                LogEnter(method);
                HttpStatusCode ret;
                if (string.IsNullOrEmpty(settingsInfo?.setting_type))
                    return await ReturnBadRequest(method, "setting type null");

                switch (settingsInfo.setting_type) {
                    case "presets":
                        ret = _settingMgr.SetPresets(settingsInfo);
                        break;
                    case "settings":
                        ret = _settingMgr.SetSettings(settingsInfo);
                        break;
                    case "license":
                        ret = _settingMgr.SetLicense(settingsInfo);
                        break;
                    default:
                        return await ReturnBadRequest(method, "Unknown tag " + settingsInfo.setting_type);
                }
                return await ReturnCode(ret, null, method);
            }
            catch (Exception ex) {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPost("pingExternalTasks")]
        public async Task<IActionResult> PingExternalTasks()
        {
            try
            {
                LogEnter(nameof(PingExternalTasks));

                Dictionary<ExternalTaskMgr.Tasks, bool> results;
                using (var pieProxy = Rm.GetPieProxy())
                {
                    if (null != pieProxy)
                    {
                        if (0 != pieProxy.Proxy.CheckConnectivity(out var pieTasks, out string error))
                        {
                            return await ReturnError(nameof(PingExternalTasks), error);
                        }
                        results = pieTasks.ToDictionary(task => ExternalTaskMgr.Ism2MonsoonTask(task.Key),
                            task => task.Value.Key);
                    }
                    else
                    {
                        results =
                            new List<ExternalTaskMgr.Tasks>(
                                    (ExternalTaskMgr.Tasks[])Enum.GetValues(typeof(ExternalTaskMgr.Tasks)))
                                .ToDictionary(task => task, task => false);
                    }
                }

                using (var libProxy = Rm.GetLibProxy())
                {
                    if (libProxy != null) 
                    {
                        if (libProxy.Proxy.CheckCopyDestination(out var copyAvailable, out var err) != 0)
                            return await ReturnError(nameof(PingExternalTasks), err);

                        if (copyAvailable != null)
                            results[ExternalTaskMgr.Tasks.CopyTask] = (bool)copyAvailable;
                    }
                    else
                    {
                        results[ExternalTaskMgr.Tasks.CopyTask] = false;
                    }
                }

                    return Ok(results);
            }
            catch (Exception ex)
            {
                return await ReturnError(nameof(PingExternalTasks), ex.Message);
            }
        }
    }
}