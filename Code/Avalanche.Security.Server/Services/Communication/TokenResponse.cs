using Avalanche.Security.Server.Core.Security.Tokens;

namespace Avalanche.Security.Server.Core.Services.Communication
{
    public class TokenResponse 
    {
        public AccessToken Token { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }

        public TokenResponse(bool success, string message, AccessToken token)
        {
            Token = token;
            Success = success;
            Message = message;
        }
    }
}
