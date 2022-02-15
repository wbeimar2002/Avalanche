using System.Threading.Tasks;
using Avalanche.Api.ViewModels;
using System.Collections.Generic;

namespace Avalanche.Api.Managers.Security
{
    public interface IUsersManager
    {
        Task<IEnumerable<UserViewModel>> Search(UserSearchFilterViewModel filter);
    }
}
