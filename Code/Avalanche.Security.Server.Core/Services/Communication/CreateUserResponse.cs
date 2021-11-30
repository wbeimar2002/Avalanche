using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Core.Services.Communication
{
    public class CreateUserResponse : BaseResponse
    {
        public UserEntity User { get; private set; }

        public CreateUserResponse(bool success, string message, UserEntity user) : base(success, message)
        {
            User = user;
        }
    }
}