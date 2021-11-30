using Avalanche.Security.Server.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Security.Server.Core.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(UserEntity user, ERole[] userRoles);

        Task<UserEntity> FindByLoginAsync(string loginName);

        Task<List<UserEntity>> GetUsers(UserFilterModel filter);
    }
}