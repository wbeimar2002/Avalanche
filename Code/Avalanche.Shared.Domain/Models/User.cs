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

        //For Configuration Context
        public string DepartmentId { get; set; }
        public string IdnId { get; set; }
        public string SiteId { get; set; }
        public string SystemId { get; set; }
    }
}
