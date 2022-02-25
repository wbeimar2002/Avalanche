using Avalanche.Security.Server.Core.Models;
using FluentValidation;

namespace Avalanche.Security.Server.Core.Validators
{
    public class NewUserValidator : AbstractValidator<NewUserModel>
    {
        public NewUserValidator()
        {
            Include(new UserBaseValidator());
            RuleFor(x => x.Password).NotNull().NotEmpty();
        }
    }
}
