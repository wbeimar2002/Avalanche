using System.ComponentModel.DataAnnotations;

namespace Avalanche.Security.Server.ViewModels
{
    public class RevokeTokenViewModel
    {
        [Required]
        public string Token { get; set; }
    }
}
