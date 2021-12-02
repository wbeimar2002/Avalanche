using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Entities;

namespace Avalanche.Security.Server.Managers
{
    public interface IUserService
    {
        Task<UserEntity> FindByUserNameAsync(string userName);
    }
}
