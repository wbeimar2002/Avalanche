using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class SigningConfigurations
    {
        public SecurityKey Key { get; }
        public SigningCredentials SigningCredentials { get; }

        public SigningConfigurations(string secretKey = null)
        {
            if (string.IsNullOrEmpty(secretKey))
            {
                using (var provider = new RSACryptoServiceProvider(2048))
                {
                    Key = new RsaSecurityKey(provider.ExportParameters(true));
                }

                SigningCredentials = new SigningCredentials(Key, SecurityAlgorithms.RsaSha256Signature);
            }
            else
            {
                Key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)); ;
                SigningCredentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256); ;
            }
        }
    }
}
 