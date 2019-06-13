using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.ServiceModel.Configuration;
using Microsoft.Extensions.Configuration;
using IsmLogCommon;
using IsmUtility;
using IsmStateServer;
using ISM.Middleware2Si;
using ISM.Library.Types;
using ISM.LibrarySi;
using IsmRec.Types;
using IsmRecSi;
using IsmStateServer.Types;
using PatInfoEngineSi;
using IIsmLivePreview;
using LssSi;

namespace MonsoonAPI {
    public enum EMonsoonConfig { Unknown, EasyView, Standalone, Connected };
    public enum ESuccess { Ok = 0, Error, PartialRetrieve, Offline };

    public class ResMgr : IMonsoonResMgr, IssCallBackContract, IsmLogCallBackContract
    {

        #region general attributes
        private readonly LogClient _logClient;
        private readonly LogClient _logClientWeb;
        private INodeMgr _nodeMgr;
        #endregion  

        #region configuration
        private bool _bStateCacheRetrieved;
        private Tuple<string, int> _recIpPort;
        private Tuple<string, int> _remLibIpPort;
        private int _remLibFileTransfPort = -1;
        private int _lclLibFileTransfPort = -1;
        private Tuple<string, int> _localLibIpPort;
        private Tuple<string, int> _mwIpPort;
        private Tuple<string, int> _pieIpPort;
        private Tuple<string, int> _lssIpPort;
        private List<PrintLayoutInfo> _printLayouts;
        private RecorderSettings _recSettings;
        private string _easyViewHost;
        private string _cultureName;
        #endregion

        public ResMgr()
        {
            try
            {
                _logClient = new LogClient("MonsoonApi");
                _logClientWeb = new LogClient("MonsoonWeb");

                if (!IsmLog.IsInit)
                    IsmLog.InitLog(null);
            }
            catch (Exception ex)
            {
                LogEvent(EventLogEntryType.Error, 0, $"ResMgr constructor failed with err {ex.Message}", 1);
                throw new Exception("ResMgr unable to initialize!");
            }
        }

        public void Init(IConfiguration cfg, INodeMgr nodeMgr)
        {
            try
            {
                // init that only happens once
                if (Cfg == null)
                {
                    Cfg = cfg;
                    _nodeMgr = nodeMgr; // note that this could very well be null - on VS

                    // logging
                    int nLogVerb = cfg.GetValue<int>("log_verbosity");
                    _logClient.LogVerbosity = nLogVerb;
                    LogEvent(EventLogEntryType.Information, 0, "MonsoonAPI Initializing", 3);

                    MyIp = Communications.GetMyIPv4Address();
                    LogEvent(EventLogEntryType.Information, 0, "My ip is " + MyIp, 3);
                }

                // initialize state cache if we haven't done so yet
                if (!_bStateCacheRetrieved)
                    RefreshStateCache();
            }
            catch (Exception ex)
            {
                LogEvent(EventLogEntryType.Error, 0, "ResMgr.Init err: " + ex.Message, 1);
            }
        }
        public string GetLogVersion()
        {
            try
            {
                using (var log = new WcfOneTimeUseDuplexClientWrapper<ILogService, ResMgr>(this, "127.0.0.1", LogUtility.kLogPort, "IsmLogService"))
                {
                    return log.Proxy.GetVersion();
                }
            }
            catch (Exception ex)
            {
                LogEvent(EventLogEntryType.Error, 0, "GetLogVersion err: " + ex.Message, 3);
                return "Unknown";
            }
        }

        public bool IsPieLocalEnabled => IsConfigured(_pieIpPort.Item1) && _pieIpPort.Item1.Equals("127.0.0.1");

        /// <summary>
        /// Returns true if pie's ip is not 0.0.0.0
        /// </summary>
        public bool IsPieConfigured => IsConfigured(_pieIpPort.Item1);
        
        /// <summary>
        /// Gets the PIE proxy or returns null
        /// </summary>
        /// <returns></returns>
        public WcfOneTimeUseDuplexClientWrapper<IPatInfoEngineSi, ResMgr> GetPieProxy()
        {
            try
            {
                if (!IsConfigured(_pieIpPort.Item1))
                    return null;

                // check w/IsmRec to see if it's online - of we are connected
                if (MonsoonCfg == EMonsoonConfig.Connected)
                {
                    using (var ismRecProxy = GetIsmRecProxy())
                    {
                        ismRecProxy.Proxy.GetPieState(out var remPieState);
                        if (remPieState != MsntSi.Types.eSystemState.online)
                        {
                            LogEvent(EventLogEntryType.Warning, 0, "PIE is not online", 3);
                            return null;
                        }
                    }
                }

                return new WcfOneTimeUseDuplexClientWrapper<IPatInfoEngineSi, ResMgr>(this, _pieIpPort.Item1,
                    _pieIpPort.Item2, "PatInfoEngine");
            }
            catch (Exception ex)
            {
                LogEvent(EventLogEntryType.Error, 0, $"Failed to GetPieProxy: {ex.Message}", 1);
                return null;
            }
        }

