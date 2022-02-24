using Avalanche.Security.Server.Core.Models;
using FluentValidation;

namespace Avalanche.Security.Server.Core.Validators
{
    public class UpdateUserPasswordValidator : AbstractValidator<UpdateUserPasswordModel>
    {
        public UpdateUserPasswordValidator()
        {
            _ = RuleFor(x => x.UserName).NotNull().NotEmpty().Length(1, 64);
            _ = RuleFor(x => x.Password).NotNull().NotEmpty();
        }
    }
}
