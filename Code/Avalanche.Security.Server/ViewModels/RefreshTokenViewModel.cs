using System.ComponentModel.DataAnnotations;

namespace Avalanche.Security.Server.ViewModels
{
    public class RefreshTokenViewModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [StringLength(255)]
        public string UserEmail { get; set; }
    }
}
