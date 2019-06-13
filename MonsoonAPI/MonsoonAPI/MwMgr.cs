using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using MonsoonAPI.models;
using ISM.Middleware2Si;
using System.Text;
using IsmLogCommon;
using Microsoft.AspNetCore.Http;
using ISM.Middleware2.Types;
using System.IO;
using IsmUtility;
using IsmUtility.IO;
using System.Linq;
using MonsoonAPI.Models;

namespace MonsoonAPI
{
    public interface IMwMgr
    {
        HttpStatusCode GetRights(out string[] rights);
        HttpStatusCode DoAuthorize(AuthInfo authInfo, out eMwError authStatus, out string username);
        HttpStatusCode Authenticate(AuthInfo authInfo);
        HttpStatusCode AddProcType(string proc, string username);
        HttpStatusCode AddProcType(string proc);
        HttpStatusCode AddDepartmentProcType(string proc, string department);

        HttpStatusCode GetDepartments(out List<string> departments);
        HttpStatusCode GetDefineUserLabels(out bool defineSetting);

        HttpStatusCode SetDepartmentAutoLabels(string departmentName, string procedureType, List<AutoLabelInfo> labels);
        HttpStatusCode SetUserAutoLabels(string userName, string procedureType, List<AutoLabelInfo> labels);

        HttpStatusCode GetDepartmentAutoLabels(string departmentName, string procedureType, out List<AutoLabelInfo> labels);
        HttpStatusCode GetUserAutoLabels(string userName, string procedureType, out List<AutoLabelInfo> labels);
        HttpStatusCode GetActiveAutoLabels(string username, string procedureType, out List<AutoLabelInfo> labels, out List<string> commonLabels);

        HttpStatusCode GetCommonAutoLabels(string department, out List<string> labels);
        HttpStatusCode GetActiveCommonAutoLabels(string username, out List<string> labels);
        HttpStatusCode SetCommonAutoLabels(string department, List<string> labels);

        HttpStatusCode GetAllLabels(out List<string> labels);

        HttpStatusCode GetAllProcTypes(out List<ProcedureTypeInfo> procTypes);

        HttpStatusCode UploadLabelsCsv(Stream labelsCsv, bool replace);

        HttpStatusCode AddDepartmentAutolabel(string departmentName, string procedureType, string label, int? position, string color);
        HttpStatusCode AddUserAutolabel(string userName, string procedureType, string label, int? position, string color);

        HttpStatusCode EditDepartmentLabel(string departmentName, string procedureType, string oldLabel, string newLabel, bool? newIsAutolabel, int? newAutolabelPosition, string newAutolabelColor);
        HttpStatusCode EditUserLabel(string userName, string procedureType, string oldLabel, string newLabel, bool? newIsAutolabel, int? newAutolabelPosition, string newAutolabelColor);

        HttpStatusCode DeleteDepartmentLabel(string departmentName, string procedureType, string label);
        HttpStatusCode DeleteUserLabel(string userName, string procedureType, string label);
        HttpStatusCode DeleteLabelGlobal(string label);
    }

    public class MwMgr : IMwMgr
    {
        // todo - where should this live? (probably not here)
        const string Department = "Department";
        const string Right = "Right";
        const string ProcedureType = "ProcedureType";
        const string Label = "Label";
        const string Autolabel = "Autolabel";
        private static readonly List<string> labelHeaders = new List<string> { Department, Right, ProcedureType, Label, Autolabel };

        readonly IMonsoonResMgr _rm;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MwMgr(IMonsoonResMgr rm, IHttpContextAccessor accessor)
        {
            _rm = rm;
            _httpContextAccessor = accessor;
        }

