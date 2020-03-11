using System.ComponentModel.DataAnnotations;

namespace Avalanche.Security.Server.Controllers.Resources
{
    public class RevokeTokenResource
    {
        [Required]
        public string Token { get; set; }
    }
}