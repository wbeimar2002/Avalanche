using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    public interface IPieService
    {
        Task<Ism.PatientInfoEngine.V1.Protos.SearchResponse> Search(Ism.PatientInfoEngine.V1.Protos.SearchRequest searchRequest);
        Task<Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordResponse> RegisterPatient(Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordRequest addPatientRecordRequest);
        Task UpdatePatient(Ism.Storage.PatientList.Client.V1.Protos.UpdatePatientRecordRequest updatePatientRecordRequest);
        Task<Ism.Storage.PatientList.Client.V1.Protos.DeletePatientRecordResponse> DeletePatient(Ism.Storage.PatientList.Client.V1.Protos.DeletePatientRecordRequest deletePatientRecordRequest);
    }
}
