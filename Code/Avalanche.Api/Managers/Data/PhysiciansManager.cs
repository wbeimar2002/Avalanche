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
        private readonly ISecurityService _SecurityService;

        public PhysiciansManager(IMapper mapper, ISecurityService SecurityService)
        {
            _mapper = mapper;
            _SecurityService = SecurityService;
        }

        public async Task<IList<PhysicianViewModel>> GetPhysicians()
        {
            var response = await _SecurityService.GetAllUsers();
            return _mapper.Map<IList<UserMessage>, IList<PhysicianViewModel>>(response.Users);
        }
    }
}
