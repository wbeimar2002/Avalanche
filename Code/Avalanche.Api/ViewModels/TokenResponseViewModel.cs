using Avalanche.Api.ViewModels.Security;

namespace Avalanche.Api.ViewModels
{
    public class TokenResponseViewModel
    {
        public AccessToken? Token { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }

        public TokenResponseViewModel(bool success, string? message, AccessToken? token)
        {
            Token = token;
            Success = success;
            Message = message;
        }
    }
}
