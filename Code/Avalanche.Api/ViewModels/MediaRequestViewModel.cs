using System;
using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class MediaRequestViewModel
    {
        public ProcedureIdViewModel procedureId;
        public List<string> mediaFileNameList;
        public string RequestId;

        public MediaRequestViewModel(ProcedureIdViewModel procId, List<string> mediaFileNames, string requestId)
        {
            procedureId = procId;
            mediaFileNameList = mediaFileNames;
            RequestId = requestId;
        }
    }
}
