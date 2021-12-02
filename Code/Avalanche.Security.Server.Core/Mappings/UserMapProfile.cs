
using AutoMapper;
using Avalanche.Security.Server.Core.Entities;
using Avalanche.Security.Server.Core.Models;

using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Security.Server.Core.Mappings
{
    public class UserMapProfile : Profile
    {
        public sealed class UserConverter : ITypeConverter<UserEntity, UserModel>
        {
            public UserModel Convert(UserEntity source, UserModel destination, ResolutionContext context)
            {
                ThrowIfNull(nameof(source), source);
                ThrowIfNull(nameof(context), context);

                return new UserModel()
                {
                    Id = source.Id
                };
            }
        }

        public sealed class UserEntityConverter : ITypeConverter<UserModel, UserEntity>
        {
            public UserEntity Convert(UserModel source, UserEntity destination, ResolutionContext context)
            {
                ThrowIfNull(nameof(source), source);
                ThrowIfNull(nameof(context), context);

                return new UserEntity()
                {
                    Id = source.Id
                };
            }
        }

        public UserMapProfile()
        {
            CreateMap<UserModel, UserEntity>(MemberList.Destination).ConvertUsing<UserEntityConverter>();
            CreateMap<UserEntity, UserModel>(MemberList.Destination).ConvertUsing<UserConverter>();
        }
    }
}
