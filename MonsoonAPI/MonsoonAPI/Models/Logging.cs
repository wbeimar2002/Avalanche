using System;
using System.Diagnostics;

namespace MonsoonAPI.models
{
    public class LogMsg
    {
        public EventLogEntryType type;
        public string msg;
        public int verbosity;
        public DateTime ts;
    }
}
