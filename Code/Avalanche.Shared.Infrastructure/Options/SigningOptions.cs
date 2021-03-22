using Microsoft.IdentityModel.Tokens;

using System.Security.Cryptography;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Options
{
    public class SigningOptions
    {
        public SecurityKey Key { get; }
        public SigningCredentials SigningCredentials { get; }

        public SigningOptions(string secretKey = null)
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
 