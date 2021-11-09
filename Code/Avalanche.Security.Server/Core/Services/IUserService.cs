using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Core.Services.Communication;

namespace Avalanche.Security.Server.Core.Services
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateUserAsync(User user, params ERole[] userRoles);
        Task<User> FindByLoginAsync(string loginName);
        Task<List<User>> GetUsers(UserFilterModel filter);
    }
}
