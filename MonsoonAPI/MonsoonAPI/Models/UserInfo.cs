using System.Collections.Generic;
using Newtonsoft.Json;

namespace MonsoonAPI.models
{
    [JsonObject]
    public class MonsoonUserInfo : PersonNameDataM
    {
        // ReSharper disable once InconsistentNaming
        public string pwd;
        // ReSharper disable once InconsistentNaming
        public IEnumerable<string> rights;
        // ReSharper disable once InconsistentNaming
        public bool m_bModifiable;
    }
}
