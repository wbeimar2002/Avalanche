using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Security
{
    public interface IAuthorizationServiceClient
    {
        Task<bool> AuthenticateUserAsync(User user);
    }
}
