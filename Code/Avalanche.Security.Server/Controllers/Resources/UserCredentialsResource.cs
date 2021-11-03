using System.ComponentModel.DataAnnotations;

namespace Avalanche.Security.Server.Controllers.Resources
{
    public class UserCredentialsResource
    {
        [Required]
        [StringLength(128)]
        public string LoginName { get; set; }

        [Required]
        [StringLength(64)]
        public string Password { get; set; }
    }
}
