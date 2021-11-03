using System.ComponentModel.DataAnnotations;

namespace Avalanche.Security.Server.Controllers.Resources
{
    public class RefreshTokenResource
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [StringLength(128)]
        public string LoginName { get; set; }
    }
}
