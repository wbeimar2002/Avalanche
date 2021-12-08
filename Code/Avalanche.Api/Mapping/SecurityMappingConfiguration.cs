using AutoMapper;
using Avalanche.Api.ViewModels;
using Avalanche.Api.ViewModels.Security;
using Avalanche.Security.Server.Client.V1.Protos;
using Avalanche.Shared.Domain.Models;

namespace Avalanche.Api.Mapping
{
    public class SecurityMappingConfiguration : Profile
    {
        public SecurityMappingConfiguration()
        {
            CreateMap<UserCredentialsViewModel, UserModel>();
            CreateMap<UserMessage, UserModel>();

            CreateMap<AccessToken, AccessTokenViewModel>()
                .ForMember(a => a.AccessToken, opt => opt.MapFrom(a => a.Token))
                .ForMember(a => a.RefreshToken, opt => opt.MapFrom(a => a.RefreshToken.Token))
                .ForMember(a => a.Expiration, opt => opt.MapFrom(a => a.Expiration));
        }
    }
}
