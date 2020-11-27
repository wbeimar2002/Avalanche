using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Metadata
{
    public interface IMetadataManager
    {
        Task<List<KeyValuePairViewModel>> GetMetadata(Avalanche.Shared.Domain.Models.User user, Shared.Domain.Enumerations.MetadataTypes type);
        Task<Department> AddDepartment(Avalanche.Shared.Domain.Models.User user, Department department);
        Task DeleteDepartment(Avalanche.Shared.Domain.Models.User user, int departmentId);
        Task<List<Department>> GetAllDepartments(Avalanche.Shared.Domain.Models.User user);
        Task<ProcedureType> AddProcedureType(Avalanche.Shared.Domain.Models.User user, ProcedureType procedureType);
        Task DeleteProcedureType(Avalanche.Shared.Domain.Models.User user, ProcedureType procedureType);
        Task<List<ProcedureType>> GetProceduresByDepartment(Avalanche.Shared.Domain.Models.User user, int? departmentId);
    }
}
