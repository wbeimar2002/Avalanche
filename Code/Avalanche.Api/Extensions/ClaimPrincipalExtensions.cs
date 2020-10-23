using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Avalanche.Api.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ClaimPrincipalExtensions
    {
		public static Avalanche.Shared.Domain.Models.User GetUser(this ClaimsPrincipal claimPrincipal)
		{
			return new Avalanche.Shared.Domain.Models.User()
            {
                Id = claimPrincipal.FindFirst("Id")?.Value,
                FirstName = claimPrincipal.FindFirst("FirstName")?.Value,
                LastName = claimPrincipal.FindFirst("LastName")?.Value,
            };
        }
	}
}
