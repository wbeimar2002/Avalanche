using Avalanche.Api.ViewModels;
using Ism.IsmPatientInfo.Core.Types;
using Ism.PatientInfoEngine.Common.Core.Models;
using Ism.PatientInfoEngine.Common.Core.proto;
using Ism.Storage.Common.Core.PatientList.Models;
using Ism.Utility.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Mapping.Health
{
    public class PieMapping: IPieMapping
    {
        public PieMapping() { }


        public PatientSearchFields GetDetailsSearchFields(PatientDetailsSearchFilterViewModel filter)
        {
            Preconditions.ThrowIfNull(nameof(filter), filter);

            return new PatientSearchFields(
                filter.RoomName,
                filter.LastName,
                filter.MRN,
                filter.MinDate,
                filter.MaxDate,
                filter.AccessionNumber,
                null,
                filter.DepartmentName,
                filter.ProcedureId);

        }

        public Shared.Domain.Models.Patient ToApiPatient(PatientRecord pieRecord)
        {
            Preconditions.ThrowIfNull(nameof(pieRecord), pieRecord);

            return new Shared.Domain.Models.Patient
            {
                AccessionNumber = pieRecord.AccessionNumber,
                DateOfBirth = pieRecord.Patient.Dob.ToDateTimeUnspecified(),
                Department = pieRecord.Department,
                Gender = pieRecord.Patient.Sex.ToString(),
                Id = pieRecord.Id,
                MRN = pieRecord.MRN,
                LastName = pieRecord.Patient.LastName,
                Name = pieRecord.Patient.FirstName,
                ProcedureType= pieRecord.ProcedureType,
                Room = pieRecord.Room
            };
        }

    }
}
