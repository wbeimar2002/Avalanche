using System;
using Avalanche.Shared.Domain.Enumerations;

namespace Avalanche.Shared.Domain.Models
{
    public class ProcedureExportStatus
    {
        public ExportType Type { get; set; }
        public ExportStatus Status { get; set; }
        public DateTime ExportDate { get; set; }
    }
}
