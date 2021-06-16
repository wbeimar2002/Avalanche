using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class ProceduresContainerReponseViewModel
    {
        public int TotalCount { get; set; }
        public object PagedProcedures { get; set; }
    }
}
