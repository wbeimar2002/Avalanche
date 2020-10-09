using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    public interface IPieService
    {
        Task<Ism.PatientInfoEngine.V1.Protos.SearchResponse> Search(Ism.PatientInfoEngine.V1.Protos.SearchRequest searchRequest);
        Task<Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordResponse> RegisterPatient(Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest addPatientRecordRequest);
        Task UpdatePatient(Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest updatePatientRecordRequest);
        Task<Ism.Storage.Core.PatientList.V1.Protos.DeletePatientRecordResponse> DeletePatient(Ism.Storage.Core.PatientList.V1.Protos.DeletePatientRecordRequest deletePatientRecordRequest);
    }
}
