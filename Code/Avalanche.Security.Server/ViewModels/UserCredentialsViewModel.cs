using System.ComponentModel.DataAnnotations;

namespace Avalanche.Security.Server.ViewModels
{
    public class UserCredentialsViewModel
    {
        [Required]
        [StringLength(255)]
        public string UserName { get; set; }

        [Required]
        [StringLength(32)]
        public string Password { get; set; }
    }
}
