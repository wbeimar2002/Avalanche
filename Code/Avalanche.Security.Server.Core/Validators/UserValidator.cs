using Avalanche.Security.Server.Core.Models;
using FluentValidation;

namespace Avalanche.Security.Server.Core.Validators
{
    public class UserValidator : AbstractValidator<UserModel>
    {
        public UserValidator()
        {
            Include(new PersistedUserValidator());
            RuleFor(x => x.PasswordHash).NotNull().NotEmpty();
        }
    }
}
