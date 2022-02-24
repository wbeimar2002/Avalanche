using AutoMapper;
using Avalanche.Security.Server.Client.V1.Protos;
using Avalanche.Security.Server.Core.Models;

using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Security.Server.V1.Mappings
{
    public class UserMapProfile : Profile
    {
        public UserMapProfile()
        {
            CreateMap<UserModel, UserMessage>(MemberList.Destination).ConvertUsing<UserMessageConverter>();
            CreateMap<UserMessage, UserModel>(MemberList.Destination).ConvertUsing<UserModelConverter>();
            CreateMap<NewUserMessage, NewUserModel>(MemberList.Destination).ConvertUsing<NewUserModelConverter>();
            CreateMap<UpdateUserMessage, UpdateUserModel>(MemberList.Destination).ConvertUsing<UpdateUserModelConverter>();
            CreateMap<UpdateUserPasswordMessage, UpdateUserPasswordModel>(MemberList.Destination).ConvertUsing<UpdateUserPasswordModelConverter>();
        }

        public sealed class NewUserModelConverter : ITypeConverter<NewUserMessage, NewUserModel>
        {
            public NewUserModel Convert(NewUserMessage source, NewUserModel destination, ResolutionContext context)
            {
                ThrowIfNull(nameof(source), source);
                ThrowIfNull(nameof(context), context);

                return new NewUserModel()
                {
                    FirstName = source.FirstName,
                    LastName = source.LastName,
                    UserName = source.UserName,
                    Password = source.Password
                };
            }
        }

        public sealed class UpdateUserModelConverter : ITypeConverter<UpdateUserMessage, UpdateUserModel>
        {
            public UpdateUserModel Convert(UpdateUserMessage source, UpdateUserModel destination, ResolutionContext context)
            {
                ThrowIfNull(nameof(source), source);
                ThrowIfNull(nameof(context), context);

                return new UpdateUserModel()
                {
                    FirstName = source.FirstName,
                    LastName = source.LastName,
                    UserName = source.UserName
                };
            }
        }

        public sealed class UpdateUserPasswordModelConverter : ITypeConverter<UpdateUserPasswordMessage, UpdateUserPasswordModel>
        {
            public UpdateUserPasswordModel Convert(UpdateUserPasswordMessage source, UpdateUserPasswordModel destination, ResolutionContext context)
            {
                ThrowIfNull(nameof(source), source);
                ThrowIfNull(nameof(context), context);

                return new UpdateUserPasswordModel()
                {
                    Password = source.Password,
                    UserName = source.UserName
                };
            }
        }

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
                    UserName = source.UserName
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
                };
            }
        }
    }
}
