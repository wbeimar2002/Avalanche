using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using IsmRec.Types;
using MonsoonAPI.models;
using MsntSi.Types;
using IsmUtility;
using IsmDmag;
using IsmLogCommon;
using IsmStateServer;
using IsmUtility.Security;

namespace MonsoonAPI
{

    public interface ISettingsMgr
    {
        void Init(IMonsoonResMgr rm);
        HttpStatusCode GetPresets(string strRoom, string strUser, out Dictionary<string, string> class2Val);
        HttpStatusCode GetAllSettings(out Dictionary<ESettings, string> settings);
        HttpStatusCode GetFriendlyLicenses(out List<string> friendllyLicenses);
        HttpStatusCode SetSettings(SettingsInfo settingsInfo);
        HttpStatusCode SetPresets(SettingsInfo settingsInfo);
        HttpStatusCode SetLicense(SettingsInfo settingsInfo);

        Dictionary<ESettings, string> GetLibSettings();

    }

    public class SettingsMgr : ISettingsMgr
    {
        static readonly ESettings[] KSsSettings =  { ESettings.vaultstream_server, ESettings.log_verbosity, ESettings.room_name, ESettings.web_server };
        static readonly ESettings[] KRecSettings = {ESettings.overscan, ESettings.pip_location, ESettings.overlay_record_info, ESettings.overlay_pip,
            ESettings.overlay_logo, ESettings.overlay_pat_info, ESettings.frequency_analyzer, ESettings.movie_mode, ESettings.max_clip_time, /*ESettings.rec_resolution,*/ // FB20823 - 720p depricated, no current choice other than 1080p
            ESettings.include_bitmaps, ESettings.room_name, ESettings.log_verbosity, ESettings.pat_info_autoshow, ESettings.dicom_export_modality_1,
            ESettings.dicom_export_modality_2 ,ESettings.enable_video_edit, ESettings.enable_video};
        static readonly ESettings[] KPieSettings =  { ESettings.pie_timeout, ESettings.log_verbosity, ESettings.date_range_days };
        static readonly ESettings[] KLibSettings =  { ESettings.vaultstream_server,ESettings.dicom_export_hours_delay,
            ESettings.institution_name, ESettings.aging_days,ESettings.AllowExport, ESettings.autolabel_mode,
            ESettings.dicom_export_server_1 , ESettings.dicom_export_automatic, ESettings.log_verbosity};

        private static readonly ESettings[] KRemLiBSettings = { ESettings.expiry_hours};
        
        IMonsoonResMgr _rm;
        private Dictionary<ESettings, string> _pieSettings ;
        private Dictionary<ESettings, string> _libSettings ;
        private Dictionary<ESettings, string> _recSettings ;

        public void Init(IMonsoonResMgr rm)
        {
            if (_rm == null)
                _rm = rm;
        }

        public HttpStatusCode GetPresets(string strRoom, string strUser, out Dictionary<string, string> class2Val)
        {
            class2Val = null;
            try
            {
                _rm.LogEvent(EventLogEntryType.Information, 0, $"SettingsMgr.GetPresets entered for {strRoom}, {strUser}", 3);
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                        return ReturnOffline(eSystemType.middleware, "GetPresets");

                    var userStack = ResMgr.GetOwnerStack(strRoom, strUser);
                    if (mwProxy.Proxy.Presets_GetValues(userStack, ref class2Val, out var strErr) != 0)
                        return ReturnErr("Presets_GetValues err: " + strErr);

                    // massage clip/clips
                    // deal with clips & movies
                    if (class2Val.ContainsKey("edited_output"))
                    {
                        eRecording_Mode mode =
                            (eRecording_Mode) Enum.Parse(typeof(eRecording_Mode), class2Val["edited_output"]);
                        class2Val["edited_output"] = ProcMovieInfoM.FromRecMode(mode).ToString();
                    }

                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr("GetPresets err: " + ex.Message);
            }
        }

     

