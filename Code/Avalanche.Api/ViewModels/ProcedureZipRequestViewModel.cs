using System;
using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureZipRequestViewModel
    {
        public ProcedureIdViewModel ProcedureId { get; set; }
        public List<string> MediaFileNameList { get; set; }
        public string RequestId { get; set; }

        public ProcedureZipRequestViewModel(string requestId, ProcedureIdViewModel procId, List<string> mediaFileNames )
        {
            ProcedureId = procId;
            MediaFileNameList = mediaFileNames;
            RequestId = requestId;
        }
    }
}
