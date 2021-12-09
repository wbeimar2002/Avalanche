using System.ComponentModel.DataAnnotations;

namespace Avalanche.Api.ViewModels
{
    public class RevokeTokenViewModel
    {
        [Required]
        public string Token { get; set; }
    }
}
