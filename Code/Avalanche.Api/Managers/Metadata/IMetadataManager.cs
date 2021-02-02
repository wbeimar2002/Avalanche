using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Metadata
{
    public interface IMetadataManager
    {
        Task<IList<KeyValuePairViewModel>> GetMetadata(User user, MetadataTypes type);
        Task<IList<SourceKeyValuePairViewModel>> GetSource(User user, MetadataTypes settingTypes);
        Task<Department> AddDepartment(User user, Department department);
        Task DeleteDepartment(User user, int departmentId);
        Task<IList<Department>> GetAllDepartments(User user);
        Task<ProcedureType> AddProcedureType(User user, ProcedureType procedureType);
        Task DeleteProcedureType(User user, ProcedureType procedureType);
        Task<List<ProcedureType>> GetProceduresByDepartment(User user, int? departmentId);
        Task ValidateDepartmentsSupport(User user);
        Task ValidateDepartmentsSupport(User user, int? departmentId);
    }
}
