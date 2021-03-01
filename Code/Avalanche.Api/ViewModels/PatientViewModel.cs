﻿using Avalanche.Shared.Domain.Models;
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
        public DepartmentModel Department { get; set; }
        public ProcedureTypeModel ProcedureType { get; set; }
        public PhysicianModel Physician { get; set; }

        [JsonIgnore]
        public AccessInfoModel AccessInformation { get; set; }
    }
}
