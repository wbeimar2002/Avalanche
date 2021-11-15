using System.Linq;
using AutoMapper;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Core.Security.Tokens;
using Avalanche.Security.Server.Entities;
using Avalanche.Security.Server.ViewModels;

namespace Avalanche.Security.Server.Mapping
{
    public class ModelToViewModelProfile : Profile
    {
        public ModelToViewModelProfile()
        {
            CreateMap<UserEntity, UserViewModel>()
                .ForMember(u => u.Roles, opt => opt.MapFrom(u => u.UserRoles.Select(ur => ur.Role.Name)));

            CreateMap<AccessToken, AccessTokenViewModel>()
                .ForMember(a => a.AccessToken, opt => opt.MapFrom(a => a.Token))
                .ForMember(a => a.RefreshToken, opt => opt.MapFrom(a => a.RefreshToken.Token))
                .ForMember(a => a.Expiration, opt => opt.MapFrom(a => a.Expiration));

            CreateMap<UserFilterModel, UserFilterViewModel>();
        }
    }
}
