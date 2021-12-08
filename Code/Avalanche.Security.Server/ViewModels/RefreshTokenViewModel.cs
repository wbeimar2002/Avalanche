using System.ComponentModel.DataAnnotations;

namespace Avalanche.Security.Server.ViewModels
{
    public class RefreshTokenViewModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [StringLength(255)]
        public string UserName { get; set; }
    }
}
