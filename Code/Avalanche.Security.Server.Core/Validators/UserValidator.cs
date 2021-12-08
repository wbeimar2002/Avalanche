using FluentValidation;
using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Core.Validators
{
    public class UserValidator : AbstractValidator<UserModel>
    {
        //TODO: Pending Finish Validations
        public UserValidator() => RuleFor(x => x.FirstName).NotEmpty().NotNull().Length(1, 64);
    }
}
