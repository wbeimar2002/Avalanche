using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Ism.IsmLogCommon.Core;
using Ism.PatientInfoEngine.Common.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    public interface IPieService
    {
        Task<List<Patient>> Search(PatientSearchFieldsMessage searchFields, int firstRecordIndex, int maxResults, string searchCultureName, AccessInfo accessInfo);
        Task<Patient> RegisterPatient(Patient newPatient, AccessInfo accessInfo);
        Task UpdatePatient(Patient existingPatient, AccessInfo accessInfo);
        Task<int> DeletePatient(ulong patiendId, AccessInfo accessInfo);
    }
}
