
using AutoMapper;
using Avalanche.Security.Server.Client.V1.Protos;
using Avalanche.Security.Server.Core.Models;

using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Security.Server.Mappings
{
    public class UserMapProfile : Profile
    {
        public sealed class UserMessageConverter : ITypeConverter<UserModel, UserMessage>
        {
            public UserMessage Convert(UserModel source, UserMessage destination, ResolutionContext context)
            {
                ThrowIfNull(nameof(source), source);
                ThrowIfNull(nameof(context), context);

                return new UserMessage()
                {
                    Id = source.Id,
                    FirstName = source.FirstName,
                    LastName = source.LastName,
                    UserName = source.UserName,
                    Password = source.Password
                };
            }
        }

        public sealed class UserModelConverter : ITypeConverter<UserMessage, UserModel>
        {
            public UserModel Convert(UserMessage source, UserModel destination, ResolutionContext context)
            {
                ThrowIfNull(nameof(source), source);
                ThrowIfNull(nameof(context), context);

                return new UserModel()
                {
                    Id = source.Id,
                    FirstName = source.FirstName,
                    LastName = source.LastName,
                    UserName = source.UserName,
                    Password = source.Password
                };
            }
        }

        public UserMapProfile()
        {
            CreateMap<UserModel, UserMessage>(MemberList.Destination).ConvertUsing<UserMessageConverter>();
            CreateMap<UserMessage, UserModel>(MemberList.Destination).ConvertUsing<UserModelConverter>();
        }
    }
}
