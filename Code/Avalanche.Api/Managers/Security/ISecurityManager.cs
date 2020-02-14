using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Security
{
    public interface ISecurityManager
    {
        Task<string> Authenticate(User user);
    }
}
