using System.ComponentModel.DataAnnotations.Schema;
using Avalanche.Security.Server.Entities;

namespace Avalanche.Security.Server.Core.Models
{
    [Table("UserRoles")]
    public class UserRole
    {
        public int UserId { get; set; }
        public UserEntity User { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
