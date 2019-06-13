using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Security;
using System.Xml;
using IsmLogCommon;
using ISM.Middleware2Si;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MonsoonAPI.Controllers.Compatibility
{
    public class CompatibilityGlobal
    {
        private IMonsoonResMgr m_Rm;
        private Timer m_Timer = null;
        private DateTime m_dtLastMonsoonApiPing = DateTime.MinValue;

        public  string SESSION_USER_NAME = "UserName";
        public  string SESSION_CREDENTIALS = "Credentials";
       
        public object LockObj { get; }

        public Dictionary<string, DateTime> UploadingFileTimes { get; }

        public Dictionary<string, BinaryWriter> UploadingFiles { get; }

        public CompatibilityGlobal(IMonsoonResMgr rm, IConfiguration cfg, IHttpContextAccessor httpContextAccessor,
            INodeMgr ndMgr)
        {
            UploadingFiles = new Dictionary<string, BinaryWriter>(StringComparer.OrdinalIgnoreCase);
            UploadingFileTimes = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            LockObj = new object();
            m_Rm = rm;
            m_Rm.Init(cfg, ndMgr);
            m_Timer = new System.Timers.Timer(10000);
            m_Timer.Elapsed += MTimerOnElapsed;
            m_Timer.Start();
        }

        private void MTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                CheckOpenFiles();
                if ((DateTime.UtcNow - m_dtLastMonsoonApiPing).TotalMinutes >= 10)
                {
                    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://localhost:5000/vip/webutils.aspx?command=ping");
                    try
                    {
                        req.Headers["command"] = "ping";
                        req.GetResponse();
                        m_Rm.LogEvent(EventLogEntryType.Information, 0, "MonsoonApi.Ping succeeded", 4);
                    }
                    catch
                    {
                        m_Rm.LogEvent(EventLogEntryType.Information, 0, "MonsoonApi.Ping failed", 4);
                    }
                    m_dtLastMonsoonApiPing = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "MTimerOnElapsed err: " + ex.Message, 3);
            }
        }

        /// <summary>
        /// Authenticate user (using credentials if passed in or login/pw otherwise) and verify user has libray access
        /// </summary>
        public HttpStatusCode DoLogIn(ref string userLogin, string userPass, string strUserHost, ref string userCredentials, out string strErr)
        {
            m_Rm.LogEvent(EventLogEntryType.Information, 0, "DoLogIn entered for user " + userLogin, 4);
            strErr = string.Empty;

            //try { Global.RemoteObjsOnline(true, true); }
            //catch { return System.Net.HttpStatusCode.ServiceUnavailable; }

            //24384 user login can be null or empty
            var accessLogUserName = userLogin;
            if (string.IsNullOrWhiteSpace(accessLogUserName))
            {
                MwUtils.LoginFromCredentials(userCredentials, out accessLogUserName, out strErr);
                if (string.IsNullOrWhiteSpace(accessLogUserName))
                    accessLogUserName = "Unknown Compatibility UserName";
            }

            var accessInfo = new AccessInfo(strUserHost, accessLogUserName, "MonsoonWebCompatibility", Environment.MachineName, nameof(DoLogIn));
            try
            {
                // we'd like tohave both credentials string and user login
                if (string.IsNullOrEmpty(userCredentials))
                    userCredentials = MwUtils.Credentials(userLogin, userPass);
                else if (string.IsNullOrEmpty(userLogin))
                    MwUtils.LoginFromCredentials(userCredentials, out userLogin, out strErr);

                using (var mw = m_Rm.GetMwProxy())
                {

                    if (mw.Proxy.AA_Authenticate(userCredentials, accessInfo, out strErr) != eMwError.Ok)
                    {
                        strErr = "Invalid credentials. ";
                        return HttpStatusCode.NotAcceptable;
                    }
                }

                // if no lib access - don't bother
                //TODO do we want to log access here? should whoever performs the authentication do the logging?
                HttpStatusCode ret = UserHasLibraryAccess(userLogin);
                if (ret != HttpStatusCode.OK && ret != System.Net.HttpStatusCode.Accepted)
                {
                    strErr = $"User has no library access from {strUserHost}";
                    m_Rm.LogAccessEvent(new LoginFailureAccessDetails(userLogin), accessInfo, $"User has no library access from {strUserHost}");
                }
                else
                {
                    // set session variables
                    m_Rm.LogAccessEvent(new LoginSuccessAccessDetails(userLogin), accessInfo, $"User logged into VIP room from {strUserHost}");
                   // FormsAuthentication.SetAuthCookie(userLogin, false, "/");
                }

                return ret;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "DoLogIn error " + ex.Message, 1);
                strErr = $"User has no library access from {strUserHost}";
                m_Rm.LogAccessEvent(new LoginFailureAccessDetails(userLogin), accessInfo, $"User has no library access from {strUserHost}");

                return System.Net.HttpStatusCode.InternalServerError;
            }
        }

        private HttpStatusCode UserHasLibraryAccess(string usetLogin)
        {
            try
            {
                using (var lib = m_Rm.GetLibProxy())
                {
                    lib.Proxy.GetLibrariesForUser(usetLogin, out var libs, out _);
                    if (!libs.Any())
                    {
                        m_Rm.LogEvent(EventLogEntryType.Information, 0, "GetLibrariesForUser returned empty string", 3);
                        return HttpStatusCode.Unauthorized;
                    }

                    return HttpStatusCode.Accepted;
                }
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Information, 0, "UserHasLibraryAccess exception - " + ex.Message, 3);
                return HttpStatusCode.InternalServerError;
            }
        }

        public void SetCredentials(string strCredentials, string strUserName, HttpContext context)
        {
            try
            {
                context.Session.SetString(SESSION_CREDENTIALS, strCredentials);
                context.Session.SetString(SESSION_USER_NAME, strUserName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void CheckOpenFiles()
        {
            lock (LockObj)
            {
                try
                {
                    Dictionary<string, DateTime> uploadingFileTimes = UploadingFileTimes;
                    Dictionary<string, System.IO.BinaryWriter> uploadingFiles = UploadingFiles;
                    if (uploadingFileTimes == null || uploadingFiles == null)
                        return;

                    string[] filesNames = new string[uploadingFileTimes.Count];
                    uploadingFileTimes.Keys.CopyTo(filesNames, 0);
                    for (int nFile = 0; nFile < filesNames.Length; nFile++)
                    {
                        string strFileName = filesNames[nFile];
                        DateTime dtTimestamp = uploadingFileTimes[strFileName];
                        TimeSpan tsFromUpdate = DateTime.UtcNow - dtTimestamp;
                        if (tsFromUpdate.TotalSeconds < 30)
                            continue; // no need to close

                        try
                        {
                            m_Rm.LogEvent(EventLogEntryType.Information, 0,
                                string.Format("About to close file {0}; seconds from last update - {1}", System.IO.Path.GetFileName(strFileName), tsFromUpdate.TotalSeconds), 3);

                            // if we can't find the writer, don't bother
                            if (uploadingFiles.ContainsKey(strFileName))
                            {
                                // close
                                System.IO.BinaryWriter writer = uploadingFiles[strFileName];
                                writer.Close();
                                uploadingFiles.Remove(strFileName);
                            }
                            else
                                m_Rm.LogEvent(EventLogEntryType.Information, 0, "Unable to close file because it is not in the list.", 3);
                        }
                        catch (Exception ex)
                        {
                            m_Rm.LogEvent(EventLogEntryType.Error, 0,
                                $"WebUtils.FileCheckTimer_Tick encountered error while processing {strFileName}. Err - {ex.Message}",1);
                        }

                        uploadingFileTimes.Remove(strFileName);
                        m_Rm.LogEvent(EventLogEntryType.Information, 0, "On timer, the following file has been removed from list: " + strFileName, 3);
                    }
                }
                catch (Exception ex)
                {
                    m_Rm.LogEvent(EventLogEntryType.Error, 0, "WebUtils.FileCheckTimer_Tick error " + ex.Message,1);
                }
            }
        }
    }
}
