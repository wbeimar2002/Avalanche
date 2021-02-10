using Avalanche.Security.Server.Core.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace Avalanche.Security.Server.Security
{
    public interface IClaimMapper
    {
        IEnumerable<Claim> GetClaims(User user);
    }
}