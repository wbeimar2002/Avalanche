using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Entities
{
    public class UserEntity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string LoginName { get; set; }

        [Required]
        [StringLength(64)]
        public string Password { get; set; }

        [StringLength(64)]
        public string FirstName { get; set; }

        [StringLength(64)]
        public string LastName { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new Collection<UserRole>();
    }
}
