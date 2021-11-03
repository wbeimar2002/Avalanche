using AutoMapper;
using Avalanche.Security.Server.Controllers.Resources;
using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Mapping
{
    public class ResourceToModelProfile : Profile
    {
        public ResourceToModelProfile()
        {
            CreateMap<UserCredentialsResource, User>();
            CreateMap<CreateUserResource, User>();
        }
    }
}
