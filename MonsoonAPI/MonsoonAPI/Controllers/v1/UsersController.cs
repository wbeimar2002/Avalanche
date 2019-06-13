using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ISM.Middleware2Si;
using IsmUtility;
using MonsoonAPI.models;
using MsntSi.Types;
using System.Linq;
using System.Text;
using System.Net;

namespace MonsoonAPI.Controllers.v1
{
    public class UsersController : ControllerBase
    {
        readonly IMwMgr _mwMgr;

        public UsersController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IConfiguration cfg, IMwMgr mwMgr) 
            : base("Users", resMgr,nodeMgr, cfg) => _mwMgr = mwMgr;


        [HttpPost]
        public async Task<IActionResult> Auth([FromBody]AuthInfo authInfo)
        {
            const string method = "Auth";
            try
            {
                LogEnter(method);
                HttpStatusCode ret = _mwMgr.DoAuthorize(authInfo, out var authStatus, out var username);
                return await ReturnCode(ret, new {  authStatus, username }, method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpDelete("{login}")]
        public async Task<IActionResult> DeleteUser(string login)
        {
            var method = $"DeleteUser/{login}";
            try
            {
                LogEnter(method);
                using (var mwProxy = Rm.GetMwProxy())
                {
                    if (mwProxy == null)
                        return await ReturnOffline(eSystemType.middleware, method);

                    // call mw
                    if (mwProxy.Proxy.Users_DeleteUser(login, out var strErr) != 0)
                        return await ReturnError(method, "DeleteUser err: " + strErr);

                    return await ReturnOk(method);
                }
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpGet("{login}")]
        public async Task<IActionResult> GetUser(string login)
        {
            string method = $"GetUser/{login}";
            try
            {
                LogEnter(method);
                string xml;
                using (var mwProxy = Rm.GetMwProxy())
                {
                    if (mwProxy == null)
                        return await ReturnOffline(eSystemType.middleware, method);

                    // call mw
                    if (mwProxy.Proxy.Users_GetWithOptions(login, new[] { EFields.is_modifiable }, out xml, out var strErr) != 0)
                        return await ReturnError(method, "Users_GetXml err: " + strErr);
                }

                // Load & return
                var usr = XElement.Parse(xml);
                MonsoonUserInfo user = new MonsoonUserInfo
                {
                    m_strLogin = XmlUtils.El2String(usr, "login"),
                    m_strFirstName = XmlUtils.El2String(usr, "fname", string.Empty),
                    m_strLastName = XmlUtils.El2String(usr, "lname"),
                    m_bModifiable = XmlUtils.El2Bool(usr, "is_modifiable"),
                    rights = usr.Element("rightsoverride")?.Elements("right").Select(right => right.Attribute("name")?.Value)
                };
                
                return await ReturnOk(method, user);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
         
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            const string method = "GetUsers";
            try
            {
                LogEnter(method);
                using (var mwProxy = Rm.GetMwProxy())
                {
                    if (mwProxy == null)
                        return await ReturnOffline(eSystemType.middleware, method);

                    // call mw
                    if (mwProxy.Proxy.Users_GetList(new[] { EUserType.person }, new[] { "is_modifiable" }, out var xml, out var strErr) != 0)
                        return await ReturnError(method, "Users_GetAll err: " + strErr);
                

                    // Load & return
                    XElement userList = XElement.Parse(xml);
                    return await ReturnOk(method, new {  user_list = userList });
                }
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }

        }

        [HttpPut]
        public async Task<IActionResult> SetUser([FromBody]MonsoonUserInfo userInfo)
        {
            const string method = "SetUser";
            try
            {
                LogEnter(method);

                XElement elUser = new XElement("user",
                                    new XElement("login", userInfo.m_strLogin),
                                    new XElement("lname", userInfo.m_strLastName),
                                    new XElement("fname", userInfo.m_strFirstName),
                                    new XElement("rightsoverride",
                                        userInfo.rights?.Select(right => new XElement("right", new XAttribute("name", right)))));

                if (userInfo.pwd != null) // note that setting to empty string is 100% OK!!!
                {
                    var  pw = Encoding.UTF8.GetString(Convert.FromBase64String(userInfo.pwd));
                    pw = MwUtils.CreateHashedPassword(pw);
                    elUser.Add(new XElement("pwd", pw));
                }

                using (var mwProxy = Rm.GetMwProxy())
                {
                    if (mwProxy == null)
                        return await ReturnOffline(eSystemType.middleware, method);

                    var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(HttpContext);

                    // call mw
                    if (mwProxy.Proxy.Users_Copy(userInfo.m_strLogin, elUser.ToString(), MwUtils.SystemCredentials, accessInfo, out var strErr) != 0)
                        return await ReturnError(method, "Users_Copy err: " + strErr);

                    return await ReturnOk(method);
                }
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
           
        }

    }
}
 