        public WcfOneTimeUseDuplexClientWrapper<IIss, ResMgr> GetIssProxy()
        {
            try
            {
                // state cache info
                var strIssIpPort = Cfg.GetValue<string>("iss_ip_port");

                // init
                Communications.ParseIpPort(strIssIpPort, out string ip, out int port);

                return new WcfOneTimeUseDuplexClientWrapper<IIss, ResMgr>(this, ip, port, "IsmStateServer");
            }
            catch (Exception ex)
            {
                LogEvent(EventLogEntryType.Error, 0, $"GetIssProxy err: {ex.Message}", 3);
                return null;
            }
        }

        public void RefreshStateCache()
        {
            try
            {

                // we are connected, so refresh cache
                IssDataCodes[] lstCodes =
                {
                    IssDataCodes.library_ip_port,
                    IssDataCodes.middleware_ip_port,
                    IssDataCodes.pat_info_ip_port,
                    IssDataCodes.recorder_ip_port,
                    IssDataCodes.remote_library_ip_port,
                    IssDataCodes.library_data_usable,
                    IssDataCodes.room_name,
                    IssDataCodes.easy_view_ip,
                    IssDataCodes.culture,
                    IssDataCodes.lss_ip_port
                };
                switch (GetIssDataMap(lstCodes, out var stateCacheData))
                {
                    case ESuccess.PartialRetrieve:
                        _bStateCacheRetrieved = false;
                        break; // we'll process what we can, but want to pull again later
                    case ESuccess.Ok:
                        _bStateCacheRetrieved = true;
                        break;
                    default:
                        LogEvent(EventLogEntryType.Error, 0, "GetIssDataMap failed", 1);
                        return;
                }

                // mtl
                if (stateCacheData.TryGetValue(IssDataCodes.room_name, out var obj) && obj != null)
                    RoomId = (string)obj;

                // Easy View Host
                if (stateCacheData.TryGetValue(IssDataCodes.easy_view_ip, out obj) && obj != null)
                    _easyViewHost = (string)obj;

                // culture
                if (stateCacheData.TryGetValue(IssDataCodes.culture, out obj) && obj != null)
                    _cultureName = (string)obj;
                
                // init recorder
                if (stateCacheData.TryGetValue(IssDataCodes.recorder_ip_port, out obj) && obj != null)
                {
                    Communications.ParseIpPort((string)obj, out string strRecIp, out int nRecPort);
                    _recIpPort = new Tuple<string, int>(strRecIp, nRecPort);
                }

                // mw
                if (stateCacheData.TryGetValue(IssDataCodes.middleware_ip_port, out obj) && obj != null)
                {
                    Communications.ParseIpPort((string)obj, out string strMwIp, out int nMwPort);
                    _mwIpPort = new Tuple<string, int>(strMwIp, nMwPort);
                }

                // pie
                if (stateCacheData.TryGetValue(IssDataCodes.pat_info_ip_port, out obj) && obj != null)
                {
                    Communications.ParseIpPort((string)obj, out string strPieIp, out int nPiePort);
                    _pieIpPort = new Tuple<string, int>(strPieIp, nPiePort);
                }

                // lss
                if (stateCacheData.TryGetValue(IssDataCodes.lss_ip_port, out obj) && obj != null)
                {
                    Communications.ParseIpPort((string)obj, out string strLssIp, out int lssPort);
                    _lssIpPort = new Tuple<string, int>(strLssIp, lssPort);
                }

                // lib
                if (!stateCacheData.TryGetValue(IssDataCodes.library_ip_port, out obj) || obj == null)
                {
                    throw new Exception("Failed to retrieve Library IpPor from SS");
                }

                var strLocalLibIpPort = (string)obj;
                Communications.ParseIpPort(strLocalLibIpPort, out string strLibIp, out int nLibPort);
                var lipIpPort = new Tuple<string, int>(strLibIp, nLibPort);

                // who am i: if no recorder, I'm EasyView; if recorder but no remote - standalone; otherwise - connected
                if (!IsConfigured(_recIpPort.Item1))
                {
                    MonsoonCfg = EMonsoonConfig.EasyView;
                    _localLibIpPort = lipIpPort;
                    _remLibIpPort = lipIpPort;
                }
                else
                {
                    _localLibIpPort = lipIpPort;

                    if (!stateCacheData.TryGetValue(IssDataCodes.remote_library_ip_port, out obj) || obj == null)
                        throw new Exception("Failed to retrieve remote Lib Ip/port from SS");

                    var strRemoteLibIpPort = (string)obj;
                    Communications.ParseIpPort(strRemoteLibIpPort, out strLibIp, out nLibPort);
                    if (!IsConfigured(strLibIp))
                    {
                        MonsoonCfg = EMonsoonConfig.Standalone;
                        _remLibIpPort = lipIpPort;
                        LogEvent(EventLogEntryType.Information, 0, "I am a standalone Monsoon", 3);
                    }
                    else
                    {
                        MonsoonCfg = EMonsoonConfig.Connected;
                        _remLibIpPort = new Tuple<string, int>(strLibIp, nLibPort);
                        LogEvent(EventLogEntryType.Information, 0, $"I am a connected Monsoon, talking to {strLibIp} ", 3);
                    }
                }
                // if not EV, send notice to node
                if (MonsoonCfg != EMonsoonConfig.EasyView)
                {

                    // retrieve rec data usable
                    var objRecDataUsable = GetIssData(IssDataCodes.recorder_data_usable, false);
                    var bRecUsable = objRecDataUsable != null && (bool)objRecDataUsable;

                    // lib
                    var bLibUsable = false;
                    if (stateCacheData.TryGetValue(IssDataCodes.library_data_usable, out obj) && obj != null)
                        bLibUsable = (bool)obj;

                    // recorder & lib usable together make up recorder-ready
                    var bRecorderUsable = bRecUsable && bLibUsable;
                    _nodeMgr?.SendRecorderReady( bRecorderUsable);
                }

            }
            catch (Exception ex)
            {
                MonsoonCfg = EMonsoonConfig.Unknown; // we want to try getting this again
                LogEvent(EventLogEntryType.Error, 0, $"InitStateCache err: {ex.Message}", 3);
            }
        }

