using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Avalanche.Api.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ClaimPrincipalExtensions
    {
		public static Avalanche.Shared.Domain.Models.User GetUser(this ClaimsPrincipal claimPrincipal)
		{
			return claimPrincipal == null ? new Shared.Domain.Models.User()
            {
                Id = "Unidentified",
                FirstName = "Unidentified",
                LastName = "Unidentified",
            }
            : 
        new Avalanche.Shared.Domain.Models.User()
            {
                Id = claimPrincipal.FindFirst("Id")?.Value,
                FirstName = claimPrincipal.FindFirst("FirstName")?.Value,
                LastName = claimPrincipal.FindFirst("LastName")?.Value,
            };
        }
	}
}
