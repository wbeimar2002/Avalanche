using Avalanche.Security.Server.Core.Models;
using FluentValidation;

namespace Avalanche.Security.Server.Core.Validators
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserModel>
    {
        public UpdateUserValidator()
        {
            Include(new PersistedUserValidator());
            RuleFor(x => x.Password).NotNull().NotEmpty();
        }
    }
}