        public string MyIp { get; private set; }

        public EMonsoonConfig MonsoonCfg { get; private set; } = EMonsoonConfig.Unknown;

        public IConfiguration Cfg { get; private set; }

        public WcfOneTimeUseClientWrapper<IIsmRecSi> GetIsmRecProxy()
        {
            return new WcfOneTimeUseClientWrapper<IIsmRecSi>(_recIpPort.Item1, _recIpPort.Item2, "IsmRecSvc");
        }

        public WcfOneTimeUseClientWrapper<ILivePreview> GetIsmLivePreviewProxy()
        {
            try
            {
                // get to cfg
                var config = AppConfigManager.GetConfigFileAuto();
                if (config == null)
                    throw new Exception("Failed to load wcf cfg");

                if (!(config.GetSection("system.serviceModel/client") is ClientSection sections))
                {
                    LogEvent(EventLogEntryType.Error, 0, "Failed to get IsmLivePreview information from WCF cfg", 3);
                    return null;
                }

                // get IsmLivePreview port
                var nPort = -1;
                foreach (ChannelEndpointElement e in sections.Endpoints)
                {
                    if (!e.Name.Equals("IsmLivePreview", StringComparison.InvariantCultureIgnoreCase)) continue;
                    nPort = e.Address.Port;
                    break;
                }
                if (nPort == -1)
                {
                    LogEvent(EventLogEntryType.Error, 0, "Did not find IsmLivePreview information in loaded wcf cfg", 3);
                    return null;
                }
                LogEvent(EventLogEntryType.Information, 0, $"About to get proxy for 127.0.0.1:{nPort}/IsmLivePreview", 4);
                return new WcfOneTimeUseClientWrapper<ILivePreview>("127.0.0.1", nPort, "IsmLivePreview");
            }
            catch (Exception e)
            {
                LogEvent(EventLogEntryType.Error, 0, "GetIsmLivePreviewProxy err: " + e.Message, 3);
                return null;
            }
        }


