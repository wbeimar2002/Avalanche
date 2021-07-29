using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Infrastructure.Enumerations;
using System;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureAdvancedSearchFilterViewModel : FilterViewModelBase
    {
        public string? PatientLastName { get; set; }
        public string? PatientId { get; set; }
        public string? PhysicianId { get; set; }
        public string? DepartmentName { get; set; }
        public string? ProcedureTypeName { get; set; }

        public string Keyword { get; set; }

        public DateTime? StartCreationTime { get; set; }
        public DateTime? EndCreationTime { get; set; }

        public bool? HasPendingEdits { get; set; }

        public bool IsDescending { get; set; }
        public ProcedureIndexSortingColumns ProcedureIndexSortingColumn { get; set; }

        public ProcedureAdvancedSearchFilterViewModel()
        {

        }

        public ProcedureAdvancedSearchFilterViewModel(string keyword, DateTime? startCreationTime,
            DateTime? endCreationTime, bool? hasPendingEdits, bool isDescending, ProcedureIndexSortingColumns procedureIndexSortingColumn,
            string patientLastName, string? patientId, string? physicianId, string departmentName, string procedureTypeName)
        {
            this.Keyword = keyword;
            this.StartCreationTime = startCreationTime;
            this.EndCreationTime = endCreationTime;
            this.HasPendingEdits = hasPendingEdits;
            this.IsDescending = IsDescending;
            this.ProcedureIndexSortingColumn = procedureIndexSortingColumn;
            this.PatientLastName = patientLastName;
            this.PatientId = patientId;
            this.PhysicianId = physicianId;
            this.DepartmentName = departmentName;
            this.ProcedureTypeName = procedureTypeName;
        }

        public override object Clone()
        {
            return new ProcedureAdvancedSearchFilterViewModel(this.Keyword, this.StartCreationTime, this.EndCreationTime, 
                this.HasPendingEdits, this.IsDescending, this.ProcedureIndexSortingColumn,
                this.PatientLastName, this.PatientId, this.PhysicianId, this.DepartmentName, this.ProcedureTypeName);
        }
    }
}
