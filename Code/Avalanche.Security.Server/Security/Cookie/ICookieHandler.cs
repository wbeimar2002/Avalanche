using Avalanche.Security.Server.Core.Models;
using System.Threading.Tasks;

namespace Avalanche.Security.Server.Security.Cookie
{
    public interface ICookieHandler
    {
        Task SignInUser(User user);
        Task SignOut();
    }
}