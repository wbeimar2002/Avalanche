using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Security;
using Avalanche.Api.Services.Health;
using Avalanche.Api.ViewModels;
using Avalanche.Security.Server.Client.V1.Protos;
using Ism.Library.V1.Protos;

namespace Avalanche.Api.Managers.Data
{
    public class PhysiciansManager : IPhysiciansManager
    {
        private readonly IMapper _mapper;
        private readonly ISecurityService _SecurityService;
        private readonly ILibraryService _libraryService;

        public PhysiciansManager(IMapper mapper, ISecurityService SecurityService, ILibraryService LibraryService)
        {
            _mapper = mapper;
            _SecurityService = SecurityService;
            _libraryService = LibraryService;
        }

        public async Task<IList<PhysicianViewModel>> GetPhysicians()
        {
            var response = await _SecurityService.GetAllUsers();
            return _mapper.Map<IList<UserMessage>, IList<PhysicianViewModel>>(response.Users);
        }

        public async Task<IList<PhysicianSearchResultViewModel>> GetPhysicians(string keyword)
        {
            var request = new GetPhysiciansSearchRequest
            {
                Keyword = keyword
            };

            var response = await _libraryService.GetPhysicians(request).ConfigureAwait(false);
            return _mapper.Map<IList<PhysicianSearchMessage>, IList<PhysicianSearchResultViewModel>>(response.Physicians);
        }
    }
}
