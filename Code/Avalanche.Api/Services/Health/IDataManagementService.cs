using Google.Protobuf.WellKnownTypes;
using Ism.Storage.DataManagement.Client.V1.Protos;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    public interface IDataManagementService
    {
        Task<AddDepartmentResponse> AddDepartment(AddDepartmentRequest request);
        Task<AddProcedureTypeResponse> AddProcedureType(AddProcedureTypeRequest request);
        Task DeleteDepartment(DeleteDepartmentRequest request);
        Task DeleteProcedureType(DeleteProcedureTypeRequest request);
        Task<GetDepartmentsResponse> GetAllDepartments();
        Task<GetProcedureTypesResponse> GetProcedureTypesByDepartment(GetProcedureTypesByDepartmentRequest request);
        Task<ProcedureTypeMessage> GetProcedureType(GetProcedureTypeRequest request);
        Task<GetProcedureTypesResponse> GetAllProcedureTypes();
        Task<AddLabelResponse> AddLabel(AddLabelRequest label);
        Task DeleteLabel(DeleteLabelRequest request);
        Task<GetLabelsResponse> GetLabelsByProcedureType(GetLabelsByProcedureTypeRequest request);
        Task<LabelMessage> GetLabel(GetLabelRequest request);
        Task<GetLabelsResponse> GetAllLabels();
    }
}
