using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Security;
using Avalanche.Api.ViewModels;
using Avalanche.Security.Server.Client.V1.Protos;

namespace Avalanche.Api.Managers.Data
{
    public class PhysiciansManager : IPhysiciansManager
    {
        private readonly IMapper _mapper;
        private readonly IUsersManagementService _usersManagementService;

        public PhysiciansManager(IMapper mapper, IUsersManagementService usersManagementService)
        {
            _mapper = mapper;
            _usersManagementService = usersManagementService;
        }

        public async Task<IList<PhysicianViewModel>> GetPhysicians()
        {
            var response = await _usersManagementService.GetAllUsers();
            return _mapper.Map<IList<UserMessage>, IList<PhysicianViewModel>>(response.Users);
        }
    }
}
