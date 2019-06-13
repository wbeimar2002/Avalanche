using ISM.Middleware2Si;
using System.Collections.Generic;

namespace MonsoonAPI.Models
{
    public class ActiveAutoLabelInfo
    {
        public List<AutoLabelInfo> ProcedureTypeLabels { get; set; }

        public List<string> CommonLabels { get; set; }

        public ActiveAutoLabelInfo() { }

        public ActiveAutoLabelInfo(List<AutoLabelInfo> procedureTypeLabels, List<string> commonLabels)
        {
            ProcedureTypeLabels = procedureTypeLabels;
            CommonLabels = commonLabels;
        }
    }
}