        public HttpStatusCode GetAllSettings(out Dictionary<ESettings, string> settings)
        {
            settings = null;
            try
            {
                _rm.LogEvent(EventLogEntryType.Information, 0, "SettingsMgr.GetAllSettings entered", 3);
                
                // pie stuff?
                GetPieSettings();
                if (_pieSettings != null)
                    settings = _pieSettings;

                // lib stuff?
                bool bAllLibSettings = GetLibSettings() != null;
                if (_libSettings != null)
                {
                    if (settings != null)
                    {
                        var pieKeys = settings.Keys.Except(_libSettings.Keys);
                        var settingsNoLibConflicts = settings?.Where(s => pieKeys.Contains(s.Key));
                        settings = settingsNoLibConflicts.Concat(_libSettings).ToDictionary(k2V => k2V.Key, k2V => k2V.Value);
                    }
                    else
                    {
                        settings = _libSettings;
                    }
                }
                if (!bAllLibSettings)
                    _libSettings = null; // we didn't get all of them, so we want to retry later

                // rec
                GetRecSettings();
                if (_recSettings != null)
                {
                    if (null != settings)
                    {
                        foreach (var kvp in _recSettings)
                        {
                            settings[kvp.Key] = kvp.Value;
                        }
                    }
                    else
                    {
                        settings = _recSettings;
                    }
                }
                    //settings = settings?.Concat(_recSettings).ToDictionary(k2V => k2V.Key, k2V => k2V.Value) ?? _recSettings;

                // just go through all settings, filling stuff in...
                settings = settings ?? new Dictionary<ESettings, string>();
                settings[ESettings.log_verbosity] = _rm.LogVerbosity.ToString();
                settings[ESettings.room_name_adt] = _rm.RoomId;
                settings[ESettings.vaultstream_server] = _rm.MonsoonCfg == EMonsoonConfig.Connected ? _rm.LibHost : string.Empty;
                settings[ESettings.web_server] = _rm.MonsoonCfg == EMonsoonConfig.Connected ? _rm.MonsoonLibHost : string.Empty;
                settings[ESettings.filter_search_by_room] = _rm.Cfg[ESettings.filter_search_by_room.ToString()];

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "GetSettings err " + ex.Message, 3);
                return HttpStatusCode.OK;
            }
        }

