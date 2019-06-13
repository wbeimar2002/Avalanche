using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Xml.Linq;
using IsmLogCommon;
using IsmUtility;
using ISM.LibrarySi;

namespace MonsoonAPI.Controllers.v1
{
    public class VersionsController : ControllerBase
    {
        private readonly IConfiguration _cfg;

        public VersionsController(IMonsoonResMgr resMgr, IConfiguration cfg, INodeMgr ndMgr) : base("Versions", resMgr,
            ndMgr, cfg)
        {
            _cfg = cfg;
        }

        [HttpGet]
        public async Task<IActionResult> GetVersions()
        {
            const string method = "GetVersions";
            try
            {
                LogEnter(method);
                // get library versions
                Dictionary<EComponents, string> libVersions = null; // lib, mw, pie, ISS, compatibility, LSS, TelemetryAgent
                using (var lib = Rm.GetLibProxy(true))
                {
                    string err = string.Empty;
                    if (lib?.Proxy.GetVersions(out  libVersions, out err) != 0)
                        Rm.LogEvent(EventLogEntryType.Warning, 0, "Failed to get lib versions w/err " + err,3);
                }

                // start building versions to return
                var versionsToReturn = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var appPath =  Path.GetPathRoot(Assembly.GetEntryAssembly().Location);
                var webRoot = getLocalWwwRootPath(appPath);
                if (string.IsNullOrEmpty(webRoot) || !Directory.Exists(webRoot))
                {
                    IsmLog.LogEvent(EventLogEntryType.Warning, 0, "Web root cannot be read. Web component versions will not be reported", 3);
                    webRoot = string.Empty;
                }

                if (Rm.MonsoonCfg != EMonsoonConfig.EasyView)
                {
                    AddNCareVersions(versionsToReturn, libVersions);
                }
                else
                {
                    AddEasyViewVersions(versionsToReturn, libVersions, webRoot);
                }
             

                versionsToReturn["Web_API"] = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
                versionsToReturn["Web_App"] =  getMonsoonWebVersion(webRoot);

                versionsToReturn["IP_Address"] = Communications.GetMyIPv4Address();
                versionsToReturn["SystemOS"] = getOSInfo();

                return await ReturnOk(method, versionsToReturn);

            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
  
        }

        private void AddEasyViewVersions(Dictionary<string, string> versionsToReturn, 
            IReadOnlyDictionary<EComponents, string> libVersions, string webRoot)
        {
            // get everything we can from library
            if (libVersions != null)
            {
                var libVersionsMap = libVersions.ToDictionary(vers => ComponentToName(vers.Key), vers => vers.Value, StringComparer.OrdinalIgnoreCase);
                versionsToReturn = versionsToReturn.Concat(libVersionsMap)
                    .ToDictionary(vers => vers.Key, vers => vers.Value, StringComparer.OrdinalIgnoreCase);
            }

            // Add Live Stream Portal - from disk
            versionsToReturn["Live Stream Portal"] = GetVersion(webRoot, @"LSP\bin\LiveStreamPortal.dll");

            // client comp
            var clientComp = GetIsmClientCompVersion();
            if (!string.IsNullOrEmpty(clientComp))
                versionsToReturn["Ism Client Components"] = clientComp;

            // others
            versionsToReturn["Log"] = Rm.GetLogVersion();
        }

        private void AddNCareVersions(Dictionary<string, string> versionsToReturn, IReadOnlyDictionary<EComponents, string> libVersions)
        {
            // serial number - unless EasyViwe
            versionsToReturn["Serial_Number"] = _cfg.GetValue<string>("serial_number");

            // some versions (many) come from IsmRec
            using (var ismRec = Rm.GetIsmRecProxy())
            {
                if (ismRec != null)
                {
                    if (ismRec.Proxy.GetVersions(out var recVersions, out _) == 0)
                    {
                        versionsToReturn = versionsToReturn.Concat(recVersions) // ismRec, mw, Library, Log, Client Comp, Video/Live previews, FPGA, SD
                            .ToDictionary(v => v.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
            if (libVersions != null)
            {
                var compNameToFriendly = new Dictionary<EComponents, string>
                {
                    [EComponents.StateServer] = "State Server",
                    [EComponents.PatInfoEngine] = "Patient Information Engine",
                    [EComponents.Telemetry] = "Telemetry Agent",
                    [EComponents.DeviceMgmt] = "Device Management",
                    [EComponents.Provisioner] = "Provisioner"
                };

                foreach (var comp2Friendly in compNameToFriendly)
                {
                    if (versionsToReturn.ContainsKey(comp2Friendly.Value))
                    {
                        versionsToReturn[comp2Friendly.Value] = libVersions[comp2Friendly.Key];
                    }
                    else if (libVersions.ContainsKey(comp2Friendly.Key))
                    {
                        versionsToReturn[comp2Friendly.Value] = libVersions[comp2Friendly.Key];
                    }

                }
            }
        }

        private static string GetVersion(string root, string filePath)
        {
            var fullFilePath  = Path.Combine(root, filePath);
            if (!System.IO.File.Exists(fullFilePath))
            {
                IsmLog.LogEvent(EventLogEntryType.Warning, 0, $"File {fullFilePath} not found. Version will not be reported.", 3);
                return "N/A";
            }

            return FileVersionInfo.GetVersionInfo(fullFilePath).ProductVersion;

        }



        private string ComponentToName(EComponents comp)
        {
            switch (comp)
            {
                case EComponents.StateServer: return "State_Server";
                case EComponents.PatInfoEngine: return "Patient_Information_Engine";
                case EComponents.LSS: return "Live_Stream_Service";
                case EComponents.Telemetry: return "Telemetry_Agent";
                default: return comp.ToString();
            }
        }

        const string kIsmClientVersion = "IsmClientComponents version ";
        private string GetIsmClientCompVersion()
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"..\");
                var version = Path.Combine(di.FullName, @"Program Files\Common Files\VideoDecoder\Versions.txt");
                if (!System.IO.File.Exists(version))
                {
                    version = @"C:\Program Files\Common Files\VideoDecoder\Versions.txt";
                }
                if (!System.IO.File.Exists(version))
                {
                    return null;
                }

                using (TextReader reader = new StreamReader(version)) // open up version file
                {
                    string str;
                    while ((str = reader.ReadLine()) != null) // read all lines - though version should be the 2nd one on the list
                    {
                        if (str.Contains(kIsmClientVersion))
                        { // yay! We found the versions line!
                            int nStartIndex = str.IndexOf(kIsmClientVersion, StringComparison.CurrentCulture) + kIsmClientVersion.Length; // where in the line should we look?
                            return str.Substring(nStartIndex); // strip off "IsmClientComponents version ", get the rest
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Rm.LogEvent(EventLogEntryType.Error, 0, "GetIsmClientCompVersion err: " + ex.Message, 3);
            }
            return null;
        }

        private string getLocalWwwRootPath(string appPath)
        {
            // if I am not an EasyView, then it's c:\inetpub
            if (Rm.MonsoonCfg != EMonsoonConfig.EasyView)
                return @"C:\inetpub\wwwroot";

            // else, find vss.xml on my drive & read from that
            var vssCfg = Path.Combine(new[] { appPath, "VaultStreamConfigurator", "cfg", "vss.xml"});
            if (System.IO.File.Exists(vssCfg))
            {
                var doc = XDocument.Load(vssCfg);
                return doc.Root?.Element("locations")?.Element("WebPath")?.Value;
            }

            // can't find vss.xml, so just assume its ISM_Web on my drive
            return Path.Combine(appPath, "ISM_Web");
        }


        const string kMonsoonVersionFile = @"monsoon\include\version.txt";
        private string getMonsoonWebVersion(string wwwRootFolder)
        {
            try
            {
                string versionFile = Path.Combine(wwwRootFolder, kMonsoonVersionFile);
                if (!System.IO.File.Exists(versionFile))
                {
                    Rm.LogEvent(EventLogEntryType.Error, 0, $"{versionFile} does not exist", 3);
                    return "N/A";
                }

                using (TextReader versionReader = new StreamReader(versionFile))
                {
                    return versionReader.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Rm.LogEvent(EventLogEntryType.Error, 0, "getMonsoonWebVersion err: " + ex.Message, 3);
                return "N/A";
            }
        }

        /// <summary>
        /// OSVersion gives a very user-unfriendly string (such as Win32NT for Windows 7.0 Professional); this method translates version into a friendlier string
        /// </summary>
        /// <returns></returns>
        private string getOSInfo()
        {
            try
            {
                var result = string.Empty;
                using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
                {
                    using (var collection = searcher.Get())
                    {
                        foreach (var o in collection)
                        {
                            try
                            {
                                var xos = (ManagementObject)o;
                                result = xos["Caption"].ToString();
                            }
                            finally
                            {
                                o.TryDispose(Rm.LogEvent);
                            }

                            break;
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                Rm.LogEvent(EventLogEntryType.Error, 0, "getOSInfo error: " + e.Message, 1);
                return "Unknown";
            }
        }
    }
}
