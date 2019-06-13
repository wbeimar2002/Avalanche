using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using IsmLogCommon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MonsoonAPI.Models;

namespace MonsoonAPI.Controllers.v1
{
    public class LibraryController : ControllerBase
    {
        readonly ILibMgr _libMgr;
        public LibraryController(IMonsoonResMgr rm, INodeMgr nodeMgr, IConfiguration cfg, ILibMgr libMgr, IMwMgr mwMgr) :
            base("Library", rm,  nodeMgr, cfg)
        {
            _libMgr = libMgr;
        }


        //[HttpGet("{login}")]
        //public async Task<IActionResult> Search(string login)
        //{
        //    var method = $"Search/{login}";
        //    try
        //    {
        //        LogEnter(method);
        //        HttpStatusCode retCode = _libMgr.SearchLib(login, out var searchRes);
        //        return await ReturnCode(retCode, searchRes, method);
        //    }
        //    catch (Exception e)
        //    {
        //        return await ReturnError(method, e.Message);
        //    }
        //}

        //[HttpGet("{login}/{keyword}")]
        //public async Task<IActionResult> Search(string login, string keyword)
        //{
        //    var method = $"Search/{login}/{keyword}";
        //    try
        //    {
        //        LogEnter(method);
        //        HttpStatusCode retCode = _libMgr.SearchLib(login, keyword, out var searchRes);
        //        return await ReturnCode(retCode, searchRes, method);
        //    }
        //    catch (Exception e)
        //    {
        //        return await ReturnError(method, e.Message);
        //    }
        //}

        //[HttpGet("{login}/{keyword}/{startDate}/{endDate}")]
        //public async Task<IActionResult> Search(string login, string keyword, string startDate, string endDate)
        //{
        //    var method = $"Search/{login}/{keyword}/{startDate}/{endDate}";
        //    try
        //    {
        //        LogEnter(method);
        //        HttpStatusCode retCode = _libMgr.SearchLib(login, keyword, startDate, endDate, out var searchRes);
        //        return await ReturnCode(retCode, searchRes, method);
        //    }
        //    catch (Exception e)
        //    {
        //        return await ReturnError(method, e.Message);
        //    }
        //}

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SearchCriteria criteria)
        {
            const string method = nameof(Search);

            try
            {
                LogEnter(method);
                var retCode = _libMgr.SearchLibrary(criteria, out var results, CultureInfo.CurrentCulture);
                return await ReturnCode(retCode, results, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetRecFromRedirect([FromBody] string eargs)
        {
            const string method = "GetRecFromRedirect";
            try
            {
                LogEnter(method);
                HttpStatusCode ret = _libMgr.ProcessRedirect(eargs, out var obj, CultureInfo.CurrentCulture);
                return await ReturnCode(ret, obj, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }
    }
}