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
            CreateMap<UserCredentialsViewModel, UserModel>()
                .ForMember(a => a.Id, opt => opt.Ignore())
                .ForMember(a => a.DepartmentId, opt => opt.Ignore())
                .ForMember(a => a.FirstName, opt => opt.Ignore())
                .ForMember(a => a.IdnId, opt => opt.Ignore())
                .ForMember(a => a.LastName, opt => opt.Ignore())
                .ForMember(a => a.SiteId, opt => opt.Ignore())
                .ForMember(a => a.SystemId, opt => opt.Ignore())
                .ForMember(a => a.UserName, opt => opt.MapFrom(a => a.UserName))
                .ForMember(a => a.IsAdmin, opt => opt.Ignore());

            CreateMap<UserMessage, UserModel>()
                .ForMember(a => a.DepartmentId, opt => opt.Ignore())
                .ForMember(a => a.IdnId, opt => opt.Ignore())
                .ForMember(a => a.SiteId, opt => opt.Ignore())
                .ForMember(a => a.SystemId, opt => opt.Ignore())
                .ForMember(a => a.FirstName, opt => opt.MapFrom(a => a.FirstName))
                .ForMember(a => a.LastName, opt => opt.MapFrom(a => a.LastName))
                .ForMember(a => a.UserName, opt => opt.MapFrom(a => a.UserName))
                .ForMember(a => a.Id, opt => opt.MapFrom(a => a.Id))
                .ForMember(a => a.Password, opt => opt.Ignore())
                .ForMember(a => a.IsAdmin, opt => opt.MapFrom(a => a.IsAdmin));

            CreateMap<AccessToken, AccessTokenViewModel>()
                .ForMember(a => a.AccessToken, opt => opt.MapFrom(a => a.Token))
                .ForMember(a => a.RefreshToken, opt => opt.MapFrom(a => a.RefreshToken.Token))
                .ForMember(a => a.Expiration, opt => opt.MapFrom(a => a.Expiration));


            // SMELL: Why is a gRPC Mesage being mapped directly to a ViewModel?
            CreateMap<UserMessage, UserViewModel>()
                .ForMember(a => a.FirstName, opt => opt.MapFrom(a => a.FirstName))
                .ForMember(a => a.LastName, opt => opt.MapFrom(a => a.LastName))
                .ForMember(a => a.UserName, opt => opt.MapFrom(a => a.UserName))
                .ForMember(a => a.Id, opt => opt.MapFrom(a => a.Id));
        }
    }
}
