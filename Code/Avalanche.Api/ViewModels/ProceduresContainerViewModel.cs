using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class ProceduresContainerViewModel
    {
        public IList<ProcedureViewModel> Procedures { get; set; }
        public int TotalCount { get; set; }
    }
}
