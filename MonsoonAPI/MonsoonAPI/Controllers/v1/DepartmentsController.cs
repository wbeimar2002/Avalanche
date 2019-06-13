using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MonsoonAPI.Models;
using MsntSi.Types;

namespace MonsoonAPI.Controllers.v1
{
    public class DepartmentsController : ControllerBase
    {
        private readonly IMwMgr _mwMgr;

        public DepartmentsController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IConfiguration cfg, IMwMgr mwMgr) :
            base("Departments", resMgr, nodeMgr, cfg)
        {
            _mwMgr = mwMgr;
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            const string method = "GetDepartments";
            try
            {
                LogEnter(method);
                if ( _mwMgr.GetDepartments(out var departments) != HttpStatusCode.OK)
                    return await ReturnError(method, "GetDepartments failed");

                return  await ReturnOk(method, departments);
            }
            catch (Exception e)
            {
                return await ReturnError(method, "GetDepartments exception " + e.Message);
            }
        }

        [HttpPost("addDepartment")]
        public async Task<IActionResult> AddDepartment([FromBody] AddDepartmentRequest department)
        {
            var method = $"AddDepartment";
            try
            {
                LogEnter(method);
                using (var mwProxy = Rm.GetMwProxy())
                {
                    if (mwProxy == null)
                    {
                        return await ReturnOffline(eSystemType.middleware, method);
                    }
                    if (0 != mwProxy.Proxy.Department_CreateDepartment(department.Name, department.Right, out string err))
                    {
                        return await ReturnError(method, "Department_CreateDepartment err: " + err);
                    }
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