        private void GetPieSettings()
        {
            try
            {
                if (_pieSettings != null)
                {
                    _rm.LogEvent(EventLogEntryType.Information, 0, "GetPieSettings returning because _pieSettings has already been retrieved", 4 );
                    return; // already retrieved;
                }

                // pie stuff?
                using (var pieProxy = _rm.GetPieProxy())
                {
                    if (pieProxy == null)
                    {
                        //21257 if pie is 0.0.0.0, don't log a warning
                        if (_rm.IsPieConfigured)
                            _rm.LogEvent(EventLogEntryType.Warning, 0, "Pie offline", 3);
                        else
                            _rm.LogEvent(EventLogEntryType.Information, 0, "Pie is disabled so skipping getting its settings", 3);
                        
                        return;
                    }

                    pieProxy.Proxy.GetSettings(out _pieSettings, out _);
                    _rm.LogEvent(EventLogEntryType.Information, 0, "Pie settings retrieved", 3);
                }
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "GetPieSettings err: " + ex.Message, 2);
            }
        }

        private readonly ESettings[] localLibSettingTags =
        {
            ESettings.institution_name, ESettings.AllowExport,
            ESettings.forward_lib, ESettings.remote_printing,
            ESettings.autolabel_mode, ESettings.aging_days
        };
        /// <summary>
        /// GetLibrary settings
        /// </summary>
        /// <returns>If everything was retrieved, return true; if some items failed (and we will need to pull again later), return false</returns>
       public  Dictionary<ESettings, string> GetLibSettings()
        {
            try
            {
                if (_libSettings != null)
                    return _libSettings;

                _rm.LogEvent(EventLogEntryType.Information, 0, "SettingsMgr.GetLibSettings entered", 4);
                Dictionary<ESettings, string> localLibSettings = null;
                if (_rm.MonsoonCfg == EMonsoonConfig.Connected)
                {
                    using (var libProxy = _rm.GetLibProxy(true))
                    {
                        if (libProxy?.Proxy == null)
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0, "Failed to connect to local library", 3);
                            return null;
                        }

                        if (libProxy.Proxy.GetLibrarySettings("cache", out  localLibSettings, out string strErr) != 0 || localLibSettings == null) // for local, we only care about cache
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0, "Failed to get setting from local library. Err - " + strErr, 3);
                            return null;
                        }

                        // only leave specific items from here
                        localLibSettings = localLibSettings.Where(setting => localLibSettingTags.Contains(setting.Key))
                            .ToDictionary(set => set.Key, set => set.Value);
                    }
                    _rm.LogEvent(EventLogEntryType.Information, 0, "Local settings retrieved", 6);
                }


                using (var libProxy = _rm.GetLibProxy())
                {
                    if (libProxy?.Proxy == null)
                    {
                        _rm.LogEvent(EventLogEntryType.Warning, 0, "Failed to connect to ultimate library!", 3);
                        return null;
                    }

                    string strLib = "cache";
                    localLibSettings?.TryGetValue(ESettings.forward_lib, out strLib);

                    if (libProxy.Proxy.GetLibrarySettings(strLib, out _libSettings, out string strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Warning, 0, $"Failed to get lib settings for {strLib} with err {strErr}"  , 3);
                        return null;
                    }

                    _rm.LogEvent(EventLogEntryType.Information, 0, $"Main lib settings for {strLib} retrieved", 6);

                    // merge other local items if needed
                    if (localLibSettings != null)
                    {
                        _libSettings = _libSettings.Where(set => !localLibSettings.ContainsKey(set.Key))
                                        .Union(localLibSettings)
                                        .ToDictionary(set => set.Key, set => set.Value);
                    }

                    // if edit task is disabled, AND we're in connected - check if it is really disabled on all libraries
                    if (!string.IsNullOrEmpty(strLib) && bool.Parse(_libSettings[ESettings.video_edit_task]) == false)
                    {
                        if (libProxy.Proxy.GetLibrarySettings(null, out var allLibSettings, out strErr) != 0)
                        {
                            _rm.LogEvent(EventLogEntryType.Warning, 0, $"Failed to get lib settings for NULL with err {strErr}", 3);
                            return null;
                        }
                        _libSettings[ESettings.video_edit_task] = allLibSettings[ESettings.video_edit_task];
                    }
                }
                return _libSettings;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "GetLibSettings err: " + ex.Message, 2);
                return null;
            }
        }

        private void GetRecSettings()
        {
            try
            {
                if (_recSettings != null || _rm.MonsoonCfg == EMonsoonConfig.EasyView)
                    return ;

                using (var recProxy = _rm.GetIsmRecProxy())
                {
                    if (recProxy == null)
                        return ;

                    if (recProxy.Proxy.GetSettings(out _recSettings, out var strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "GetSettings err: " + strErr, 3);
                        return ;
                    }

                    // massage
                    if (_recSettings.ContainsKey(ESettings.overlay_pat_info) || _recSettings.ContainsKey(ESettings.pat_info_autoshow))
                    {
                        bool bPatInfoAuto = bool.Parse(_recSettings[ESettings.pat_info_autoshow]);
                        if (bPatInfoAuto)
                            _recSettings[ESettings.overlay_pat_info] = "autoshow";
                        else
                        {
                            bool bOverlay = bool.Parse(_recSettings[ESettings.overlay_pat_info]);
                            _recSettings[ESettings.overlay_pat_info] = bOverlay ? "always" : "never";
                        }

                        _recSettings.Remove(ESettings.pat_info_autoshow);
                    }
                }
            }
            catch (Exception ex)
            {
                _recSettings = null;
                _rm.LogEvent(EventLogEntryType.Error, 0, "GetRecSettings failed, Err - " + ex.Message, 3);
            }
        }

        private bool StreamingLicensed => bool.Parse(_rm.Cfg.GetSection("licenses:streaming").Value);


        public HttpStatusCode GetFriendlyLicenses (out List<string> friendllyLicenses)
        {
            friendllyLicenses = new List<string>();
            try
            {
                bool bMobileCaptureLic = false;
                // some things only on clients
                if (_rm.MonsoonCfg != EMonsoonConfig.EasyView)
                {
                    // streaming?
                    if (StreamingLicensed)
                        friendllyLicenses.Add("streaming");

                    // Get video_edit_enabled from ism rec
                    using (var rec = _rm.GetIsmRecProxy())
                    {
                        if (rec == null)
                            return ReturnOffline(eSystemType.recorder, "GetFriendlyLicenses");

                        string strErr;
                        if (rec.Proxy.GetSettings(out var recSettings, out strErr) != 0)
                            return ReturnErr("GetSettings failed with err " + strErr);

                        if (recSettings.ContainsKey(ESettings.enable_recording) &&
                            bool.Parse(recSettings[ESettings.enable_recording]))
                            friendllyLicenses.Add("recording");
                        if (recSettings.ContainsKey(ESettings.enable_video_edit) &&
                            bool.Parse(recSettings[ESettings.enable_video_edit]))
                            friendllyLicenses.Add("video_editing");
                    }
                    bool.TryParse(_rm.Cfg.GetSection("licenses:mobileCapture")?.Value, out  bMobileCaptureLic);
                }

                // DICOM 
                bool bDicomLic = bool.Parse(_rm.Cfg.GetSection("licenses:dicom").Value);

                // other options ONLY for connected
                if (_rm.MonsoonCfg == EMonsoonConfig.Connected)
                {
                    string strTemp;

                    // HL7 input & output from PIE
                    bool bDicomOnVs = false;
                    GetPieSettings();
                    if (_pieSettings != null)
                    {
                        if (_pieSettings.TryGetValue(ESettings.hl7_orders, out strTemp) && bool.Parse(strTemp))
                            friendllyLicenses.Add("hl7_import_enabled");
                        if (_pieSettings.TryGetValue(ESettings.hl7_results, out strTemp) && bool.Parse(strTemp))
                            friendllyLicenses.Add("hl7_export_enabled");

                        // is server configured for DICOM?
                        if (bDicomLic)
                        {
                            if (_pieSettings.TryGetValue(ESettings.dicom_import_server, out strTemp) && !string.IsNullOrEmpty(strTemp))
                            {
                                bDicomOnVs = true;
                                friendllyLicenses.Add("dicom_import_enabled");
                            }
                        }
                        if (bMobileCaptureLic)
                        {
                            friendllyLicenses.Add("mobile_capture");
                        }
                    }

                    // DICOM video & Auto-Edit from Library
                    bool bLibAllSet = GetLibSettings() != null;
                    if (_libSettings != null)
                    {
                        if (bDicomLic && bDicomOnVs)
                        {
                            if (_libSettings.TryGetValue(ESettings.dicom_export_video, out strTemp) && bool.Parse(strTemp))
                                friendllyLicenses.Add("dicom_video_export");
                        }
                        if (!bLibAllSet)
                            _libSettings = null;
                    }

                }
                // DICOM for Standalone and EasyView
                else
                {
                    if (bDicomLic)
                        friendllyLicenses.Add("dicom_import_enabled");
                }
                

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                return ReturnErr( "GetFriendlyLicenses err: " + ex.Message);
            }
        }

        public HttpStatusCode SetSettings(SettingsInfo settingsInfo)
        {
            try
            {
                if (!SetSsSettings(settingsInfo.settings))
                    return ReturnErr("Failed to set SS settings");
                if (!SetApiSettings(settingsInfo.settings))
                    return ReturnErr("Failed to set API settings");

                if (_rm.MonsoonCfg != EMonsoonConfig.EasyView)
                {
                    if (!SetRecSettings(settingsInfo.settings))
                        return ReturnErr("Failed to set Rec settings");
                }

                if (!SetPieSettings(settingsInfo.settings))
                    return ReturnErr("Failed to set Pie settings");
                if (!SetLibSettings(settingsInfo.settings))
                    return ReturnErr("Failed to set Lib settings");
                if (!SetRemLibSettings(settingsInfo.settings))
                    return ReturnErr("Failed to set Lib settings");
                if (!SetMiddlewareSettings(settingsInfo.settings))
                    return ReturnErr("Failed to set Middleware settings");
                if (!SetLogSettings(settingsInfo.settings))
                    return ReturnErr("Failed to set Log settings");
                if (!SetLssSettings(settingsInfo.settings))
                    return ReturnErr("Failed to set LSS settings");
                SetNtp(settingsInfo.settings);

                // clear out all saved settings - we WANT to retake them next time we need them
                _libSettings = _pieSettings = _recSettings = null;

                // now - refresh
                _rm.RefreshStateCache();

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                return ReturnErr( "SetSettings err: " + ex.Message);
            }
        }

        private const string NtpCfg = @"C:\Program Files (x86)\NTP\etc\ntp.conf";

        private static void SetNtp(IReadOnlyDictionary<ESettings, string> settings)
        {
            try
            {
                if (!settings.TryGetValue(ESettings.vaultstream_server, out var vaultStream) || string.IsNullOrEmpty(vaultStream))
                    return;

                if (!File.Exists(NtpCfg))
                {
                    IsmLog.LogEvent(EventLogEntryType.Warning, 0, $"Unable to save Ntp because file {NtpCfg} is missing", 3);
                    return;
                }

                // read in old
                var ntpLines = new List<string>();
                using (var reader = new StreamReader(NtpCfg))
                {
                    string str;
                    while ((str = reader.ReadLine()) != null)
                        ntpLines.Add(str);
                }

                // write out
                //Need admin rights to write to ntp.conf, so impersonate IsmAdmin
                const string strKfPath = @"C:\Middleware\db\kf.xml";
                string strAdminPwd = CryptographyUtilities.ReadEncryptedAdminPassword(strKfPath);
                using (ImpersonationContext context = ImpersonationContext.Impersonate("IsmAdmin", Environment.MachineName, strAdminPwd))
                using (var stream = new FileStream(NtpCfg, FileMode.Create))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        foreach (var str in ntpLines)
                        {
                            //sample line we're looking for: server W.X.Y.Z iburst
                            if (!str.StartsWith("server", StringComparison.OrdinalIgnoreCase) || !str.EndsWith("iburst", StringComparison.OrdinalIgnoreCase))
                                writer.WriteLine(str);
                            else
                                writer.WriteLine($"server {vaultStream} iburst");
                        }
                    }
                }
                IsmLog.LogEvent(EventLogEntryType.Information, 0, "Ntp server set", 3);
            }
            catch (Exception e)
            {
                IsmLog.LogEvent(EventLogEntryType.Error, 0, "SetNtp failed with err " + e.Message, 1);
            }
        }

        public HttpStatusCode SetPresets(SettingsInfo settingsInfo)
        {
            try
            {
                using (var mwProxy = _rm.GetMwProxy())
                {
                    if (mwProxy == null)
                        return ReturnOffline(eSystemType.middleware, "SetPresets");

                    Dictionary<string, string> presetValues =
                        settingsInfo.settings.ToDictionary(set => set.Key.ToString(), set => set.Value, StringComparer.OrdinalIgnoreCase);

                    // deal with clips & movies
                    if (presetValues.ContainsKey("edited_output"))
                    {
                        ProcMovieInfoM.EMovieModeM mode = (ProcMovieInfoM.EMovieModeM) Enum.Parse(typeof(ProcMovieInfoM.EMovieModeM),
                            presetValues["edited_output"]);
                        presetValues["edited_output"] = ProcMovieInfoM.ToRecMode(mode).ToString();
                    }

                    if (mwProxy.Proxy.Presets_SetPresetValues(settingsInfo.login, presetValues, out var strErr) != 0)
                        return ReturnErr( "Presets_SetPresetValues err: " + strErr);

                    return HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return ReturnErr( "SetPresets err: " + ex.Message);
            }
        }

        private HttpStatusCode ReturnOffline(eSystemType serv, string strMethod)
        {
            _rm.LogEvent(EventLogEntryType.Warning, 0, $"SettingsMgr.{strMethod} failure because {serv} is offline", 3);
            return HttpStatusCode.ServiceUnavailable;
        }

        private HttpStatusCode ReturnErr(string strErr)
        {
            _rm.LogEvent(EventLogEntryType.Error, 0, strErr, 3);
            return HttpStatusCode.InternalServerError;
        }

        private bool SetSsSettings(Dictionary<ESettings, string> settings)
        {
            try
            {
                var ssSettings = settings.Where(set => KSsSettings.Contains(set.Key));
                var keyValuePairs = ssSettings as KeyValuePair<ESettings, string>[] ?? ssSettings.ToArray();
                if (!keyValuePairs.Any())
                    return true;

                // if VaultStream is being set, let SS know whether or not it is to set LSS
                var ssSettingsMap = keyValuePairs.ToDictionary(set => set.Key, set => set.Value);
                if (ssSettingsMap.ContainsKey(ESettings.vaultstream_server))
                    ssSettingsMap[ESettings.video_streaming] = StreamingLicensed.ToString();

                using (var issProxy = _rm.GetIssProxy())
                {
                    if (issProxy == null)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "SetSSSettings failed becasue SS is offline", 3);
                        return false;
                    }

                    if (issProxy.Proxy.SetSettings(ssSettingsMap, out var strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "SetLogVerbosity err: " + strErr, 3);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "SetSSSettings err: " + ex.Message, 2);
                return false;
            }
        }



        private bool SetPieSettings(Dictionary<ESettings, string> settings)
        {
            try
            {
                var pisSettings = settings.Where(setting =>
                    Array.IndexOf(KPieSettings, setting.Key) >= 0 ||
                    setting.Key.ToString().StartsWith("dicom_", StringComparison.OrdinalIgnoreCase));
                var keyValuePairs = pisSettings as KeyValuePair<ESettings, string>[] ?? pisSettings.ToArray();
                if (!keyValuePairs.Any())
                {
                    _rm.LogEvent(EventLogEntryType.Information, 0, "SetPieSettings skipping because no rec settings are being set", 3);
                    return true;
                }

                if (!_rm.IsPieLocalEnabled)
                {
                    _rm.LogEvent(EventLogEntryType.Warning, 0, "SetPieSettings skipping because PIE is not local",3);
                    return true;
                }

                using (var pieProxy = _rm.GetPieProxy())
                {
                    if (pieProxy == null)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "SetPieSettings failed because PIE is offline", 3);
                        return false;
                    }

                    var pieSettingsMap = keyValuePairs.ToDictionary(ls => ls.Key, ls => ls.Value);
                    if (pieProxy.Proxy.SetSettings(pieSettingsMap, out var strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "SetPieSettings err: " + strErr, 3);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "SetPieSettings err: " + ex.Message, 2);
                return false;
            }
        }

        private bool SetRecSettings(Dictionary<ESettings, string> settings)
        {
            try
            {
                // Deal with 'overlay_pat_info'
                if (settings.TryGetValue(ESettings.overlay_pat_info, out var strOverlayPatInfo))
                {
                    switch (strOverlayPatInfo)
                    {
                        case "autoshow":
                            settings[ESettings.pat_info_autoshow] = "true";
                            settings[ESettings.overlay_pat_info] = "true";
                            break;
                        case "always":
                            settings[ESettings.overlay_pat_info] = "true";
                            settings[ESettings.pat_info_autoshow] = "false";
                            break;
                        case "never":
                            settings[ESettings.overlay_pat_info] = "false";
                            settings[ESettings.pat_info_autoshow] = "false";
                            break;
                        default: throw new Exception("Unrecognized value for overlay_pat_info " + strOverlayPatInfo);
                    }
                }

                var recSettings = settings.Where(setting => Array.IndexOf(KRecSettings, setting.Key) >= 0);
                var keyValuePairs = recSettings as KeyValuePair<ESettings, string>[] ?? recSettings.ToArray();
                if (!keyValuePairs.Any())
                {
                    _rm.LogEvent(EventLogEntryType.Information, 0, "SetRecSettings skipping because no rec settings are being set", 3);
                    return true;
                }



                using (var recProxy = _rm.GetIsmRecProxy())
                {
                    if (recProxy == null)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "SetRecSettings failed because IsmRec is offline", 3);
                        return false;
                    }

                    var recSettingsMap = keyValuePairs.ToDictionary(ls => ls.Key, ls => ls.Value);
                    if (recProxy.Proxy.SetSettings(recSettingsMap, out var strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "SetRecSettings err: " + strErr, 3);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "SetRecSettings err: " + ex.Message, 2);
                return false;
            }
        }

        private bool SetLibSettings(Dictionary<ESettings, string> settings)
        {
            try
            {
                var libSettings = settings.Where(setting => Array.IndexOf(KLibSettings, setting.Key) >= 0);
                var keyValuePairs = libSettings as KeyValuePair<ESettings, string>[] ?? libSettings.ToArray();
                if (!keyValuePairs.Any())
                {
                    _rm.LogEvent(EventLogEntryType.Information, 0,
                        "SetLibSettings skipping because no library settings are being set", 3);
                    return true;
                }

                using (var libProxy = _rm.GetLibProxy(true)) // get our local library
                {
                    if (libProxy == null)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "SetLibSettings failed because Lib is offline", 3);
                        return false;
                    }

                    var libSettingsMap = keyValuePairs.ToDictionary(ls => ls.Key, ls => ls.Value);
                    if (libProxy.Proxy.SetLibrarySettings(libSettingsMap, "cache", out var strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "SetLibrarySettings err: " + strErr, 3);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "SetLibSettings err: " + ex.Message, 2);
                return false;
            }
            finally
            {
                _libSettings = null;
            }
        }

        private bool SetRemLibSettings(Dictionary<ESettings, string> settings)
        {
            try
            {
                var remLibSettings = settings.Where(setting => Array.IndexOf(KRemLiBSettings, setting.Key) >= 0);
                var keyValuePairs = remLibSettings as KeyValuePair<ESettings, string>[] ?? remLibSettings.ToArray();
                if (!keyValuePairs.Any())
                {
                    _rm.LogEvent(EventLogEntryType.Information, 0, "SetRemLibSettings skipping because no library settings are being set", 3);
                    return true;
                }

                // which library is ours configured to forward to?
                string strForwardLib;
                using (var libProxy = _rm.GetLibProxy(true))
                {
                    if (libProxy == null)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "Unable to find forwarding lib name because library is offline", 3);
                        return false;
                    }

                    if (libProxy.Proxy.GetLibrarySettings("cache", out var libSettings, out string strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "Failed to get forwarding lib name. Err - " + strErr, 3);
                        return false;
                    }
                    if(!libSettings.TryGetValue(ESettings.forward_lib, out strForwardLib))
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "Forward lib name not returnred" + strErr, 3);
                        return false;
                    }

                }

                using (var libProxy = _rm.GetLibProxy(false)) // definitely want downstream library here
                {
                    if (libProxy == null)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "SetRemLibSettings failed because Lib is offline", 3);
                        return false;
                    }

                    var libSettingsMap = keyValuePairs.ToDictionary(ls => ls.Key, ls => ls.Value);
                    if (libProxy.Proxy.SetLibrarySettings(libSettingsMap, strForwardLib, out var strErr) != 0)
                    {
                        _rm.LogEvent(EventLogEntryType.Error, 0, "SetLibrarySettings err: " + strErr, 3);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "SetLibSettings err: " + ex.Message, 2);
                return false;
            }
        }

        private bool SetApiSettings(Dictionary<ESettings, string> settings)
        {
            try
            {
                var filePath = "appsettings.json";
                var objSettings = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(filePath));

                bool bDoSave = false;

                if (settings.ContainsKey(ESettings.filter_search_by_room))
                {
                    _rm.Cfg[ESettings.filter_search_by_room.ToString()] = settings[ESettings.filter_search_by_room];
                    objSettings[ESettings.filter_search_by_room.ToString()] = _rm.Cfg[ESettings.filter_search_by_room.ToString()];
                    bDoSave = true;
                }
                if (settings.ContainsKey(ESettings.log_verbosity))
                {
                    _rm.Cfg[ESettings.log_verbosity.ToString()] = settings[ESettings.log_verbosity];
                    _rm.LogVerbosity = int.Parse(settings[ESettings.log_verbosity]);
                    objSettings[ESettings.log_verbosity.ToString()] = _rm.Cfg[ESettings.log_verbosity.ToString()];
                    bDoSave = true;
                }

                if (settings.ContainsKey(ESettings.dicom_licensed))
                {
                    _rm.Cfg.GetSection("licenses:dicom").Value= settings[ESettings.dicom_licensed];
                    objSettings["licenses"]["dicom"] = _rm.Cfg.GetSection("licenses:dicom").Value;
                    bDoSave = true;

                }
                if (settings.ContainsKey(ESettings.video_streaming))
                {
                    _rm.Cfg.GetSection("licenses:streaming").Value = settings[ESettings.video_streaming];
                    objSettings["licenses"]["streaming"] = _rm.Cfg.GetSection("licenses:streaming").Value;
                    bDoSave = true;
                }
                if (settings.ContainsKey(ESettings.mobile_capture))
                {
                    _rm.Cfg.GetSection("licenses:mobileCapture").Value = settings[ESettings.mobile_capture];
                    objSettings["licenses"]["mobileCapture"] = _rm.Cfg.GetSection("licenses:mobileCapture").Value;
                    bDoSave = true;
                }

                if (bDoSave)
                    File.WriteAllText(filePath, objSettings.ToString());

                return true;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "SetApiSettings err: " + ex.Message, 2);
                return false;
            }
        }

        public bool SetMiddlewareSettings(Dictionary<ESettings, string> settings)
        {
            try
            {
                if (settings.ContainsKey(ESettings.log_verbosity))
                {
                    int verbosity = int.Parse(settings[ESettings.log_verbosity]);

                    using (var mwProxy = _rm.GetMwProxy())
                    {
                        if (mwProxy == null)
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0, "SetMiddlewareSettings failed because Middleware is offline", 1);
                            return false;
                        }

                        if (0 != mwProxy.Proxy.SetLogVerbosity(verbosity))
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0, "SetMiddlewareSettings failed to set log verbosity", 1);
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "SetMiddlewareSettings err: " + ex.Message, 1);
                return false;
            }
        }

        public bool SetLogSettings(Dictionary<ESettings, string> settings)
        {
            try
            {
                if (settings.ContainsKey(ESettings.log_verbosity))
                {
                    int verbosity = int.Parse(settings[ESettings.log_verbosity]);

                    using (var logProxy = _rm.GetLogProxy())
                    {
                        if (logProxy == null)
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0, "SetLogSettings failed because IsmLog is offline", 1);
                            return false;
                        }

                        if (0 != logProxy.Proxy.SetLogServiceVerbosity(verbosity, out string strErr))
                        {
                            _rm.LogEvent(EventLogEntryType.Error, 0, $"SetLogSettings failed to set log verbosity with error: {strErr}", 1);
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, $"SetLogSettings err: {ex.Message}", 1);
                return false;
            }
        }

        public bool SetLssSettings(Dictionary<ESettings, string> settings)
        {
            try
            {
                if (_rm.IsLssEnabled)
                {
                    if (settings.ContainsKey(ESettings.log_verbosity))
                    {
                        int verbosity = int.Parse(settings[ESettings.log_verbosity]);

                        using (var lssProxy = _rm.GetLssProxy())
                        {
                            if (lssProxy == null)
                            {
                                _rm.LogEvent(EventLogEntryType.Error, 0, "SetLssSettings failed because LSS is offline", 1);
                                return false;
                            }

                            if (0 != lssProxy.Proxy.SetLogVerbosity(verbosity))
                            {
                                _rm.LogEvent(EventLogEntryType.Error, 0, "SetLssSettings failed to set log verbosity", 1);
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _rm.LogEvent(EventLogEntryType.Error, 0, "SetLssSettings err: " + ex.Message, 1);
                return false;
            }
        }

        public HttpStatusCode SetLicense(SettingsInfo settingsInfo)
        {
            string key = null;
            try
            {
                if (settingsInfo?.settings == null || !settingsInfo.settings.TryGetValue(ESettings.key, out key))
                {
                    _rm.LogEvent(EventLogEntryType.Error, 0, "key not contained in structure passed into SetLicense", 3);
                    return HttpStatusCode.BadRequest;
                }

                if (UCDRC4.DecodeMoncoonLicKey(key, out string strSn, out var options, out var strErr) != 0)
                    return ReturnErr("DecodeMoncoonLicKey err: " + strErr);

                // fix up current serial number incase it's an old 5 digit one
                string curSerialNumber = _rm.Cfg["serial_number"];
                if (string.IsNullOrEmpty(curSerialNumber))
                {
                    return ReturnErr("No serial number is currently configured");
                }
                UCDRC4.GetMonsoonSerialParts(curSerialNumber, out string prefix, out string suffix, true);
                curSerialNumber = $"{prefix}-{suffix}";

                if (!string.Equals(strSn, curSerialNumber, StringComparison.CurrentCultureIgnoreCase))
                    return ReturnErr("Serial number in the key does not match configured serial number");

                var apiSettings = new Dictionary<ESettings, string>();
                if (options.Contains(UCDRC4.MonsoonOptions.Dicom))
                    apiSettings[ESettings.dicom_licensed] = "true";
                if (options.Contains(UCDRC4.MonsoonOptions.Streaming))
                    apiSettings[ESettings.video_streaming] = "true";
                if (options.Contains(UCDRC4.MonsoonOptions.MobileCapture))
                    apiSettings[ESettings.mobile_capture] = "true";

                if (apiSettings.Any())
                {
                    if (!SetApiSettings(apiSettings))
                        return ReturnErr("Failed to enable system for DICOM/Streaming");
                    _rm.LogEvent(EventLogEntryType.Information, 0, "Successfully enabled system for DICOM/Streaming", 3);
                }

     
                var recSettings = new Dictionary<ESettings, string>();
                if (options.Contains(UCDRC4.MonsoonOptions.Video))
                {
                    recSettings[ESettings.enable_video] = "true";
                    _rm.LogEvent(EventLogEntryType.Information, 0, "About to enable video recording", 3);
                }
                if (options.Contains(UCDRC4.MonsoonOptions.VideoEdit))
                {
                    recSettings[ESettings.enable_video_edit] = "true";
                }

                if (recSettings.Any())
                {
                    if (!SetRecSettings(recSettings))
                        return ReturnErr("Failed to enable system for recording settings");
                }
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                return ReturnErr($"SetLicense failed for {key} with err {ex.Message}");
            }
        }

    }
}