        public HttpStatusCode GetDepartments(out List<string> departments)
        {
            departments = null;
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                        return ReturnOffline("GetDepartments");

                    // call mw
                    if (mwProxy.Proxy.Department_GetAllDeparments(out departments, out var strErr) != 0)
                    {
                        return ReturnErr("Department_GetAllDeparments err: " + strErr);
                    }

                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr("GetDepartments failed with err " + ex.Message);
            }
        }
        public HttpStatusCode GetRights(out string[] rights)
        {
            rights = null;
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                        return ReturnOffline("GetRights");

                    // call mw
                    if (mwProxy.Proxy.Rights_Get(out rights, out _, out var strErr) != 0)
                        return ReturnErr("Rights_Get err: " + strErr);

                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr("GetRights failed with err " + ex.Message);
            }
        }

        public HttpStatusCode Authenticate(AuthInfo authInfo)
        {
            try
            {

                _rm.LogEvent(EventLogEntryType.Information, 0, $"Authenticate entered for {authInfo.Username}", 3);

                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                        return ReturnOffline("Authenticate");

                    // convert data to credentials
                    string userCredentials = MwUtils.Credentials(authInfo.Username, authInfo.Password);

                    var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext, authInfo.Username);

                    if (mwProxy.Proxy.AA_Authenticate(userCredentials, accessInfo, out _) == eMwError.Ok)
                    {
                        return HttpStatusCode.OK;
                    }
                    _rm.LogEvent(EventLogEntryType.Error, 0, "Authentication failed", 3);
                    return HttpStatusCode.Forbidden;
                }

            }
            catch (Exception ex)
            {
                return ReturnErr("Authenticate err: " + ex.Message);
            }
        }

        public HttpStatusCode DoAuthorize(AuthInfo authInfo, out eMwError authStatus, out string username)
        {
            authStatus = eMwError.not_found;
            username = authInfo?.Username;
            try
            {
                // this method can only be called locally. If box is set to autologin, get administrative login & return
                if (authInfo?.Autologin == true)
                {
                    _rm.LogEvent(EventLogEntryType.Information, 0, "DoAuthorize returning administrative login for an autologin request", 3);
                    username = MwUtils.ADMIN_LOGIN;
                    authStatus = eMwError.Ok;
                    return HttpStatusCode.OK;
                }

                // authorize credentials
                if (authInfo == null)
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0, "DoAuthorize failed because proper AuthInfo is not passed in", 3);
                    return HttpStatusCode.BadRequest;
                }

                // if no user name passed in - skip (but, because of how MonsoonWeb is structured, still return Ok!)
                if (string.IsNullOrEmpty(username))
                {
                    _rm.LogEvent(EventLogEntryType.Information, 0, "DoAuthorize skipping because username is not passed into auth", 8);
                    authStatus = eMwError.not_found;
                    return HttpStatusCode.OK;
                }

                _rm.LogEvent(EventLogEntryType.Information, 0, $"Auth entered for {authInfo.Username}", 3);

                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                        return ReturnOffline("DoAuthorize");

                    // convert data to credentials
                    string strPassword = Encoding.UTF8.GetString(Convert.FromBase64String(authInfo.Password));
                    string userCredentials = MwUtils.Credentials(authInfo.Username, strPassword);

                    var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(_httpContextAccessor.HttpContext, authInfo.Username);

                    // call mw
                    authStatus = mwProxy.Proxy.AA_CheckManageCredentials(userCredentials, accessInfo, out var strErr);
                    if (authStatus == eMwError.error)
                        return ReturnErr("AA_CheckManageCredentials err: " + strErr);

                    // log & return
                    _rm.LogEvent(EventLogEntryType.Information, 0,
                        $"User {authInfo.Username}: logged-in - {authStatus}, manager - {authStatus == eMwError.Ok}", 3);
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr("DoAuthorize err: " + ex.Message);
            }

        }

        public HttpStatusCode SetDepartmentAutoLabels(string departmentName, string procedureType, List<AutoLabelInfo> labels)
        {
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(SetDepartmentAutoLabels));
                    }

                    if (mwProxy.Proxy.Department_SetAutoLabels(departmentName, procedureType, labels, out var strErr) != 0)
                    {
                        return ReturnErr($"{nameof(SetDepartmentAutoLabels)} err: " + strErr);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(SetDepartmentAutoLabels)} failed with err " + ex.Message);
            }
        }

        public HttpStatusCode GetDepartmentAutoLabels(string departmentName, string procedureType, out List<AutoLabelInfo> labels)
        {
            labels = null;
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(GetDepartmentAutoLabels));
                    }

                    if (mwProxy.Proxy.Department_GetAutoLabels(departmentName, procedureType, false, out labels, out var strErr) != 0)
                    {
                        return ReturnErr($"{nameof(GetDepartmentAutoLabels)} err: " + strErr);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(GetDepartmentAutoLabels)} failed with err " + ex.Message);
            }
        }

        public HttpStatusCode SetUserAutoLabels(string userName, string procedureType, List<AutoLabelInfo> labels)
        {
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(SetUserAutoLabels));
                    }

                    if (mwProxy.Proxy.Department_SetUserAutoLabels(userName, procedureType, labels, out var strErr) != 0)
                    {
                        return ReturnErr($"{nameof(SetUserAutoLabels)} err: " + strErr);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(SetUserAutoLabels)} failed with err " + ex.Message);
            }
        }

        public HttpStatusCode GetUserAutoLabels(string userName, string procedureType, out List<AutoLabelInfo> labels)
        {
            labels = null;
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(GetUserAutoLabels));
                    }

                    if (mwProxy.Proxy.Department_GetUserAutoLabels(userName, procedureType, false, out labels, out var strErr) != 0)
                    {
                        return ReturnErr($"{nameof(GetDepartmentAutoLabels)} err: " + strErr);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(GetDepartmentAutoLabels)} failed with err " + ex.Message);
            }
        }

        public HttpStatusCode GetActiveAutoLabels(string userName, string procedureType, out List<AutoLabelInfo> labels, out List<string> commonLabels)
        {
            labels = null;
            commonLabels = null;
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(GetActiveAutoLabels));
                    }

                    if (mwProxy.Proxy.Department_GetActiveAutoLabels(userName, procedureType, out labels, out var strErr) != 0)
                    {
                        return ReturnErr($"{nameof(GetActiveAutoLabels)} err: " + strErr);
                    }

                    if (mwProxy.Proxy.Department_GetActiveCommonAutoLabels(userName, out commonLabels, out strErr) != 0)
                    {
                        return ReturnErr($"{nameof(GetActiveAutoLabels)} err: " + strErr);
                    }

                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(GetActiveAutoLabels)} failed with err " + ex.Message);
            }
        }

        private HttpStatusCode ReturnErr(string strErr)
        {
            _rm.LogEvent(EventLogEntryType.Error, 0, strErr, 3);
            return HttpStatusCode.InternalServerError;
        }

        private HttpStatusCode ReturnOffline(string strMethod)
        {
            _rm.LogEvent(EventLogEntryType.Warning, 0, $"MwMgr failure in {strMethod} because middleware is offline", 3);
            return HttpStatusCode.ServiceUnavailable;
        }

        public HttpStatusCode AddProcType(string proc)
        {
            return AddProcType(proc, null);
        }

        public HttpStatusCode AddProcType(string proc, string username)
        {
            try
            {
                _rm.LogEvent(EventLogEntryType.Information, 0, $"AddProcType entered for {proc}", 3);
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                        return ReturnOffline("AddProcType");

                    // call mw
                    if (string.IsNullOrEmpty(username)) // generic
                    {
                        if (mwProxy.Proxy.Department_CreateProcType("*", proc, out string strErr) != 0)
                            return ReturnErr("Department_CreateProcType err: " + strErr);
                    }
                    else // user department
                    {
                        if (mwProxy.Proxy.Department_CreateUserProcType(username, proc, out string strErr) != 0)
                            return ReturnErr("Department_CreateProcType err: " + strErr);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr("Error: AddProcType failed with err " + ex.Message);
            }
        }

        public HttpStatusCode AddDepartmentProcType(string proc, string department)
        {
            try
            {
                _rm.LogEvent(EventLogEntryType.Information, 0, $"AddDepartmentProcType entered for proc: {proc}, department: {department}", 3);
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline("AddDepartmentProcType");
                    }

                    string mwDepartment = string.IsNullOrEmpty(department) ? "*" : department;
                    if (mwProxy.Proxy.Department_CreateProcType(mwDepartment, proc, out string strErr) != 0)
                    {
                        return ReturnErr("Department_CreateProcType err: " + strErr);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr("Error: AddProcType failed with err " + ex.Message);
            }

        }

        public HttpStatusCode GetDefineUserLabels(out bool defineSetting)
        {
            defineSetting = false;
            try
            {
                _rm.LogEvent(EventLogEntryType.Information, 0, "MwMgr.GetDefineUserLabels entered", 3);
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                        return ReturnOffline("AddProcType");

                    // call mw
                    return mwProxy.Proxy.Department_Get_Define_User_Labels_Settings(out defineSetting) != 0 ?
                        ReturnErr("Department_Get_Define_User_Labels_Settings error.") : HttpStatusCode.OK;
                }
            }
            catch (Exception e)
            {
                return ReturnErr("Error: MwMgr.GetDefineUserLabels failed with err " + e.Message);
            }
        }

        public HttpStatusCode GetAllLabels(out List<string> labels)
        {
            labels = null;
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(GetAllLabels));
                    }

                    if (mwProxy.Proxy.Department_GetAllLabels(out labels, out string err) != 0)
                    {
                        return ReturnErr($"{nameof(GetAllLabels)} err: " + err);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(GetAllLabels)} failed with err " + ex.Message);
            }
        }

        public HttpStatusCode GetAllProcTypes(out List<ProcedureTypeInfo> procTypes)
        {
            procTypes = null;
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(GetAllProcTypes));
                    }

                    if (mwProxy.Proxy.Department_GetAllProcTypes(out procTypes, out string err) != 0)
                    {
                        return ReturnErr($"{nameof(GetAllProcTypes)} err: " + err);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(GetAllProcTypes)} failed with err " + ex.Message);
            }
        }


        public HttpStatusCode GetCommonAutoLabels(string department, out List<string> labels)
        {
            labels = null;
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(GetCommonAutoLabels));
                    }

                    if (mwProxy.Proxy.Department_GetCommonAutoLabels(department, out labels, out string err) != 0)
                    {
                        return ReturnErr($"{nameof(GetCommonAutoLabels)} err: " + err);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(GetCommonAutoLabels)} failed with err " + ex.Message);
            }
        }

        public HttpStatusCode GetActiveCommonAutoLabels(string username, out List<string> labels)
        {
            labels = null;
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(GetActiveCommonAutoLabels));
                    }

                    if (mwProxy.Proxy.Department_GetActiveCommonAutoLabels(username, out labels, out string err) != 0)
                    {
                        return ReturnErr($"{nameof(GetActiveCommonAutoLabels)} err: " + err);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(GetActiveCommonAutoLabels)} failed with err " + ex.Message);
            }
        }

        public HttpStatusCode SetCommonAutoLabels(string department, List<string> labels)
        {
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(SetCommonAutoLabels));
                    }

                    if (mwProxy.Proxy.Department_SetCommonAutoLabels(department, labels, out string err) != 0)
                    {
                        return ReturnErr($"{nameof(SetCommonAutoLabels)} err: " + err);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(SetCommonAutoLabels)} failed with err " + ex.Message);
            }
        }

        public HttpStatusCode UploadLabelsCsv(Stream labelsCsv, bool replace)
        {
            try
            {
                // note - this ugliness is to keep O(1) lookups as we build the object graph
                ImplicitKeyedCollection<string, DepartmentInfo> departments = new ImplicitKeyedCollection<string, DepartmentInfo>(d => d.Name, StringComparer.OrdinalIgnoreCase);
                Dictionary<DepartmentInfo, ImplicitKeyedCollection<string, ProcTypeInfo>> departmentProcedures = new Dictionary<DepartmentInfo, ImplicitKeyedCollection<string, ProcTypeInfo>>();
                Dictionary<ProcTypeInfo, ImplicitKeyedCollection<string, AnnotationInfo>> procedureAnnotations = new Dictionary<ProcTypeInfo, ImplicitKeyedCollection<string, AnnotationInfo>>();

                using (CsvReader reader = new CsvReader(labelsCsv, new CsvHeaderParameters(labelHeaders, false), new CsvRowParseParameters(true, true)))
                {
                    Dictionary<string, string> row;
                    int i = 0;
                    while ((row = reader.ReadLineDictionary(out _)) != null)
                    {
                        i++;
                        if (false == row.TryGetValue(Department, out string department)
                            || false == row.TryGetValue(Right, out string right)
                            || false == row.TryGetValue(ProcedureType, out string procedureType)
                            || false == row.TryGetValue(Label, out string label))
                        {
                            throw new Exception($"Invalid csv format - row {i} is missing required data");
                        }

                        bool? isAutolabel = null;
                        if (row.TryGetValue(Autolabel, out string strAutolabel))
                        {
                            if (false == string.IsNullOrEmpty(strAutolabel))
                            {
                                if (bool.TryParse(strAutolabel, out bool result))
                                {
                                    isAutolabel = result;
                                }
                            }
                        }

                        department = string.IsNullOrWhiteSpace(department) ? "*" : department;
                        procedureType = string.IsNullOrWhiteSpace(procedureType) ? "*" : procedureType;
                        right = string.IsNullOrWhiteSpace(right) ? "*" : right;

                        if (false == string.IsNullOrWhiteSpace(label))
                        {
                            // check for department - if not found, create and add to various lookups
                            if (false == departments.TryGetValue(department, out DepartmentInfo curDepInfo))
                            {
                                curDepInfo = new DepartmentInfo(department, right, new List<ProcTypeInfo>());
                                departments.Add(curDepInfo);

                                departmentProcedures.Add(curDepInfo, new ImplicitKeyedCollection<string, ProcTypeInfo>(proc => proc.Name, StringComparer.OrdinalIgnoreCase));
                            }

                            var procTypeLookup = departmentProcedures[curDepInfo];

                            // check for procedure type (within department) - if not found, create and add to various lookups
                            if (false == procTypeLookup.TryGetValue(procedureType, out ProcTypeInfo curProcInfo))
                            {
                                curProcInfo = new ProcTypeInfo(procedureType, new List<UserAnnotations>());
                                curProcInfo.UserAnnotations.Add(new UserAnnotations("*", new List<AnnotationInfo>())); // note - this is ONLY supported in the absence of user-specific labels

                                curDepInfo.ProcedureTypes.Add(curProcInfo);

                                procTypeLookup.Add(curProcInfo);
                                procedureAnnotations.Add(curProcInfo, new ImplicitKeyedCollection<string, AnnotationInfo>(l => l.Value, StringComparer.Ordinal)); // note - supposing they could want different capitalizations here?
                            }

                            var labelLookup = procedureAnnotations[curProcInfo];

                            // check for label (within department + procedure type) - if not found, create and add to various lookups
                            if (false == labelLookup.TryGetValue(label, out AnnotationInfo curAnnotationInfo))
                            {
                                curAnnotationInfo = new AnnotationInfo(label, isAutolabel);
                                curProcInfo.UserAnnotations[0].Annotations.Add(curAnnotationInfo); // note - userannotations[0] is user="*" (because that's all we support via this call)

                                labelLookup.Add(curAnnotationInfo);
                            }
                        }
                    }
                }

                List<DepartmentInfo> finalData = departments.ToList();

                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(UploadLabelsCsv));
                    }

                    if (mwProxy.Proxy.Department_UpdateDepartmentLabels(finalData, replace, out string err) != 0)
                    {
                        return ReturnErr($"{nameof(UploadLabelsCsv)} err: " + err);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(UploadLabelsCsv)} failed with err " + ex.Message);
            }
        }

        public HttpStatusCode AddDepartmentAutolabel(string departmentName, string procedureType, string label, int? position, string color)
        {
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(AddDepartmentAutolabel));
                    }

                    if (mwProxy.Proxy.Department_AddDepartmentAutolabel(departmentName, procedureType, label, position, color, out string err) != 0)
                    {
                        return ReturnErr($"{nameof(AddDepartmentAutolabel)} err: " + err);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(AddDepartmentAutolabel)} failed with err " + ex.Message);
            }
        }
        public HttpStatusCode AddUserAutolabel(string userName, string procedureType, string label, int? position, string color)
        {
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(AddUserAutolabel));
                    }

                    if (mwProxy.Proxy.Department_AddUserAutolabel(userName, procedureType, label, position, color, out string err) != 0)
                    {
                        return ReturnErr($"{nameof(AddUserAutolabel)} err: " + err);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(AddUserAutolabel)} failed with err " + ex.Message);
            }
        }
        
        public HttpStatusCode EditDepartmentLabel(string departmentName, string procedureType, string oldLabel, string newLabel, bool? newIsAutolabel, int? newAutolabelPosition, string newAutolabelColor)
        {
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(EditDepartmentLabel));
                    }

                    if (mwProxy.Proxy.Department_EditDepartmentLabel(departmentName, procedureType, oldLabel, newLabel, newIsAutolabel, newAutolabelPosition, newAutolabelColor, out string err) != 0)
                    {
                        return ReturnErr($"{nameof(EditDepartmentLabel)} err: " + err);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(EditDepartmentLabel)} failed with err " + ex.Message);
            }
        }
        public HttpStatusCode EditUserLabel(string userName, string procedureType, string oldLabel, string newLabel, bool? newIsAutolabel, int? newAutolabelPosition, string newAutolabelColor)
        {
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(EditUserLabel));
                    }

                    if (mwProxy.Proxy.Department_EditUserLabel(userName, procedureType, oldLabel, newLabel, newIsAutolabel, newAutolabelPosition, newAutolabelColor, out string err) != 0)
                    {
                        return ReturnErr($"{nameof(EditUserLabel)} err: " + err);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(EditUserLabel)} failed with err " + ex.Message);
            }
        }
        
        public HttpStatusCode DeleteDepartmentLabel(string departmentName, string procedureType, string label)
        {
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(DeleteDepartmentLabel));
                    }

                    if (mwProxy.Proxy.Department_DeleteDepartmentLabel(departmentName, procedureType, label, out string err) != 0)
                    {
                        return ReturnErr($"{nameof(DeleteDepartmentLabel)} err: " + err);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(DeleteDepartmentLabel)} failed with err " + ex.Message);
            }
        }
        public HttpStatusCode DeleteUserLabel(string userName, string procedureType, string label)
        {
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(DeleteUserLabel));
                    }

                    if (mwProxy.Proxy.Department_DeleteLabel(userName, procedureType, label, out string err) != 0)
                    {
                        return ReturnErr($"{nameof(DeleteUserLabel)} err: " + err);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(DeleteUserLabel)} failed with err " + ex.Message);
            }
        }

        public HttpStatusCode DeleteLabelGlobal(string label)
        {
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return ReturnOffline(nameof(DeleteLabelGlobal));
                    }

                    if (mwProxy.Proxy.Department_DeleteLabelGlobal(label, out string err) != 0)
                    {
                        return ReturnErr($"{nameof(DeleteLabelGlobal)} err: " + err);
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr($"{nameof(DeleteLabelGlobal)} failed with err " + ex.Message);
            }
        }

    }
}
