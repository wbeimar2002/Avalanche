using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Core.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(User user, ERole[] userRoles);
        Task<User> FindByEmailAsync(string email);
    }
}