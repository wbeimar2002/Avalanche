using System;
using Avalanche.Shared.Infrastructure.Enumerations;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureSearchFilterViewModel : FilterViewModelBase
    {
        public string? PatientLastName { get; set; }
        public string? PatientId { get; set; }
        public string? PhysicianId { get; set; }
        public string? DepartmentName { get; set; }
        public string? ProcedureTypeName { get; set; }
        public bool? IsClinical { get; set; }
        public string? Sex { get; set; }

        public string? Keyword { get; set; }

        public DateTime? StartCreationTime { get; set; }
        public DateTime? EndCreationTime { get; set; }

        public bool? HasPendingEdits { get; set; }

        public bool IsDescending { get; set; }
        public ProcedureIndexSortingColumns ProcedureIndexSortingColumn { get; set; }

        public ProcedureSearchFilterViewModel()
        {

        }

        public ProcedureSearchFilterViewModel(string? keyword, DateTime? startCreationTime,
            DateTime? endCreationTime, bool? hasPendingEdits, bool isDescending, ProcedureIndexSortingColumns procedureIndexSortingColumn,
            string patientLastName, string? patientId, string? physicianId, string? departmentName, string? procedureTypeName, bool? isClinical, string? sex)
        {
            this.Keyword = keyword;
            this.StartCreationTime = startCreationTime;
            this.EndCreationTime = endCreationTime;
            this.HasPendingEdits = hasPendingEdits;
            this.IsDescending = isDescending;
            this.ProcedureIndexSortingColumn = procedureIndexSortingColumn;
            this.PatientLastName = patientLastName;
            this.PatientId = patientId;
            this.PhysicianId = physicianId;
            this.DepartmentName = departmentName;
            this.ProcedureTypeName = procedureTypeName;
            this.IsClinical = isClinical;
            this.Sex = sex;
        }

        public override object Clone()
        {
            return new ProcedureSearchFilterViewModel(this.Keyword, this.StartCreationTime, this.EndCreationTime, 
                this.HasPendingEdits, this.IsDescending, this.ProcedureIndexSortingColumn,
                this.PatientLastName, this.PatientId, this.PhysicianId, this.DepartmentName, this.ProcedureTypeName, this.IsClinical, this.Sex);
        }
    }
}
