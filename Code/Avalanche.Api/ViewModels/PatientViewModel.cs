using Avalanche.Shared.Domain.Models;
using System;
using System.Text.Json.Serialization;

namespace Avalanche.Api.ViewModels
{
    public class PatientViewModel
    {
        public ulong? Id { get; set; }
        public string MRN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public KeyValuePairViewModel Sex { get; set; }
        public Department Department { get; set; }
        public ProcedureType ProcedureType { get; set; }
        public Physician Physician { get; set; }

        [JsonIgnore]
        public AccessInfo AccessInformation { get; set; }
    }
}