        private int GetFileTransferPort(bool bCapture)
        {
            if (bCapture || MonsoonCfg != EMonsoonConfig.Connected)
            {
                if (_lclLibFileTransfPort > 0)
                    return _lclLibFileTransfPort;
            }
            else if (_remLibFileTransfPort > 0)
                return _remLibFileTransfPort;

            int nPort;
            using (var lib = GetLibProxy(bCapture))
            {
                if (lib == null)
                    throw new Exception("Failed to contact remote lib");
                if (lib.Proxy.GetLibrarySettings(null, out var settings, out _) != 0 ||
                    !settings.ContainsKey(ESettings.file_transfer_port))
                    throw new Exception("Failed to get library settings from remote lib");

                nPort = int.Parse(settings[ESettings.file_transfer_port]);
            }
            return bCapture || MonsoonCfg != EMonsoonConfig.Connected
                ? _lclLibFileTransfPort = nPort
                : _remLibFileTransfPort = nPort;
        }


        public WcfOneTimeUseClientWrapper<IFileRepositoryService> GetLibFileRepProxy(bool bCapture = false)
        {
            string ip = null;
            int nPort = 0;
            try
            {
                nPort = GetFileTransferPort(bCapture);
                if (bCapture || MonsoonCfg != EMonsoonConfig.Connected)
                    ip = _localLibIpPort.Item1;
                else
                {
                    // if we are connecting to a remote library, first ask local one if it's online
                    if (GetRemoteLibState() != MsntSi.Types.eSystemState.online)
                    {
                        LogEvent(EventLogEntryType.Warning, 0, "Remote library is not online", 3);
                        return null;
                    }

                    ip = _remLibIpPort.Item1;
                }

                var proxy = new WcfOneTimeUseClientWrapper<IFileRepositoryService>(ip, nPort, "FileService");
                LogEvent(EventLogEntryType.Information, 0, $"GetLibFileRepProxy succeeded for capture-{bCapture}, for ip {ip}, port {nPort}", 5);
                return proxy;
            }
            catch (Exception ex)
            {
                LogEvent(EventLogEntryType.Error, 0, $"GetLibFileRepProxy failed for capture-{bCapture}, for ip {ip}, port {nPort}, with err {ex.Message}", 3);
                return null;
            }
        }

        private MsntSi.Types.eSystemState GetRemoteLibState()
        {
            using (var localLibProxy = GetLibProxy(true))
            {
                localLibProxy.Proxy.GetRemoteLibState(out var remLibState);
                return remLibState;
            }
        }

        public WcfOneTimeUseDuplexClientWrapper<ILibrary6Si, ResMgr> GetLibProxy(bool bCapture = false)
        {
            var  libIpPort = new Tuple<string, int>(string.Empty, -1);
            try
            {
                // if it's a non-capture library, or if I am a standalone - get my regular one; else, get remote
                bool localLib = bCapture || MonsoonCfg != EMonsoonConfig.Connected;
                libIpPort = localLib ? _localLibIpPort : _remLibIpPort;
                if (libIpPort == null)
                    throw new Exception($"Lib ip/port for local {localLib} lib is null ");

                // if we are connecting to a remote library, first ask local one if it's online
                if (!localLib && GetRemoteLibState() != MsntSi.Types.eSystemState.online)
                {
                    LogEvent(EventLogEntryType.Warning, 0, "Remote library is not online", 3);
                    return null;
                }

                
                var proxy = new WcfOneTimeUseDuplexClientWrapper<ILibrary6Si, ResMgr>(this, libIpPort.Item1, libIpPort.Item2, "LibraryService");
                LogEvent(EventLogEntryType.Information, 0, $"GetLibProxy for capture={bCapture} succeeded for {libIpPort.Item1}:{libIpPort.Item2}", 6);
                return proxy;
            }
            catch (Exception ex)
            {
                LogEvent(EventLogEntryType.Error, 0, $"GetLibProxy failed for capture={bCapture}, {libIpPort?.Item1}:{libIpPort?.Item2} with err {ex.Message}", 3);
                return null;
            }
        }


        public WcfOneTimeUseClientWrapper<IMiddleware2Si> GetMwProxy()
        {
            try
            {
                return new WcfOneTimeUseClientWrapper<IMiddleware2Si>(_mwIpPort.Item1, _mwIpPort.Item2, "IsmMiddleware");
            }
            catch (Exception ex)
            {
                LogEvent(EventLogEntryType.Error, 0, "GetMwProxy err: " + ex.Message, 3);
                return null;
            }
        }

