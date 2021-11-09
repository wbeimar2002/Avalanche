using Avalanche.Shared.Domain.Models;

namespace Avalanche.Api.ViewModels
{
    public class ArchiveServiceViewModel
    {
        public ulong SessionId { get; set; }
        public string? Title { get; set; }
        public string? Description {get; set;}
        public PhysicianModel? Physician { get; set; }
        public DepartmentModel? Department { get; set; }
        public ProcedureTypeModel? ProcedureType { get; set; }
    }
}

