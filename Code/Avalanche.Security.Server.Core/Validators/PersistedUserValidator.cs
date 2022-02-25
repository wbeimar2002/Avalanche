using Avalanche.Security.Server.Core.Models;
using FluentValidation;

namespace Avalanche.Security.Server.Core.Validators
{
    public class PersistedUserValidator : AbstractValidator<PersistedUserBase>
    {
        public PersistedUserValidator()
        {
            Include(new UserBaseValidator());
            _ = RuleFor(x => x.Id).NotNull().NotEmpty(); // NotEmpty include default value for non-nullable types (e.g. 0 for int)
        }
    }
}
