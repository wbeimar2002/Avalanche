using Avalanche.Shared.Domain.Models;
using System;
using System.Text.Json.Serialization;

namespace Avalanche.Api.ViewModels
{
    public class PatientViewModel : PatientModel
    {
        public KeyValuePairViewModel Sex { get; set; }
        public DepartmentModel Department { get; set; }
        public ProcedureTypeModel ProcedureType { get; set; }
        public PhysicianModel Physician { get; set; }
    }
}
