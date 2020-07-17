﻿using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Health
{
    public interface IPatientsManager
    {
        Task<List<Patient>> Search(PatientKeywordSearchFilterViewModel filter);
        Task<List<Patient>> Search(PatientDetailsSearchFilterViewModel filter);
        Task<Patient> RegisterPatient(NewPatientViewModel newPatient);
        Task<Patient> RegisterQuickPatient();
        Task UpdatePatient(NewPatientViewModel existing);
        Task DeletePatient(ulong id);
    }
}
