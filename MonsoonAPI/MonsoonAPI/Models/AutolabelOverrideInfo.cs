using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonsoonAPI.Models
{
    public class AutolabelOverrideInfo
    {
        public List<string> CommonLabelOverrides { get; set; }

        public List<string> ProcTypeLabelOverrides { get; set; }

        public AutolabelOverrideInfo() { }

        public AutolabelOverrideInfo(List<string> commonLabelOverrides, List<string> procTypeLabelOverrides)
        {
            CommonLabelOverrides = commonLabelOverrides;
            ProcTypeLabelOverrides = procTypeLabelOverrides;
        }
    }
}
