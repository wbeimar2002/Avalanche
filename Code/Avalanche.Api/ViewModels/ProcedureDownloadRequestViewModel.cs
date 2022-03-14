using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureDownloadRequestViewModel
    {
        public ProcedureIdViewModel ProcedureId { get; set; }
        public List<string> ContentItemIds { get; set; }
        public string RequestId { get; set; }
        public bool IncludePHI { get; set; }
        public ProcedureDownloadRequestViewModel(string requestId, ProcedureIdViewModel procId, List<string> contentItemIds, bool includePHI)
        {
            ProcedureId = procId;
            ContentItemIds = contentItemIds;
            RequestId = requestId;
            IncludePHI = includePHI;
        }

        public ProcedureDownloadRequestViewModel()
        {
        }
    }
}