        public WcfOneTimeUseDuplexClientWrapper<ILogService, ResMgr> GetLogProxy()
        {
            try
            {
                return new WcfOneTimeUseDuplexClientWrapper<ILogService, ResMgr>(this, "127.0.0.1", LogUtility.kLogPort, "IsmLogService");
            }
            catch (Exception ex)
            {
                LogEvent(EventLogEntryType.Error, 0, "GetLogProxy err: " + ex.Message, 3);
                return null;
            }
        }

        public bool IsLssEnabled { get => IsConfigured(_lssIpPort?.Item1); }

        public WcfOneTimeUseClientWrapper<ILssMainSi> GetLssProxy()
        {
            try
            {
                if (!IsConfigured(_lssIpPort?.Item1))
                    return null;

                return new WcfOneTimeUseClientWrapper<ILssMainSi>(_lssIpPort.Item1, _lssIpPort.Item2, "LssMgr");
            }
            catch (Exception ex)
            {
                LogEvent(EventLogEntryType.Error, 0, "GetLssProxy err: " + ex.Message, 3);
                return null;
            }
        }

        public List<VideoData> GetVideoData()
        {
            try
            {
                return (List<VideoData>)GetIssData(IssDataCodes.recorder_videos);
            }
            catch (Exception e)
            {
                LogEvent(EventLogEntryType.Error, 0, "GetVideoData err: " + e.Message, 3);
                return null;
            }
        }

        public ESuccess GetIssDataMap(IssDataCodes[] codesToGet, out Dictionary<IssDataCodes, object> dataValues)
        {
            dataValues = null;
            try
            {
                var nCodes = codesToGet.Select(code => (int)code).ToList();

                using (var issProxy = GetIssProxy())
                {
                    if (issProxy == null)
                    {
                        LogEvent(EventLogEntryType.Warning, 0, "SS failed to initialize or is offline!", 3);
                        return ESuccess.Offline;
                    }

                    if (issProxy.Proxy.GetCodedDataList(nCodes, out var lstData, out var strErr) != 0 && !string.IsNullOrEmpty(strErr))
                    {
                        LogEvent(EventLogEntryType.Error, 0, "GetCodedDataList err: " + strErr, 3);
                        return ESuccess.Error;
                    }

                    dataValues = lstData.ToDictionary(item => (IssDataCodes)item.m_nCode, item => item.m_Obj);
                    if (dataValues.Count == codesToGet.Length)
                    {
                        return 0;
                    }

                    var codesRetrieved = lstData.Select(item => (IssDataCodes)item.m_nCode);
                    var codesMissing = codesToGet.Except(codesRetrieved);
                    foreach (var code in codesMissing)
                        LogEvent(EventLogEntryType.Warning, 0, $"{code} not retrieved from iss", 3);
                    return ESuccess.PartialRetrieve;
                }
            }
            catch (Exception ex)
            {
                LogEvent(EventLogEntryType.Error, 0, "GetIssDataMap err: " + ex.Message, 3);
                return ESuccess.Error;
            }
        }

        public object GetIssData(IssDataCodes code, bool bLogFailure = true)
        {
            try
            {
                using (var issProxy = GetIssProxy())
                {
                    if (issProxy == null)
                    {
                        LogEvent(EventLogEntryType.Warning, 0, "SS failed to initialize or is offline!", 3);
                        return null;
                    }

                    if (issProxy.Proxy.GetCodedData((int)code, out var data, out var strErr) != 0)
                    {
                        if (bLogFailure)
                            LogEvent(EventLogEntryType.Warning, 0, $"Failed to retrieve {code} from SS. Err - {strErr}", 3);
                        return null;
                    }

                    // yay! return
                    return data.m_Obj;
                }
            }
            catch (Exception ex)
            {
                LogEvent(EventLogEntryType.Error, 0, $"GetIssData failed for {code} with err {ex.Message}", 3);
                return null;
            }

        }

        public List<PrintLayoutInfo> PrintLayouts
        {
            get
            {
                try
                {
                    return _printLayouts ??
                           (_printLayouts = (List<PrintLayoutInfo>)GetIssData(IssDataCodes.print_layouts));
                }
                catch (Exception ex)
                {
                    LogEvent(EventLogEntryType.Error, 0, "PrintLayouts.get failed with err " + ex.Message, 3);
                    return null;
                }
            }
        }

        public RecorderSettings RecorderSettings
        {
            get
            {
                try
                {
                    return _recSettings ??
                           (_recSettings = (RecorderSettings)GetIssData(IssDataCodes.recorder_settings));
                }
                catch (Exception ex)
                {
                    LogEvent(EventLogEntryType.Error, 0, "RecorderSettings.get failed with err " + ex.Message, 3);
                    return null;
                }
            }
        }

