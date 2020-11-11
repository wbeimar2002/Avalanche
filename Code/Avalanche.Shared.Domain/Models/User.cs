using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class User
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
