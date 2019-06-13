using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonsoonAPI.models
{
    public class UsbExportInfo
    {
        public ProcedureIdM ProcedureId { get; set; }
        public  bool IncludePhi { get; set; }
        public  string Login { get; set; }
    }
}
