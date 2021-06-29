using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class ProceduresContainerViewModel
    {
        public IList<ProcedureViewModel> Procedures { get; set; }
        public int TotalCount { get; set; }
    }
}
