using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Metadata
{
    public interface IMetadataManager
    {
        Task<List<KeyValuePairViewModel>> GetMetadata(Shared.Domain.Enumerations.MetadataTypes type, User user);
        Task<Department> AddDepartment(Department department);
        Task DeleteDepartment(string departmentName);
        Task<List<Department>> GetAllDepartments();
        Task<ProcedureType> AddProcedureType(Avalanche.Shared.Domain.Models.User user, ProcedureType procedureType);
        Task DeleteProcedureType(Avalanche.Shared.Domain.Models.User user, ProcedureType procedureType);
        Task<List<ProcedureType>> GetProceduresByDepartment(Avalanche.Shared.Domain.Models.User user, string departmentName = null);
    }
}
