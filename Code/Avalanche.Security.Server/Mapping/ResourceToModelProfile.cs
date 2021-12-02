using AutoMapper;
using Avalanche.Security.Server.ViewModels;
using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Mapping
{
    public class ResourceToModelProfile : Profile
    {
        public ResourceToModelProfile()
        {
            CreateMap<UserCredentialsViewModel, UserModel>();
        }
    }
}