using AutoMapper;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Entities;
using Avalanche.Security.Server.ViewModels;

namespace Avalanche.Security.Server.Mapping
{
    public class ViewModelToModelProfile : Profile
    {
        public ViewModelToModelProfile()
        {
            CreateMap<UserCredentialsViewModel, UserEntity>();
            CreateMap<CreateUserViewModel, UserEntity>();
            CreateMap<UserFilterViewModel, UserFilterModel>();
            CreateMap<UserViewModel, UserEntity>();
        }
    }
}
