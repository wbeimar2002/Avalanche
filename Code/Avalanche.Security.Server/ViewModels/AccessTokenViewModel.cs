namespace Avalanche.Security.Server.ViewModels
{
    public class AccessTokenViewModel
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public long Expiration { get; set; }
    }
}
