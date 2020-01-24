using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
