using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Infrastructure.Enumerations;
using System;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureAdvancedSearchFilterViewModel : ProcedureSearchFilterViewModel
    {
        public string? PatientLastName { get; set; }
        public int? PatientId { get; set; }
        public int? PhysicianId { get; set; }
        public int? DepartmentId { get; set; }
        public int? ProcedureTypeId { get; set; }
    }
}
