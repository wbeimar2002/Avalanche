using Avalanche.Security.Server.Core.Models;
using FluentValidation;

namespace Avalanche.Security.Server.Core.Validators
{
    public class UserBaseValidator : AbstractValidator<UserBase>
    {
        public UserBaseValidator()
        {
            RuleFor(x => x.FirstName).NotNull().NotEmpty().Length(1, 64);
            RuleFor(x => x.LastName).NotNull().NotEmpty().Length(1, 64);
            RuleFor(x => x.UserName).NotNull().NotEmpty().Length(1, 64);
        }
    }
}
