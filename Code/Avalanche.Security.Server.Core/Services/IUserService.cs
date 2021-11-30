using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Core.Services.Communication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Security.Server.Core.Services
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateUserAsync(UserEntity user, params ERole[] userRoles);

        Task<UserEntity> FindByLoginAsync(string loginName);

        Task<List<UserEntity>> GetUsers(UserFilterModel filter);
    }
}