        public string LibHost => _remLibIpPort.Item1;

        public string CultureName => _cultureName;

        public string MonsoonLibHost
        {
            get
            {
                try
                {
                    if (IsConfigured(_easyViewHost))
                        return _easyViewHost;
                    if (MonsoonCfg == EMonsoonConfig.Connected)
                        return _remLibIpPort.Item1;
                    return MyIp;
                }
                catch (Exception ex)
                {
                    LogEvent(EventLogEntryType.Error, 0, "MonsoonLibHost.get err: " + ex.Message, 3);
                    return "0.0.0.0";
                }

            }
        }

        public string RoomId { get; private set; }
        public bool IsDev => Cfg["serial_number"].StartsWith("DEV", StringComparison.OrdinalIgnoreCase);

        public int LogVerbosity
        {
            get => _logClient.LogVerbosity;
            set => _logClient.LogVerbosity = value;
        }

        private static bool IsConfigured(string ip)
        {
            return ip?.Contains("0.0.0.0") != true;
        }


        public void LogEvent(EventLogEntryType et, int nCode, string strMsg, int nLogVerbosityLevel)
        {
            try
            {
                _logClient.LogPrivateEvt(et, nCode, strMsg, nLogVerbosityLevel);
            }
            catch
            {
                // ignored
            }
        }

        public void LogEventWeb(EventLogEntryType et, int nCode, string strMsg, int nLogVerbosityLevel)
        {
            try
            {
                _logClientWeb.LogPrivateEvt(et, nCode, strMsg, nLogVerbosityLevel);
            }
            catch
            {
                // ignored
            }
        }

        //TODO refactor access event
        public void LogAccessEvent<TDetails>(TDetails details, AccessInfo accessInfo, string extraDetails) where TDetails : AccessDetailsBase
        {
            try
            {
                _logClient.LogAccessEvt(details, accessInfo, extraDetails);
            }
            catch
            {
                // ignored
            }
        }

        public void LogAccessEventWeb<TDetails>(TDetails details, AccessInfo accessInfo, string extraDetails) where TDetails : AccessDetailsBase
        {
            try
            {
                _logClientWeb.LogAccessEvt(details, accessInfo, extraDetails);
            }
            catch
            {
                // ignored
            }
        }

        #region IssCallBackContract
        public void OnData(IssData obj) { }
        public void OnEvent(IssEvent evt) { }
        public int Ping(int val) { return 0; }
        #endregion

        public static Stack<string> GetOwnerStack(string room, string login)
        {
            var userStack = new Stack<string>();
            userStack.Push("SYSTEM");
            if (!string.IsNullOrEmpty(room))
                userStack.Push(room);
            if (!string.IsNullOrEmpty(login) && !string.Equals(login, "null", StringComparison.OrdinalIgnoreCase))
                userStack.Push(login);
            return userStack;
        }

        public string LogVersion
        {
            get
            {
                using (var log = new WcfOneTimeUseDuplexClientWrapper<ILogService, ResMgr>(this, "127.0.0.1", LogUtility.kLogPort, "IsmLogService"))
                {
                    return log.Proxy.GetVersion();
                }
            }
        }

        public bool PublishPhysician(PersonNameData user)
        {
            try
            {
                if (string.IsNullOrEmpty(user?.m_strLogin))
                {
                    LogEvent(EventLogEntryType.Error, 0, "Invalid user passed in", 3);
                    return false;
                }

                using (var issProxy = GetIssProxy())
                {

                    if (issProxy == null)
                    {
                        LogEvent(EventLogEntryType.Warning, 0, "PublishPhysician failure because SS is offline", 3);
                        return false;
                    }

                    if (issProxy.Proxy.PublishDataAndEvent(-1,
                            new IssData(Convert.ToInt32(IssDataCodes.context_staff_physician_performing), user),
                            new IssEvent(Convert.ToInt32(IssEventCodes.context_staff_performing_physician_change), new List<object> { user }),
                            out var strErr) != 0)
                    {
                        LogEvent(EventLogEntryType.Error, 0, "PublishDataAndEvent for context_staff_physician_performing failed with err: " + strErr, 3);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogEvent(EventLogEntryType.Error, 0, $"PublishPhysician for {user?.m_strLogin} failed with err {ex.Message}", 3);
                return false;
            }
        }

        /// <summary>
        /// For IsmLogCallBackContract
        /// </summary>
        public  void PrivateEvt(IsmLogItem evt)
        { }
    }
}
