using System.ComponentModel.DataAnnotations;

namespace Avalanche.Api.ViewModels
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
