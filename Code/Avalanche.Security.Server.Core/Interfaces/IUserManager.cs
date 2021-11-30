using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Core.Services.Communication;

namespace Avalanche.Security.Server.Core.Interfaces
{
    public interface IUserManager
    {
        Task<TokenResponse> Login(LoginInfo loginInfo);
    }
}