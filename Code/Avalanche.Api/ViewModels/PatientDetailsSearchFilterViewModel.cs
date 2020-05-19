using System;

namespace Avalanche.Api.ViewModels
{
    public class PatientDetailsSearchFilterViewModel : FilterViewModelBase
    {
        public string LastName { get; set; }

        public string MRN { get; set; }

        public string AccessionNumber { get; set; }

        public string DepartmentName { get; set; }

        public DateTimeOffset? MinDate { get; set; }

        public DateTimeOffset? MaxDate { get; set; }

        public string RoomName { get; set; }

        public string ProcedureId { get; set; }

        public PatientDetailsSearchFilterViewModel() : base()
        {
        }

        public PatientDetailsSearchFilterViewModel(
            string lastName,
            string mrn,
            string accessionNumber,
            string departmentName,
            DateTimeOffset? minDate,
            DateTimeOffset? maxDate,
            string roomName,
            string procedureId)
        {
            LastName = lastName;
            MRN = mrn;
            AccessionNumber = accessionNumber;
            DepartmentName = departmentName;
            MinDate = minDate;
            MaxDate = maxDate;
            RoomName = roomName;
            ProcedureId = procedureId;
        }

        public override object Clone()
        {
            return new PatientDetailsSearchFilterViewModel(LastName, MRN, AccessionNumber, DepartmentName, MinDate, MaxDate, RoomName, ProcedureId);
        }

    }
}
