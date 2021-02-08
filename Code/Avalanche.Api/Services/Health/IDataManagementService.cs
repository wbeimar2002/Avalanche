using Ism.Storage.Core.DataManagement.V1.Protos;
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
        Task<GetProcedureTypesResponse> GetProceduresByDepartment(GetProcedureTypesByDepartmentRequest request);
        Task<ProcedureTypeMessage> GetProcedureType(GetProcedureTypeRequest request);
    }
}
