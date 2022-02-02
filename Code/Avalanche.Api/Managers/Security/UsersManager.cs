using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Security;
using Avalanche.Api.ViewModels;
using System.Collections.Generic;
using Avalanche.Security.Server.Client.V1.Protos;

namespace Avalanche.Api.Managers.Security
{
    public class UsersManager : IUsersManager
    {
        private readonly ISecurityService _usersService;
        private readonly IMapper _mapper;

        public UsersManager(ISecurityService usersService, IMapper mapper)
        {
            _usersService = usersService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserViewModel>> Search(UserSearchFilterViewModel filter)
        {
            var response = await _usersService.SearchUsers(filter.Keyword);
            return _mapper.Map<IList<UserMessage>, IList<UserViewModel>>(response.Users);
        }
    }
}
