﻿using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Ism.IsmLogCommon.Core;
using Ism.PatientInfoEngine.Common.Core;
using Ism.PatientInfoEngine.Common.Core.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    public interface IPieService
    {
        Task<List<Patient>> Search(PatientSearchFieldsMessage searchFields, int firstRecordIndex, int maxResults, string searchCultureName);
        Task<Patient> RegisterPatient(Patient newPatient, ProcedureType procedureType, Physician physician);
        Task UpdatePatient(Patient existingPatient);
        Task<int> DeletePatient(ulong patiendId);
    }
}
