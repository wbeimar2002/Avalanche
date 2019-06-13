using System.Collections.Generic;
using IsmStateServer;

namespace MonsoonAPI.models
{
    public class SettingsInfo
    {
        // ReSharper disable once InconsistentNaming
        public string login;
        // ReSharper disable once InconsistentNaming
        public string setting_type;
        // ReSharper disable once InconsistentNaming
        public Dictionary<ESettings, string> settings;
    }
}
