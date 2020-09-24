using Avalanche.Api.ViewModels;
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
        Task<List<Ism.PatientInfoEngine.Common.Core.Protos.PatientRecordMessage>> Search(PatientSearchFieldsMessage searchFields, int firstRecordIndex, int maxResults, string searchCultureName, Ism.PatientInfoEngine.Common.Core.Protos.AccessInfoMessage accessInfoMessage);
        Task<Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage> RegisterPatient(Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage patientRecordMessage, Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage accessInfoMessage);
        Task UpdatePatient(Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage patientRecordMessage, Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage accessInfoMessage);
        Task<int> DeletePatient(ulong patiendId, Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage accessInfoMessage);
    }
}
