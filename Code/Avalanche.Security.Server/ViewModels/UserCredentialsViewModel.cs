using System.ComponentModel.DataAnnotations;

namespace Avalanche.Security.Server.ViewModels
{
    public class UserCredentialsViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(32)]
        public string Password { get; set; }
    }
}
